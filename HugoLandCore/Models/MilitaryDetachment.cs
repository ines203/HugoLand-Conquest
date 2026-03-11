using HugoLandCore.Models;

namespace HugoLandCore.Models;

public class MilitaryDetachment
{
    public int Id { get; set; }
    public int Force { get; set; }      // nb soldats
    public int Energy { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int TerritoryId { get; set; }
    public Territory? Territory { get; set; }
}
