using HugoLandCore.Models;

namespace HugoLandCore.Models;

public enum InstallationType
{
    Camp = 0,
    Fortification = 1
}

public class Installation
{
    public int Id { get; set; }
    public InstallationType Type { get; set; }

    public int TerritoryId { get; set; }
    public Territory? Territory { get; set; }
}
