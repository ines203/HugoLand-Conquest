using HugoLand.Core.Data;
using HugoLandCore.Models;

namespace HugoLand.Core.Services;

public class CombatService
{
    private readonly HugoLandContext _context;

    public CombatService(HugoLandContext context)
    {
        _context = context;
    }

    // Méthodes
    public void ResolveCombat()
    {

    }
}
