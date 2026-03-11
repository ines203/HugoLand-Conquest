using HugoLand.Core.Services;
using HugoLandCore.Models;

namespace HugoLandConsole;

/// <summary>
/// Description : Point d'entrée de l'application console pour HugoLand.
/// Date : 2026-03-10
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        //Test de L'affichage
        GameDisplay.DisplayMap(player: true, turn: 7, gold: 100, fortif: 5, troops: 20);

        //Section donné dans l'énoncé

        //Console.Write("Direction (N/S/E/O) : ");
        //var dir = Console.ReadLine();
        //var dest = /* trouver le territoire adjacent */;

        //if (ArmyService.TryMove(army, dest))
        //    Console.WriteLine("Déplacement réussi.");
        //else
        //    Console.WriteLine("Déplacement impossible.");

        //Fin de section donné dans l'énoncé
    }
}
