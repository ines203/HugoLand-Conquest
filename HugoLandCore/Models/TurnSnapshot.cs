using HugoLandCore.Models;

namespace HugoLandCore.Models;

public class TurnSnapshot
{
    public int Id { get; set; }
    public int TurnNumber { get; set; }
    public int PlayerId { get; set; }
    public int Gold { get; set; }
    public int ArmyCount { get; set; }
    public int TotalSoldiers { get; set; }
    public int FortificationCount { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }
}
