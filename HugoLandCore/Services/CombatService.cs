using HugoLand.Core.Data;
using HugoLand.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
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
}
