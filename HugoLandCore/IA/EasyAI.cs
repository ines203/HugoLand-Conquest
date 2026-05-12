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
    public class EasyAI : IAI
    {
        private readonly HugoLandContext _context;
        private Player _player;
        private ArmyService _armyService;
        private InstallationService _installationService;
        private Random _rng;

        public EasyAI(HugoLandContext context, Player player, ArmyService armyService, InstallationService installationService)
        {
            _context = context;
            _player = player;
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
                    int iChoice = _rng.Next(0, 6);
                    switch (iChoice)
                    {
                        case 0: // Move
                            int iDirectionMove = _rng.Next(0, 4);
                            _armyService.TryMove(army.Id, iDirectionMove);
                            break;
                        case 1: // Build camp
                            _installationService.BuildCamp(army);
                            break;
                        case 2: // Fortify
                            _installationService.UpgradeToFort(army);
                            break;
                        case 3: // Reinforce
                            if (_player.Gold >= 4)
                            {
                                int iReinforce = _rng.Next(1, (_player.Gold / 2));
                                _armyService.ReinforceArmy(_player, army, iReinforce);
                            }
                            break;
                        case 4: // Split
                            if (army.Force > 10)
                            {
                                int iSplit = _rng.Next(10, army.Force);
                                int iDirectionSplit = _rng.Next(0, 4);
                                _armyService.TrySplit(army.Id, iDirectionSplit, iSplit);
                            }
                            break;
                        case 5: // Nothing
                            break;
                    }
                }

            }
        }
    }
}
