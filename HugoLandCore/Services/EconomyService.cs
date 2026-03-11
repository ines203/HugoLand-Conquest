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
    public void CollectRevenue()
    {

    }
    public void PayMaintenance()
    {

    }
}
