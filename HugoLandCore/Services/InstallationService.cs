using HugoLandCore.Data;
using HugoLandCore.Models;

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
        if (detachment.Territory.Installation != null)
            return; // message d'erreur "déjà une installation"

        if (detachment.Player.Gold < 20)
            return; // message erreur "pas assez d'or (20 requis)"

        detachment.Player.Gold -= 20; // déduire le coût

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

        // Historique
        _context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Build camp at ({detachment.Territory.X},{detachment.Territory.Y}) - cost 20 gold",
            Timestamp = DateTime.Now,
            PlayerId = detachment.Player.Id
        });

        _context.SaveChanges();
    }

    public void UpgradeToFort(MilitaryDetachment detachment)
    {
        if (detachment.Territory.Installation == null
            || detachment.Territory.Installation.Type != InstallationType.Camp)
            return; // message d'erreur "pas de camp ici"

        if (detachment.Player.Gold < 50)
            return; // message erreur "pas assez d'or (50 requis)"

        detachment.Player.Gold -= 50;
        detachment.Territory.Installation.Type = InstallationType.Fortification;
        detachment.Actionable = false;

        // Historique
        _context.PlayerActions.Add(new HugoLandCore.Models.PlayerAction
        {
            Description = $"Upgrade to fortification at ({detachment.Territory.X},{detachment.Territory.Y}) - cost 50 gold",
            Timestamp = DateTime.Now,
            PlayerId = detachment.Player.Id
        });

        _context.SaveChanges();
    }

    public void CheckDestructionCamp(Installation? building)
    {
        if (building != null && building.Type == InstallationType.Camp)
        {
            if (building.Territory?.Detachment == null)
            {
                _context.Installations.Remove(building);
                _context.SaveChanges();
            }
        }
    }
}
