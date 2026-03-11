using HugoLandCore.Data;
using Microsoft.EntityFrameworkCore;
using HugoLandCore.Services;
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
        var options = new DbContextOptionsBuilder<HugoLandContext>()
        .UseInMemoryDatabase("HugoLandDB")
        .Options;

        var db = new HugoLandContext(options);

        db.Database.EnsureCreated();

        // Test pour voir si la base de données est bien créée et accessible, si 16 = fonctionne, si 0 = ne fonctionne pas
        Console.WriteLine($"Nombre de territoires : {db.Territories.Count()}");
        Console.ReadLine();

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
