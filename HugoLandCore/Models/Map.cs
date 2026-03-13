using HugoLandCore.Models;

namespace HugoLandCore.Models;

public class Map
{
    public int Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public ICollection<Territory> Territories { get; set; } = new List<Territory>();
}
