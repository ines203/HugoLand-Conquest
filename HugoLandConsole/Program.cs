using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;

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

        var context = new HugoLandContext(options);

        // Crée la BD et applique le seed
        context.Database.EnsureCreated();

        // Services
        var combatService = new CombatService(context);
        var armyService = new ArmyService(context, combatService);
        var installationService = new InstallationService(context);
        var display = new GameDisplay(context);

        // Menu manager
        var menu = new MenuManager(
            context,
            display,
            armyService,
            installationService,
            combatService
        );

        // Lancer le jeu
        menu.Start();

        // Test pour voir si la base de données est bien créée et accessible, si 16 = fonctionne, si 0 = ne fonctionne pas
        //Console.WriteLine($"Nombre de territoires : {db.Territories.Count()}");
        //Console.ReadLine();
    }
}
