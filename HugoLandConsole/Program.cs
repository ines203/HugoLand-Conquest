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

        // si besoin vider BD
        // context.Database.EnsureDeleted();

        // Crée la BD et applique le seed
        context.Database.EnsureCreated();

        // Services
        var installationService = new InstallationService(context);
        var combatService = new CombatService(context, installationService);
        var armyService = new ArmyService(context, combatService);
        combatService.SetArmyService(armyService);
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
