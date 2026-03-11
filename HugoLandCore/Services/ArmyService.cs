using HugoLandCore.Data;
using HugoLandCore.Models;
using System.Numerics;

namespace HugoLand.Core.Services;

public class ArmyService
{
    private readonly HugoLandContext _context;

    public ArmyService(HugoLandContext context)
    {
        _context = context;
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
    public void TryMove()
    {

    }
    public void TrySplit(int armyId, int direction, int split_qty) //direction en int temporairement, ca va surement etre une enum?
    {
        var army = _context.Detachments.Find(armyId);
        int new_territory_id = 0;
        MilitaryDetachment new_detachment;

        if (army == null)
            return;

        if (army.Force < 11 || split_qty < 10)
            return;
        army.Force -= split_qty;


        switch (direction)
        {
            case 0:
                new_territory_id = army.TerritoryId; // + width
                new_detachment = army.Create(new_territory_id, split_qty, army.Energy);
                break;
            case 1:
                new_territory_id = army.TerritoryId; // + 1
                new_detachment = army.Create(new_territory_id, split_qty, army.Energy);
                break;
            case 2:
                new_territory_id = army.TerritoryId; // - width
                new_detachment = army.Create(new_territory_id, split_qty, army.Energy);
                break;
            case 3:
                new_territory_id = army.TerritoryId; // - 1
                new_detachment = army.Create(new_territory_id, split_qty, army.Energy);
                break;
            default:
                break;
        }

    }
    public void GainEnergy(int armyId)
    {
        var army = _context.Detachments.Find(armyId);

        if (army == null)
            return;

        if (army.Force >= 10)
        {
            army.Energy += 2;
            if (army.Energy > 5)
                army.Energy = 5;
        }
        else
            army.Energy = 5;
    }
}
