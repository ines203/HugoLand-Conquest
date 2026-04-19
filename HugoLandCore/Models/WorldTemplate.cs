namespace HugoLandCore.Models;

/// <summary>
/// Modèle de carte réutilisable créé dans l'éditeur (Phase 2).
/// </summary>
public class WorldTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsValid { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Position de départ des joueurs
    public int? StartX1 { get; set; }
    public int? StartY1 { get; set; }
    public int? StartX2 { get; set; }
    public int? StartY2 { get; set; }

    public ICollection<TerritoryTemplate> Territories { get; set; } = new List<TerritoryTemplate>();
    public ICollection<Game> Games { get; set; } = new List<Game>();
}
