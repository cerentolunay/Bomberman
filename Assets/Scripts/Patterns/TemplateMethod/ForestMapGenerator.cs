using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Patterns.Factory;

public class ForestMapGenerator : BaseMapGeneratorTemplate
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
        // Forest theme could have more breakables (example placeholder)
        if (currentFactory == null) return;

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if ((x + y) % 3 == 0)
                    currentFactory.PlaceBreakable(new Vector3Int(x, y, 0));
            }
        }
    }

    protected override void AfterGenerate()
    {
        // Example hook for forest-specific post steps
        // (camera tweak, decoration spawn, etc.)
    }
}
