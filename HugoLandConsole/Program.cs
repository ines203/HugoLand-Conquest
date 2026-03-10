using HugoLand.Core.Services;
using HugoLand.Core.Models;

namespace HugoLandConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {



            //Section donné dans l'énoncé
            Console.Write("Direction (N/S/E/O) : ");
            var dir = Console.ReadLine();
            var dest = /* trouver le territoire adjacent */;

            if (ArmyService.TryMove(army, dest))
                Console.WriteLine("Déplacement réussi.");
            else
                Console.WriteLine("Déplacement impossible.");
            //Fin de section donné dans l'énoncé
        }
    }
}
