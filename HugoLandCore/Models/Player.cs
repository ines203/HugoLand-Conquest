namespace HugoLandCore.Models;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Gold { get; set; }
    public int DebtTurns { get; set; }

    public int GameId { get; set; }
    public Game? Game { get; set; }

    public ICollection<MilitaryDetachment> Detachments { get; set; } = new List<MilitaryDetachment>();
    public ICollection<PlayerAction> Actions { get; set; } = new List<PlayerAction>();

    public ICollection<Installation> Installations { get; set; } = new List<Installation>();
}
