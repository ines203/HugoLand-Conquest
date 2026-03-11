using HugoLandCore.Data;
using HugoLandCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HugoLandCore.Services
{
    public class GameService
    {
        private const byte MapWidth = 15;
        private const byte MapHeight = 10;
        private readonly HugoLandContext _context;

        public GameService(HugoLandContext context)
        {
            _context = context;
        }

        // Méthodes
        public void CreateGame()
        {

        }
        public void LoadGame()
        {

        }
        public void EndTurn()
        {

        }
        private void SaveChanges()
        {

        }
    }
}
