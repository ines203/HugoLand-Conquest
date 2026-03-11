using HugoLandCore.Data;
using HugoLandCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLandCore.Services
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
        private void SaveChanges()
        {

        }
    }
}
