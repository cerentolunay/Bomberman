using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Patterns.Factory;

public class DesertMapGenerator : BaseMapGeneratorTemplate
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap solidTilemap;
    [SerializeField] private Tilemap breakableTilemap;
    [SerializeField] private Tilemap hardTilemap;

    [Header("Size")]
    [SerializeField] private int width = 15;
    [SerializeField] private int height = 11;

    protected override void ClearMap()
    {
        if (solidTilemap != null) solidTilemap.ClearAllTiles();
        if (breakableTilemap != null) breakableTilemap.ClearAllTiles();
        if (hardTilemap != null) hardTilemap.ClearAllTiles();
    }

    protected override void PlaceOuterWalls()
    {
        // Minimal outer boundary example (solid)
        if (solidTilemap == null || currentFactory == null) return;

        for (int x = 0; x < width; x++)
        {
            currentFactory.PlaceSolid(new Vector3Int(x, 0, 0));
            currentFactory.PlaceSolid(new Vector3Int(x, height - 1, 0));
        }

        for (int y = 0; y < height; y++)
        {
            currentFactory.PlaceSolid(new Vector3Int(0, y, 0));
            currentFactory.PlaceSolid(new Vector3Int(width - 1, y, 0));
        }
    }

    protected override void PlaceInnerWalls()
    {
        // Minimal inner structure (example)
        if (currentFactory == null) return;

        for (int x = 2; x < width - 2; x += 2)
        {
            for (int y = 2; y < height - 2; y += 2)
            {
                currentFactory.PlaceHard(new Vector3Int(x, y, 0));
            }
        }
    }

    protected override void PlacePowerUps()
    {
        // Desert theme could bias fewer powerups (example placeholder)
        // Leave empty or call base behavior if you later implement.
    }
}
