using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLandCore.IA
{
    public class NormalAi : IAI
    {
        private readonly HugoLandContext _context;
        private Player _player;
        private Player _enemyPlayer;
        private ArmyService _armyService;
        private InstallationService _installationService;
        private Random _rng;

        public NormalAi(HugoLandContext context, Player player, Player enemyPlayer, ArmyService armyService, InstallationService installationService)
        {
            _context = context;
            _player = player;
            _enemyPlayer = enemyPlayer;
            _armyService = armyService;
            _rng = new Random();
            _installationService = installationService;
        }

        public void PlayTurn()
        {
            foreach (MilitaryDetachment army in _player.Detachments)
            {
                if (army.Force >= 10)
                {
                    int iChoice = _rng.Next(0, 5);
                    switch (iChoice)
                    {
                        case 0: // Move
                            MoveArmy(army);
                            break;
                        case 1: // Build camp
                            if (_player.Gold >= 20)
                                _installationService.BuildCamp(army);
                            else
                                MoveArmy(army);
                            break;
                        case 2: // Fortify
                            if (_player.Gold >= 50)
                                _installationService.UpgradeToFort(army);
                            else
                                MoveArmy(army);
                            break;
                        case 3: // Reinforce
                            if (_player.Gold >= 4)
                            {
                                int iReinforce = _rng.Next(1, (_player.Gold / 2));
                                _armyService.ReinforceArmy(_player, army, iReinforce);
                            }
                            else
                                MoveArmy(army);
                            break;
                        case 4: // Split
                            if (army.Force > 10)
                            {
                                int iSplit = _rng.Next(10, army.Force);
                                int iDirectionSplit = _rng.Next(0, 4);
                                _armyService.TrySplit(army.Id, iDirectionSplit, iSplit);
                            }
                            else
                                MoveArmy(army);
                            break;
                    }
                }
            }
        }
        private void MoveArmy(MilitaryDetachment army)
        {
            int iDirection = 5;
            int iClosestDistance = int.MaxValue;
            MilitaryDetachment closestEnArmy = null;

            foreach (MilitaryDetachment enemyArmy in _enemyPlayer.Detachments)
            {
                int iDistance = Math.Abs(
                    (army.Territory.X - enemyArmy.Territory.X) + 
                    (army.Territory.Y - enemyArmy.Territory.Y));

                if (iDistance <= iClosestDistance)
                {
                    closestEnArmy = enemyArmy;
                    iClosestDistance = iDistance;
                }
            }

            if (iClosestDistance == int.MaxValue)
                return;// No enemy army found

            if (army.Territory.X > closestEnArmy.Territory.X)
                iDirection = 3; // West

            else if (army.Territory.X < closestEnArmy.Territory.X)
                iDirection = 1; // Est

            else if (army.Territory.Y > closestEnArmy.Territory.Y)
                iDirection = 0; // North

            else if (army.Territory.Y < closestEnArmy.Territory.Y)
                iDirection = 2; // South

            _armyService.TryMove(army.Id, iDirection);
        }
    }
}
