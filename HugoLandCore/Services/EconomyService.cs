using HugoLandCore.Data;
using HugoLandCore.Models;

namespace HugoLand.Core.Services;

public class EconomyService
{
    private readonly HugoLandContext _context;

    public EconomyService(HugoLandContext context)
    {
        _context = context;
    }

    // Méthodes
    public void CheckDebt(int playerId)
    {
        var player = _context.Players.Find(playerId);

        if (player == null)
            return;

        if (player.Gold < 0)
        {
            player.DebtTurns++;

            if (player.DebtTurns >= 5)
            {
                // trigger abandon — géré dans MenuManager
            }
        }
        else
        {
            // Réinitialiser le compteur dès que le solde redevient >= 0 (règle du TP)
            player.DebtTurns = 0;
        }

        _context.SaveChanges();
    }

    /// <summary>
    /// Collecte les revenus des fortifications occupées (armée >= 10 soldats).
    /// Doit être appelée AVANT PayMaintenance (règle du TP).
    /// </summary>
    public void CollectRevenue(int playerId)
    {
        var player = _context.Players.Find(playerId);
        if (player == null) return;

        foreach (Installation building in player.Installations)
        {
            // Fortification occupée = fortification + armée >= 10 soldats
            if (building.Type == InstallationType.Fortification
                && building.Territory?.Detachment != null
                && building.Territory.Detachment.PlayerId == playerId
                && building.Territory.Detachment.Force >= 10)
            {
                player.Gold += 5;
            }
        }

        _context.SaveChanges();
    }

    /// <summary>
    /// Déduit l'entretien des armées (Math.Ceiling(Force / 10) or par armée).
    /// Seules les armées >= 10 soldats ont un coût d'entretien.
    /// </summary>
    public void PayMaintenance(int playerId)
    {
        var player = _context.Players.Find(playerId);
        if (player == null) return;

        foreach (MilitaryDetachment detachment in player.Detachments)
        {
            if (detachment.Force >= 10)
                player.Gold -= (int)Math.Ceiling(detachment.Force / 10.0);
        }

        CheckDebt(playerId);
    }
}