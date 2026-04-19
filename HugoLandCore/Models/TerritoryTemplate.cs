namespace HugoLandCore.Models;

/// <summary>
/// Case d'un modèle de carte (coordonnées, terrain).
/// </summary>
public class TerritoryTemplate
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public TerrainType Terrain { get; set; } = TerrainType.Plaine;

    public int WorldTemplateId { get; set; }
    public WorldTemplate? WorldTemplate { get; set; }
}
