using HugoLandCore.Data;
using HugoLandCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLandCore.Services
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
        private void SaveChanges()
        {

        }
    }
}
