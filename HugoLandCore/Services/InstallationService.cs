using HugoLandCore.Data;
using HugoLandCore.Models;
using System.Net.Mail;

namespace HugoLandCore.Services;

public class InstallationService
{
    private readonly HugoLandContext _context;

    public InstallationService(HugoLandContext context)
    {
        _context = context;
    }

    // Méthodes
    public void BuildCamp(MilitaryDetachment detachment)
    {
        if (detachment.Territory.Installation == null)
        {
            if (detachment.Player.Gold > 20)
            {
                detachment.Territory.Installation = new Installation
                {
                    Type = InstallationType.Camp,
                    Player = detachment.Player,
                    PlayerId = detachment.Player.Id,
                    IsOccupied = true,
                    Territory = detachment.Territory,
                    TerritoryId = detachment.TerritoryId!.Value
                };
                detachment.Actionable = false;
            }
            else return; //message erreur "pas assez d'or"
        }
        else
            return; //message d'erreur "déja installation"
    }
    public void UpgradeToFort(MilitaryDetachment detachment)
    {
        if (detachment.Territory.Installation != null && detachment.Territory.Installation.Type == InstallationType.Camp)// ERROR
        {
            if (detachment.Player.Gold > 50)
            {
                detachment.Territory.Installation.Type = InstallationType.Fortification;
                detachment.Actionable = false;
            }
            else return; //message erreur "pas assez d'or"
        }
        else
            return; //message d'erreur "pas de camp ici"
    }

    public void CheckDestructionCamp(Installation building)
    {
        if (building != null && building.Type == InstallationType.Camp)
        {
            //if (!building.IsOccupied)
            if (building.Territory.Detachment == null)
            {
                _context.Installations.Remove(building);
                _context.SaveChanges();
            }
        }
    }
}
