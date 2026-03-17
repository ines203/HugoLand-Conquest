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
    }
}
