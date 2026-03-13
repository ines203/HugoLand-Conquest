namespace HugoLandCore.Models;

public class CombatEvent
{
    public int Id { get; set; }
    public int AttackerPlayerId { get; set; }
    public int DefenderPlayerId { get; set; }
    public int TerritoryId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Result { get; set; } = "";

    public int GameId { get; set; }
    public Game? Game { get; set; }
}
