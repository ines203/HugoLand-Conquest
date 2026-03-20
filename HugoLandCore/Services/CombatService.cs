using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLand.Core.Services;

public class CombatService
{
    private readonly HugoLandContext _context;
    private ArmyService? armyService;
    private InstallationService installationService;
    private Random _rnd = new Random();


    public CombatService(HugoLandContext context, InstallationService insta)
    {
        _context = context;
        installationService = insta;
    }

    // Méthodes
    public void SetArmyService(ArmyService armySvc)
    {
        armyService = armySvc;
    }


    /// <summary>
    /// Method to make the combat happen
    /// </summary>
    /// <param name="army_self">Attacker Instance</param>
    /// <param name="army_enemy">Defender instance</param>
    public void ResolveCombat(MilitaryDetachment army_self, MilitaryDetachment army_enemy, int direction)
    {
        float eff_force_self = army_self.Force * (_rnd.Next(8, 13) / 10f);
        float eff_force_enemy = army_enemy.Force * (_rnd.Next(9, 14) / 10f)
            * army_enemy.Territory.TerrainBonus;
        if (army_enemy.Territory.Installation != null)
            eff_force_enemy *= army_enemy.Territory.Installation.InstallationBonus;
        bool attackerWins = eff_force_self > eff_force_enemy;

        // Apply casualties
        if (attackerWins)
        {
            army_self.Force = (int)(army_self.Force * 0.8f);
            army_enemy.Force = (int)(army_enemy.Force * 0.4f);
        }
        else
        {
            army_self.Force = (int)(army_self.Force * 0.4f);
            army_enemy.Force = (int)(army_enemy.Force * 0.8f);
        }

        army_self.Energy = 0;
        army_enemy.Energy = 0;


        if (attackerWins)
        {
            TryRetreat(army_self, army_enemy, attackerWins, direction);
        }
        else
        {
            TryRetreat(army_enemy, army_self, attackerWins, direction);
        }

        _context.SaveChanges();
    }

    private void TryRetreat(MilitaryDetachment winner, MilitaryDetachment loser, bool attackerWins, int direction)
    {
        if (armyService == null)
            return;

        if (loser.TerritoryId == null || loser.Territory == null)
            return;

        int old_territory_id;

        if (loser.Force < 10)
        {
            _context.Detachments.Remove(loser); // small force cannot survive
            return;
        }

        List<int> directions = new List<int> { 0, 1, 2, 3 };
        directions = directions.OrderBy(x => _rnd.Next()).ToList();

        bool retreated = false;

        if (attackerWins)
        {
            foreach (int dir in directions)
            {
                try
                {
                    armyService.TryMove(loser.Id, direction);
                    foreach (Installation bulding in loser.Player.Installations)
                    {
                        if (bulding.TerritoryId == loser.TerritoryId && bulding.Type == InstallationType.Camp)
                            installationService.CheckDestructionCamp(bulding);
                    }
                    retreated = true;
                    break; // success
                }
                catch
                {
                    continue;
                }
            }
        }
        else //defenders won
        {
            switch (direction)
            {
                case 0:
                    old_territory_id = loser.TerritoryId!.Value + loser.Player.Game.Map.Width;
                    break;
                case 1:
                    old_territory_id = loser.TerritoryId!.Value - 1;
                    break;
                case 2:
                    old_territory_id = loser.TerritoryId!.Value - loser.Player.Game.Map.Width;
                    break;
                case 3:
                    old_territory_id = loser.TerritoryId!.Value + 1;
                    break;
                default:
                    old_territory_id = loser.TerritoryId!.Value;
                    break;
            }

            armyService.TryMove(loser.Id, direction);
            retreated = true;
        }

        if (!retreated)
        {
            _context.Detachments.Remove(loser); // destruction
        }
    }
}

