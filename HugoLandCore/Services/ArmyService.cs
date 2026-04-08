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

    public void TryMove(int armyId, int direction)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null || army.Force < 10)
            return;

        // Calculer le territoire cible
        int width = army.Player!.Game!.Map!.Width;
        int new_territory_id = direction switch
        {
            0 => army.TerritoryId!.Value - width, // N
            1 => army.TerritoryId!.Value + 1,     // E
            2 => army.TerritoryId!.Value + width, // S
            3 => army.TerritoryId!.Value - 1,     // O
            _ => army.TerritoryId!.Value
        };

        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null)
            return;

        // Vérifier l'énergie disponible
        if (army.Energy < new_territory.TerrainEnergyCost)
            return;

        // Trouver l'ennemi
        Player? enemy = army.Player.Game.Players
            .FirstOrDefault(p => p.Id != army.PlayerId);

        var old_territory = army.Territory;

        // Détacher l'ancienne case
        if (army.Territory != null)
            army.Territory.Detachment = null;

        // Attacher la nouvelle case
        army.Territory = new_territory;
        army.TerritoryId = new_territory_id;
        new_territory.Detachment = army;

        // Fusion avec allié sur la même case
        foreach (MilitaryDetachment ally in army.Player.Detachments.ToList())
        {
            if (ally.TerritoryId == new_territory_id && ally.Id != army.Id)
            {
                TryMerge(army);
                break;
            }
        }

        // Combat avec ennemi
        if (enemy != null)
        {
            foreach (MilitaryDetachment enemy_detachment in enemy.Detachments.ToList())
            {
                if (enemy_detachment.TerritoryId == new_territory_id)
                {
                    combatService.ResolveCombat(army, enemy_detachment, direction);
                    break;
                }
            }
        }

        // Capture d'une fortification ennemie non défendue
        if (enemy != null)
        {
            foreach (Installation building in enemy.Installations.ToList())
            {
                if (building.TerritoryId == new_territory_id)
                {
                    // Vérifier qu'il n'y a plus d'ennemi sur le territoire
                    bool hasEnemyArmy = enemy.Detachments
                        .Any(d => d.TerritoryId == new_territory_id);

                    if (!hasEnemyArmy)
                    {
                        building.PlayerId = army.PlayerId;
                        building.Player = army.Player;
                        building.IsOccupied = true;
                    }
                }
            }
        }

        // Déduire l'énergie
        army.Energy -= new_territory.TerrainEnergyCost;
        army.Actionable = false;

        // Détruire le camp si l'armée quitte l'ancien territoire
        if (old_territory != null)
            installationService.CheckDestructionCamp(old_territory.Installation);

        _context.SaveChanges();
    }

    public void TrySplit(int armyId, int direction, int split_qty)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null)
            return;

        // L'armée d'origine doit avoir >= 11 soldats
        if (army.Force < 11)
            return;

        // Le groupe qui part doit former une armée (>= 10 soldats)
        if (split_qty < 10)
            return;

        // Il doit rester au moins 1 soldat
        if (army.Force - split_qty < 1)
            return;

        int width = army.Player!.Game!.Map!.Width;
        int new_territory_id = direction switch
        {
            0 => army.TerritoryId!.Value - width,
            1 => army.TerritoryId!.Value + 1,
            2 => army.TerritoryId!.Value + width,
            3 => army.TerritoryId!.Value - 1,
            _ => army.TerritoryId!.Value
        };

        var new_territory = _context.Territories.Find(new_territory_id);
        if (new_territory == null)
            return;

        // Vérifier l'énergie
        if (army.Energy < new_territory.TerrainEnergyCost)
            return;

        // Déduire les soldats de l'armée d'origine
        army.Force -= split_qty;

        // Créer le nouveau détachement avec l'énergie de l'armée d'origine
        var new_detachment = MilitaryDetachment.Create(new_territory_id, split_qty, army.Energy, false);
        new_detachment.PlayerId = army.PlayerId;
        new_detachment.Player = army.Player;
        new_detachment.Territory = new_territory;

        // Déduire l'énergie du déplacement
        new_detachment.Energy -= new_territory.TerrainEnergyCost;

        new_territory.Detachment = new_detachment;
        _context.Detachments.Add(new_detachment);

        // Marquer l'armée d'origine comme ayant agi
        army.Actionable = false;

        _context.SaveChanges();

        // Fusion si allié présent
        foreach (MilitaryDetachment ally in army.Player.Detachments.ToList())
        {
            if (ally.TerritoryId == new_territory_id && ally.Id != new_detachment.Id)
            {
                TryMerge(new_detachment);
                break;
            }
        }

        // Combat si ennemi présent
        Player? enemy = army.Player.Game.Players
            .FirstOrDefault(p => p.Id != army.PlayerId);

        if (enemy != null)
        {
            foreach (MilitaryDetachment enemy_detachment in enemy.Detachments.ToList())
            {
                if (enemy_detachment.TerritoryId == new_territory_id)
                {
                    combatService.ResolveCombat(new_detachment, enemy_detachment, direction);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Récupération d'énergie en début de tour (+2, max 5).
    /// Seules les armées >= 10 soldats sont actionnables.
    /// </summary>
    public void GainEnergy(int armyId)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null)
            return;

        if (army.Force >= 10)
        {
            army.Actionable = true;
            army.Energy = Math.Min(5, army.Energy + 2);
        }
        else
        {
            // Détachement isolé : énergie fixée à 5, non actionnable
            army.Energy = 5;
            army.Actionable = false;
        }

        _context.SaveChanges();
    }

    /// <summary>
    /// Fusionne les alliés sur le même territoire.
    /// Force = somme, Énergie = minimum des deux.
    /// </summary>
    public void TryMerge(MilitaryDetachment army)
    {
        var allies = army.Player!.Detachments
            .Where(d => d.TerritoryId == army.TerritoryId && d.Id != army.Id)
            .ToList();

        foreach (var target in allies)
        {
            army.Force += target.Force;
            army.Energy = Math.Min(army.Energy, target.Energy); // règle : minimum des deux

            if (target.Territory != null)
                target.Territory.Detachment = null;

            target.TerritoryId = null;
            _context.Detachments.Remove(target);
        }

        _context.SaveChanges();
    }

    public void ReinforceArmy(Player player, MilitaryDetachment army, int amount)
    {
        int cost = amount * 2;

        // Requiert une armée >= 10 soldats
        if (army.Force < 10)
            return;

        // Requiert une installation
        if (army.Territory?.Installation == null)
            return;

        // Vérifier l'or
        if (player.Gold < cost)
            return;

        army.Force += amount;
        player.Gold -= cost;
        army.Actionable = false;

        _context.SaveChanges();
    }
}