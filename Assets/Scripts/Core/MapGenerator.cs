using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models;
using DPBomberman.Patterns.Factory;

public class MapGenerator : MonoBehaviour
{
    [Header("Harita Ayarlarý")]
    public int width = 20;
    public int height = 15;
    public int seed = 123;

    [Header("Görseller (Tile'lar) - Fallback (Factory yoksa)")]
    public TileBase groundTile;
    public TileBase solidTile;
    public TileBase breakableTile;
    public TileBase hardTile;

    [Header("Unity Baðlantýlarý")]
    public Tilemap groundTilemap;
    public Tilemap solidTilemap;
    public Tilemap breakableTilemap;
    public Tilemap hardTilemap;

    [Range(0, 1)] public float breakableDensity = 0.5f;
    [Range(0, 1)] public float hardDensity = 0.15f; // breakable içinde hard oraný

    // IMPORTANT: FAZ 2'de harita üretimini State yönetecek.
    // void Start() { GenerateMap(); }

    // 4 köþe spawn bölgeleri: köþe hücresi + 2 komþusu
    private bool IsSpawnZone(int x, int y)
    {
        bool bottomLeft = (x == 1 && y == 1) || (x == 1 && y == 2) || (x == 2 && y == 1);
        bool bottomRight = (x == width - 2 && y == 1) || (x == width - 2 && y == 2) || (x == width - 3 && y == 1);
        bool topLeft = (x == 1 && y == height - 2) || (x == 1 && y == height - 3) || (x == 2 && y == height - 2);
        bool topRight = (x == width - 2 && y == height - 2) || (x == width - 2 && y == height - 3) || (x == width - 3 && y == height - 2);

        return bottomLeft || bottomRight || topLeft || topRight;
    }

    /// <summary>
    /// Haritayý üretir. Factory verilirse theme tile'larý kullanýlýr; verilmezse fallback tile'lar kullanýlýr.
    /// </summary>
    [ContextMenu("Haritayý Oluþtur")]
    public void GenerateMap(IWallTileFactory factory = null)
    {
        if (!groundTilemap || !solidTilemap || !breakableTilemap)
        {
            Debug.LogError("Tilemap baðlantýlarý eksik! (ground/solid/breakable)");
            return;
        }

        // Factory varsa tile'larý oradan al; yoksa fallback kullan
        TileBase gTile = factory != null ? factory.GetTile(WallType.Ground) : groundTile;
        TileBase sTile = factory != null ? factory.GetTile(WallType.Unbreakable) : solidTile;
        TileBase bTile = factory != null ? factory.GetTile(WallType.Breakable) : breakableTile;
        TileBase hTile = factory != null ? factory.GetTile(WallType.Hard) : hardTile;

        // Minimum tile kontrolü
        if (!gTile || !sTile || !bTile)
        {
            Debug.LogError(
                "Tile baðlantýlarý eksik! (ground/solid/breakable). " +
                "Factory kullanýyorsan ThemeFactorySO slotlarýný doldur, yoksa Inspector tile'larýný doldur."
            );
            return;
        }

        // Opsiyonel hard kontrolü
        if (hardTilemap == null && hTile != null)
        {
            Debug.LogWarning("[MapGenerator] Hard tile var ama hardTilemap atanmadý. Hard duvar basýlmayacak.");
        }

        groundTilemap.ClearAllTiles();
        solidTilemap.ClearAllTiles();
        breakableTilemap.ClearAllTiles();
        hardTilemap?.ClearAllTiles();

        System.Random rng = new System.Random(seed);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                groundTilemap.SetTile(pos, gTile);

                // Kenar duvarlarý
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    solidTilemap.SetTile(pos, sTile);
                }
                // Ýç kolonlar
                else if (x % 2 == 0 && y % 2 == 0)
                {
                    solidTilemap.SetTile(pos, sTile);
                }
                else
                {
                    // Spawn bölgeleri boþ kalsýn
                    if (IsSpawnZone(x, y))
                        continue;

                    if (rng.NextDouble() < breakableDensity)
                    {
                        if (hardTilemap != null && hTile != null && rng.NextDouble() < hardDensity)
                            hardTilemap.SetTile(pos, hTile);
                        else
                            breakableTilemap.SetTile(pos, bTile);
                    }
                }
            }
        }

        AdjustCamera();
    }

    private void AdjustCamera()
    {
        if (Camera.main == null) return;
        Camera.main.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10f);
        Camera.main.orthographicSize = height / 2f + 1f;
    }
}
