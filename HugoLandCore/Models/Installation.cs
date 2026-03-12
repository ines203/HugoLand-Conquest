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
    public float InstallationBonus
    {
        get
        {
            switch (Type)
            {
                case InstallationType.Camp:
                    return 1.1f;
                case InstallationType.Fortification:
                    return 1.2f;
                default:
                    return 1;
            }
        }
    }

    public int TerritoryId { get; set; }
    public Territory? Territory { get; set; }
    public bool PlayerId { get; set; }
    public Player? Player { get; set; }
}
