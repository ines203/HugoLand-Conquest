using System.ComponentModel.DataAnnotations.Schema;

namespace HugoLandCore.Models;

public enum TerrainType
{
    Plaine,
    Foret,
    Montagne,
    Mer
}

public class Territory
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public TerrainType Terrain { get; set; }


    // Fog of war
    public bool IsVisibleTo(int playerId)
    {
        return Visibility.Any(v => v.PlayerId == playerId && v.Visible);
    }

    public bool IsExploredBy(int playerId)
    {
        return Visibility.Any(v => v.PlayerId == playerId && v.Explored);
    }


    public float TerrainBonus
    {
        get
        {
            switch (Terrain)
            {
                case TerrainType.Plaine:
                    return 1;
                case TerrainType.Foret:
                    return 1.2f;
                case TerrainType.Montagne:
                    return 1.3f;
                default:
                    return 1;
            }
        }
    }

    public int TerrainEnergyCost
    {
        get
        {
            switch (Terrain)
            {
                case TerrainType.Plaine:
                    return 1;
                case TerrainType.Foret:
                    return 2;
                case TerrainType.Montagne:
                    return 3;
                default:
                    return 1;
            }
        }
    }

    public int MapId { get; set; }
    public Map? Map { get; set; }

    public MilitaryDetachment? Detachment { get; set; }
    public Installation? Installation { get; set; }
    public ICollection<TerritoryVisibility> Visibility { get; set; } = new List<TerritoryVisibility>();
}