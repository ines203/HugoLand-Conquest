namespace HugoLandCore.Models;

public class MilitaryDetachment
{
    public int Id { get; set; }
    public int Force { get; set; }      // nb soldats
    public int Energy { get; set; }
    public bool Actionable { get; set; }

    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int? TerritoryId { get; set; }
    public Territory? Territory { get; set; }

    //pour ef core
    public MilitaryDetachment() { }


    public static MilitaryDetachment Create(int territory_id, int force, int energy, bool actionable)
    {
        return new MilitaryDetachment
        {
            TerritoryId = territory_id,
            Force = force,
            Energy = energy,
            Actionable = actionable
        };
    }
}
