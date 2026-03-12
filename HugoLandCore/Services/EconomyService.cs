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
                // trigger abandon
            }
        }
    }
    public void CollectRevenue(int playerId)
    {
        var player = _context.Players.Find(playerId);

        foreach (Installation building in player.Installations)
        {
            if (building.IsOccupied && building.Type == InstallationType.Fortification)
                player.Gold += 5;
        }
    }
    public void PayMaintenance(int playerId)
    {
        var player = _context.Players.Find(playerId);

        foreach (MilitaryDetachment detachement in player.Detachments)
        {
            if (detachement.Force >= 10)
                player.Gold -= (int)Math.Ceiling((float)(detachement.Force / 10));
        }

        CheckDebt(playerId);
    }
}
