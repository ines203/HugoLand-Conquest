using HugoLandCore.Data;
using HugoLandCore.Models;

namespace HugoLand.Core.Services;

public class ArmyService
{
    private readonly HugoLandContext _context;
    private CombatService combatService;

    public ArmyService(HugoLandContext context, CombatService combat)
    {
        _context = context;
        combatService = combat;
    }


    //Section donné dans l'énoncé

    //public bool TryMove(MilitaryDetachment army, Territory destination)
    //{
    //    if (army.Force < 10) return false;             // Détachement isolé
    //    if (army.Energy < destination.MovementCost) return false;

    //    army.TerritoryId = destination.Id;
    //    army.Energy -= destination.MovementCost;
    //    _context.SaveChanges();
    //    return true;
    //}

    //Fin de section donné dans l'énoncé

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
                new_territory_id = army.TerritoryId - army.Player.Game.Map.Width;
                break;
            case 1:
                new_territory_id = army.TerritoryId + 1;
                break;
            case 2:
                new_territory_id = army.TerritoryId + army.Player.Game.Map.Width;
                break;
            case 3:
                new_territory_id = army.TerritoryId - 1;
                break;
            default:
                new_territory_id = army.TerritoryId;
                break;
        }


        foreach (Installation building in enemy.Installations)
        {
            if (building.TerritoryId == new_territory_id && !building.IsOccupied)
            {
                building.PlayerId = army.PlayerId;
                building.IsOccupied = true;
            }
        }

        foreach (MilitaryDetachment allies in army.Player.Detachments)
        {
            if (allies.TerritoryId == new_territory_id)
            {
                TryMerge(army);
            }
        }

        foreach (MilitaryDetachment enemy_detachment in enemy.Detachments)
        {
            if (enemy_detachment.TerritoryId == new_territory_id)
            {
                combatService.ResolveCombat(army, enemy_detachment, direction);
            }
        }

        var new_territory = _context.Territories.Find(new_territory_id);

        army.Energy -= new_territory.TerrainEnergyCost;
        army.Actionable = false;
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
                new_territory_id = army.TerritoryId - army.Player.Game.Map.Width;
                break;
            case 1:
                new_territory_id = army.TerritoryId + 1;
                break;
            case 2:
                new_territory_id = army.TerritoryId + army.Player.Game.Map.Width;
                break;
            case 3:
                new_territory_id = army.TerritoryId - 1;
                break;
            default:
                new_territory_id = army.TerritoryId;
                break; ;
        }

        new_detachment = MilitaryDetachment.Create(new_territory_id, split_qty, army.Energy, false);
        new_detachment.Energy -= new_detachment.Territory.TerrainEnergyCost;
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

        foreach (MilitaryDetachment target in army.Player.Detachments)
        {
            //verify for ally troops
            if (target.TerritoryId == army.TerritoryId)
            {
                army.Force += target.Force;
                army.Energy = (target.Energy <= army.Energy) ? target.Energy : army.Energy;

                //delete army object
                _context.Detachments.Remove(target); 
                _context.SaveChanges();
            }
        }
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
