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

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var options = new DbContextOptionsBuilder<HugoLandContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HugoLandDB;Trusted_Connection=True;")
            .Options;

        Context = new HugoLandContext(options);
        Context.Database.Migrate();

        // Services
        InstallationService = new InstallationService(Context);
        EconomyService = new EconomyService(Context);
        CombatService = new CombatService(Context, InstallationService);
        ArmyService = new ArmyService(Context, CombatService, InstallationService);
        CombatService.SetArmyService(ArmyService);
        WorldService = new WorldTemplateService(Context);

        // Seed les 3 mondes requis si pas encore créés
        SeedWorlds.EnsureWorlds(Context);

        // Musique menu au démarrage
        MusicManager.Volume = 0.4;
        MusicManager.PlayMenu();
    }
}
