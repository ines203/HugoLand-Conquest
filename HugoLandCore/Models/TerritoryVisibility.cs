namespace HugoLandCore.Models;

public class TerritoryVisibility
{
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int TerritoryId { get; set; }
    public Territory Territory { get; set; } = null!;

    public bool Explored { get; set; }
    public bool Visible { get; set; }
}
