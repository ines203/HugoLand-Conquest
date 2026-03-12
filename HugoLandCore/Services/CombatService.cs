using HugoLandCore.Data;
using HugoLandCore.Models;

namespace HugoLand.Core.Services;

public class CombatService
{
    private readonly HugoLandContext _context;
    private Random _rnd;

    public CombatService(HugoLandContext context)
    {
        _context = context;
    }

    // Méthodes

    /// <summary>
    /// Method to make the combat happen
    /// </summary>
    /// <param name="army_self">Attacker Instance</param>
    /// <param name="army_enemy">Defender instance</param>
    public static void ResolveCombat(MilitaryDetachment army_self, MilitaryDetachment army_enemy)
    {
        float eff_force_self = army_self.Force * (_rnd.Next(8, 13) / 10);
        float eff_force_enemy = army_enemy.Force * (_rnd.Next(9, 14) / 10) 
            * army_enemy.Territory.TerrainBonus * army_enemy.Territory.Installation.InstallationBonus;

        if (eff_force_self > eff_force_enemy)
        {
            army_self.Force *= (int)0.8f;
            army_enemy.Force *= (int)0.4f;
        }
        else // inclding ties
        {
            army_self.Force *= (int)0.4f;
            army_enemy.Force *= (int)0.8f;
        }

        army_self.Energy = 0;
        army_enemy.Energy = 0;


        if (army_self.Force > 10)
        {

        }
    }
}
