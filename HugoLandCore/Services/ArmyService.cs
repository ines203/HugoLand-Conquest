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

        if (army == null || army.Force < 11 || split_qty < 10)
            return;
        army.Force -= split_qty;


        switch (direction)
        {
            case 0:
                new_territory_id = army.TerritoryId; // + width
                break;
            case 1:
                new_territory_id = army.TerritoryId; // + 1
                break;
            case 2:
                new_territory_id = army.TerritoryId; // - width
                break;
            case 3:
                new_territory_id = army.TerritoryId; // - 1
                break;
            default:
                return;
                break;
        }

        army.Actionable = false;
        new_detachment = MilitaryDetachment.Create(new_territory_id, split_qty, army.Energy, false);
        new_detachment.Energy -= new_detachment.Territory.TerrainEnergyCost;

        foreach (MilitaryDetachment detach in army.Player.Detachments)
        {
            //verify for ally troops
            if (new_detachment.TerritoryId == detach.TerritoryId)
            {
                detach.Force += new_detachment.Force;
                detach.Energy = new_detachment.Energy <= detach.Energy ? new_detachment.Energy : detach.Energy;
                //delete new_detachement object
            }
        }

        //verify pour combat

    }
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
            
    }
}
