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
        private ArmyService _armyService;
        private InstallationService _installationService;
        private Random _rng;

        public NormalAi(HugoLandContext context, Player player, ArmyService armyService, InstallationService installationService)
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

                }
            }
        }
    }
}
