using HugoLandCore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HugoLandCore.Data;

/// <summary>
/// Factory utilisée par EF Core Tools au design-time (migrations).
/// </summary>
public class HugoLandContextFactory : IDesignTimeDbContextFactory<HugoLandContext>
{
    public HugoLandContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<HugoLandContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=HugoLandDB;Trusted_Connection=True;")
            .Options;

        return new HugoLandContext(options);
    }
}
