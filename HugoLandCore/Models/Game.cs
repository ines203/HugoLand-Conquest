using HugoLandCore.Models;

namespace HugoLandCore.Models;

public enum GameStatus
{
    InProgress = 0,
    Finished = 1
}

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public GameStatus Status { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int TurnNumber { get; set; }

    public int MapId { get; set; }
    public Map? Map { get; set; }

    public ICollection<Player> Players { get; set; } = new List<Player>();
    public ICollection<CombatEvent> CombatEvents { get; set; } = new List<CombatEvent>();
    public ICollection<TurnSnapshot> Snapshots { get; set; } = new List<TurnSnapshot>();

}
