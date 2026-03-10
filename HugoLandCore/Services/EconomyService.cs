using HugoLand.Core.Data;
using HugoLand.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
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
}
