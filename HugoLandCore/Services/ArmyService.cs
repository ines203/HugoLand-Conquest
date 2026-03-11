using HugoLand.Core.Data;
using HugoLand.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLand.Core.Services
{
    public class ArmyService
    {
        private readonly HugoLandContext _context;

        public ArmyService(HugoLandContext context)
        {
            _context = context;
        }

        //Section donné dans l'énoncé

        //public bool TryMove(MilitaryDetachment army, Territory destination)
        //{
        //    if (army.Force < 10) return false;             // Détachement isolé
        //    if (army.Energy < destination.MovementCost) return false;

        //    army.TerritoryId = destination.Id;
        //    army.Energy -= destination.MovementCost;
        //    _context.SaveChanges();
        //    return true;
        //}

        //Fin de section donné dans l'énoncé

        // Méthodes
        public void TryMove()
        {

        }
        public void TrySplit()
        {

        }
    }
}
