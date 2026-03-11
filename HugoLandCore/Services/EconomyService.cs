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
    public void CollectRevenue()
    {

    }
    public void PayMaintenance()
    {

    }
}
