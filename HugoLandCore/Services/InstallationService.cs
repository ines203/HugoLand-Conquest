using HugoLandCore.Data;
using HugoLandCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLandCore.Services
{
    public class InstallationService
    {
        private readonly HugoLandContext _context;

        public InstallationService(HugoLandContext context)
        {
            _context = context;
        }

        // Méthodes
        public void BuildCamp()
        {

        }
        public void UpgradeToFort()
        {

        }
        private void SaveChanges()
        {

        }
    }
}
