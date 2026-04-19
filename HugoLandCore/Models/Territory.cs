namespace HugoLandCore.Models;

public enum TerrainType
{
    Plaine = 0,
    Foret = 1,
    Montagne = 2,
    Mer = 3
}

public class Territory
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public TerrainType Terrain { get; set; }

    public float TerrainBonus
    {
        get
        {
            return Terrain switch
            {
                case TerrainType.Plaine:
                    return 1;
                default:
                    return 1;
            }
        }
    }

    public int TerrainEnergyCost
    {
        get
        {
            return Terrain switch
            {
                case TerrainType.Plaine:
                    return 1;
                default:
                    return 1;
            }
        }
    }

    public bool IsPassable => Terrain != TerrainType.Mer;

    public int MapId { get; set; }
    public Map? Map { get; set; }

    public MilitaryDetachment? Detachment { get; set; }
    public Installation? Installation { get; set; }
}