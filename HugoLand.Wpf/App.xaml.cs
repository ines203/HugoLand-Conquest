using HugoLandCore.Data;
using HugoLandCore.Services;
using HugoLand.Core.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace HugoLandWpf;

public partial class App : Application
{
    public static HugoLandContext Context { get; private set; } = null!;
    public static WorldTemplateService WorldService { get; private set; } = null!;
    public static InstallationService InstallationService { get; private set; } = null!;
    public static EconomyService EconomyService { get; private set; } = null!;
    public static CombatService CombatService { get; private set; } = null!;
    public static ArmyService ArmyService { get; private set; } = null!;
    public static StatisticsService StatisticsService { get; private set; } = null!;

    // Indique si le joueur 2 est une IA (défini à la création de partie)
    public static bool IsPlayer2AI { get; set; } = false;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var options = new DbContextOptionsBuilder<HugoLandContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HugoLandDB;Trusted_Connection=True;")
                .Options;

            Context = new HugoLandContext(options);

            // Si la DB existe déjà avec les tables, Migrate() plante.
            // On supprime et recrée proprement si nécessaire.
            try
            {
                Context.Database.Migrate();
            }
            catch
            {
                // La DB existe déjà mais les migrations sont en conflit :
                // on la recrée proprement.
                Context.Database.EnsureDeleted();
                Context.Database.EnsureCreated();
            }

            // Services
            InstallationService = new InstallationService(Context);
            EconomyService = new EconomyService(Context);
            CombatService = new CombatService(Context, InstallationService);
            ArmyService = new ArmyService(Context, CombatService, InstallationService);
            CombatService.SetArmyService(ArmyService);
            WorldService = new WorldTemplateService(Context);
            StatisticsService = new StatisticsService(Context);

            // Seed les 3 mondes requis si pas encore créés
            SeedWorlds.EnsureWorlds(Context);

            // Musique menu au démarrage
            MusicManager.Volume = 0.4;
            MusicManager.PlayMenu();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup error:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "HugoLand — Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}
