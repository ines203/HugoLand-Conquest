using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLand.Core.Services;

public class ArmyService
{
    private readonly HugoLandContext _context;
    private CombatService combatService;
    private InstallationService installationService;

    public ArmyService(HugoLandContext context, CombatService combat, InstallationService installation)
    {
        _context = context;
        combatService = combat;
        installationService = installation;
    }

    // Méthodes
    public void TryMove(int armyId, int direction)
    {
        var army = _context.Detachments.Find(armyId);
        int new_territory_id = 0;
        Player enemy = null;

        if (army == null || army.Force < 11)
            return;

        foreach (Player player in army.Player.Game.Players)
        {
            if (player.Id != army.PlayerId)
                enemy = player;
        }

        switch (direction)
        {
            case 0:
                new_territory_id = army.TerritoryId!.Value - army.Player.Game.Map.Width;
                break;
            case 1:
                new_territory_id = army.TerritoryId!.Value + 1;
                break;
            case 2:
                new_territory_id = army.TerritoryId!.Value + army.Player.Game.Map.Width;
                break;
            case 3:
                new_territory_id = army.TerritoryId!.Value - 1;
                break;
            default:
                new_territory_id = army.TerritoryId!.Value;
                break;
        }

        var old_territory = army.Territory;
        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null)
            return;

        // Détacher l’ancienne case
        if (army.Territory != null)
        {
            army.Territory.Detachment = null;
        }

        // Attacher la nouvelle case avant merge
        army.Territory = new_territory;
        army.TerritoryId = new_territory_id;
        new_territory.Detachment = army;

        // Maintenant que l’armée est sur la case puis merge
        foreach (MilitaryDetachment allies in army.Player.Detachments.ToList())
        {
            if (allies.TerritoryId == new_territory_id && allies.Id != army.Id)
            {
                TryMerge(army);
                break;
            }
        }

        // Combat
        foreach (MilitaryDetachment enemy_detachment in enemy.Detachments.ToList())
        {
            if (enemy_detachment.TerritoryId == new_territory_id)
            {
                combatService.ResolveCombat(army, enemy_detachment, direction);
            }
        }

        // Bâtiments
        foreach (Installation building in enemy.Installations)
        {
            if (building.TerritoryId == new_territory_id && !building.IsOccupied)
            {
                building.PlayerId = army.PlayerId;
                building.IsOccupied = true;
            }
        }

        army.Energy -= new_territory.TerrainEnergyCost;
        army.Actionable = false;

        installationService.CheckDestructionCamp(old_territory.Installation);

        _context.SaveChanges();
    }



    public void TrySplit(int armyId, int direction, int split_qty) //direction en int temporairement, ca va surement etre une enum?
    {
        Player enemy = null;
        var army = _context.Detachments.Find(armyId);
        MilitaryDetachment new_detachment;
        int new_territory_id = 0;

        if (split_qty < 10)
            return;
        army.Force -= split_qty;

        switch (direction)
        {
            case 0:
                new_territory_id = army.TerritoryId!.Value - army.Player.Game.Map.Width;
                break;
            case 1:
                new_territory_id = army.TerritoryId!.Value + 1;
                break;
            case 2:
                new_territory_id = army.TerritoryId!.Value + army.Player.Game.Map.Width;
                break;
            case 3:
                new_territory_id = army.TerritoryId!.Value - 1;
                break;
            default:
                new_territory_id = army.TerritoryId!.Value;
                break;
        }


        // si en dehors de la map
        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null)
            return;

        // créer le nouveau détachement
        new_detachment = MilitaryDetachment.Create(new_territory_id, split_qty, army.Energy, false);

        // assigner le joueur
        new_detachment.PlayerId = army.PlayerId;
        new_detachment.Player = army.Player;

        // assigner le territoire
        new_territory.Detachment = new_detachment;

        // ajouter dans la BD
        _context.Detachments.Add(new_detachment);

        // énergie
        new_detachment.Energy -= new_territory.TerrainEnergyCost;

        _context.SaveChanges();


        foreach (Player player in army.Player.Game.Players)
        {
            if (player.Id != army.PlayerId)
                enemy = player;
        }

        foreach (MilitaryDetachment enemy_detachment in enemy.Detachments)
        {
            if (enemy_detachment.TerritoryId == new_territory_id)
            {
                combatService.ResolveCombat(army, enemy_detachment, direction);
            }
        }

    }

    /// <summary>
    /// Au debut du tour, s'occupe de fair gagner energie a tt le monde
    /// </summary>
    /// <param name="armyId"></param>
    public void GainEnergy(int armyId)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null)
            return;

        if (army.Force >= 10)
        {
            army.Actionable = true;
            army.Energy += 2;
            if (army.Energy > 5)
                army.Energy = 5;
        }
        else
        {
            army.Energy = 5;
            army.Actionable = false;
        }

        _context.SaveChanges();
    }

    public void TryMerge(MilitaryDetachment army)
    {
        // Cherche un allié sur le même territoire
        var allies = army.Player.Detachments
            .Where(d => d.TerritoryId == army.TerritoryId && d.Id != army.Id)
            .ToList();

        foreach (var target in allies)
        {
            // Ajouter la force avant de supprimer
            army.Force += target.Force;

            // Détacher proprement l’armée absorbée
            if (target.Territory != null)
                target.Territory.Detachment = null;

            target.TerritoryId = null;

            // Supprimer l’armée absorbée
            _context.Detachments.Remove(target);
        }

        _context.SaveChanges();
    }

    public void ReinforceArmy(Player player, MilitaryDetachment army, int amount)
    {
        int cost = amount * 2;

        // Vérifier installation
        if (army.Territory.Installation == null)
            return;

        // Vérifier or
        if (player.Gold < cost)
            return;

        // Appliquer renforcement
        army.Force += amount;
        player.Gold -= cost;

        // L’armée a agi
        army.Actionable = false;

        _context.SaveChanges();
    }

}
