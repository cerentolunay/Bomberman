using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models.Map;
using DPBomberman.Patterns.Factory;

public class MapLogicAdapter : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap solidTilemap;
    public Tilemap breakableTilemap;
    public Tilemap hardTilemap; // varsa

    private TileWallFactory factory;
    private MapGrid mapGrid;

    public void BuildLogicMap(int width, int height)
    {
        factory = new TileWallFactory(
            solidTilemap,
            breakableTilemap,
            hardTilemap
        );

        mapGrid = new MapGrid(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                CellType type = factory.GetCellType(pos);
                mapGrid.SetCell(x, y, type);
            }
        }

        Debug.Log("[MapLogicAdapter] Logic map built.");
    }

    public MapGrid GetMapGrid() => mapGrid;
}
