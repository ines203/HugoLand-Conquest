using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLandCore.Data;

/// <summary>
/// Crée les 3 mondes 
/// </summary>
public static class SeedWorlds
{
    public static void EnsureWorlds(HugoLandContext context)
    {
        if (context.WorldTemplates.Any()) return; // déjà seedés

        var service = new WorldTemplateService(context);

        // === Monde 1 : La Grande Plaine (15x10, terrain mixte) ===
        var w1 = service.CreateWorld("The Great Plain", "Vast open plains with some forests.", 15, 10);
        // Quelques forêts
        foreach (var (x, y) in new[] { (3,2),(4,2),(3,3),(4,3),(10,6),(11,6),(10,7) })
            service.SetTerrain(w1.Id, x, y, TerrainType.Foret);
        // Positions de départ
        service.SetStartPosition(w1.Id, 1, 0, 9);
        service.SetStartPosition(w1.Id, 2, 14, 0);
        service.ValidateWorld(w1.Id);

        // === Monde 2 : L'Archipel des Batailles (15x10, beaucoup de mer) ===
        var w2 = service.CreateWorld("The Archipelago of Battles", "Islands separated by the sea, tactical combat", 15, 10);
        // Mer sur les bords et couloirs
        for (int x = 0; x < 15; x++)
        {
            service.SetTerrain(w2.Id, x, 4, TerrainType.Mer);
            service.SetTerrain(w2.Id, x, 5, TerrainType.Mer);
        }
        // Couloirs de passage
        service.SetTerrain(w2.Id, 4, 4, TerrainType.Plaine);
        service.SetTerrain(w2.Id, 4, 5, TerrainType.Plaine);
        service.SetTerrain(w2.Id, 10, 4, TerrainType.Plaine);
        service.SetTerrain(w2.Id, 10, 5, TerrainType.Plaine);
        // Montagnes défensives
        foreach (var (x, y) in new[] { (7,1),(7,2),(7,7),(7,8) })
            service.SetTerrain(w2.Id, x, y, TerrainType.Montagne);
        service.SetStartPosition(w2.Id, 1, 0, 9);
        service.SetStartPosition(w2.Id, 2, 14, 0);
        service.ValidateWorld(w2.Id);

        // === Monde 3 : Les Monts du Destin (15x10, terrain montagneux) ===
        var w3 = service.CreateWorld("The Mounts of Destiny", "Central mountain chain hard to traverse", 15, 10);
        // Chaîne de montagnes centrale
        for (int y = 0; y < 10; y++)
        {
            service.SetTerrain(w3.Id, 7, y, TerrainType.Montagne);
            if (y != 4 && y != 5) // couloirs de passage
            {
                service.SetTerrain(w3.Id, 6, y, TerrainType.Montagne);
                service.SetTerrain(w3.Id, 8, y, TerrainType.Montagne);
            }
        }
        // Forêts sur les flancs
        foreach (var (x, y) in new[] { (2,1),(3,1),(2,2),(11,7),(12,7),(12,8) })
            service.SetTerrain(w3.Id, x, y, TerrainType.Foret);
        service.SetStartPosition(w3.Id, 1, 1, 8);
        service.SetStartPosition(w3.Id, 2, 13, 1);
        service.ValidateWorld(w3.Id);

        context.SaveChanges();
    }
}
