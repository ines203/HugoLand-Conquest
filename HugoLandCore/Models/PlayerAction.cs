namespace HugoLandCore.Models;

public class PlayerAction
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public DateTime Timestamp { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }
}
