using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Controllers;
using DPBomberman.Patterns.Strategy;

public class EnemySpawnerRandom : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Tilemaps (Inspector preferred)")]
    public Tilemap ground;
    public Tilemap wallsSolid;
    public Tilemap wallsBreakable;
    public Tilemap wallsHard;
    public Tilemap decorations;

    [Header("Spawn")]
    public int enemyCount = 2;
    public int maxAttemptsPerEnemy = 300;

    [Header("Player")]
    public Transform player;
    public int minManhattanDistanceFromPlayer = 4;

    [Header("AI (Strategy)")]
    public EnemyStrategyType spawnStrategy = EnemyStrategyType.Random;

    private IEnumerator Start()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("[EnemySpawnerRandom] enemyPrefab NULL");
            yield break;
        }

        // Inspector set edilmediyse otomatik bul
        if (!ground) ground = FindTilemapSafe("Ground");
        if (!wallsSolid) wallsSolid = FindTilemapSafe("Walls_Solid");
        if (!wallsBreakable) wallsBreakable = FindTilemapSafe("Walls_Breakable");
        if (!wallsHard) wallsHard = FindTilemapSafe("Walls_Hard");
        if (!decorations) decorations = FindTilemapSafe("Decorations");

        // Harita çizilene kadar bekle (max ~ 2sn)
        int safety = 120;
        while (ground != null && SafeUsedTilesCount(ground) == 0 && safety-- > 0)
            yield return null;

        if (ground == null)
        {
            Debug.LogError("[EnemySpawnerRandom] ground tilemap bulunamadı!");
            yield break;
        }

        int used = SafeUsedTilesCount(ground);
        Debug.Log($"[EnemySpawnerRandom] Ground ready. usedTiles={used}");

        if (used == 0)
        {
            Debug.LogError("[EnemySpawnerRandom] Ground usedTiles STILL 0. MapGenerator çizmemiş olabilir.");
            yield break;
        }

        ground.CompressBounds();
        
        int spawned = 0;
        for (int i = 0; i < enemyCount; i++)
        {
            if (TrySpawnOne()) spawned++;
        }

        Debug.Log($"[EnemySpawnerRandom] Spawn result: {spawned}/{enemyCount}");
    }

    private bool TrySpawnOne()
    {
        if (ground == null) return false;

        var b = ground.cellBounds;

        // Kenarları dışarıda bırak (0 ve max kenar duvar)
        int minX = b.xMin + 1;
        int maxX = b.xMax - 2; // Bounds genelde exclusive olduğu için -1 yerine güvenli -2
        int minY = b.yMin + 1;
        int maxY = b.yMax - 2;

        // Player cell (varsa)
        Vector3Int pCell = default;
        bool hasPlayer = player != null;
        if (hasPlayer) pCell = ground.WorldToCell(player.position);

        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            // --- DÜZELTME: Çift tanımlamalar silindi, tek bir Random mantığı bırakıldı ---
            int x = Random.Range(minX, maxX + 1);
            int y = Random.Range(minY, maxY + 1);
            Vector3Int cell = new Vector3Int(x, y, 0);

            // 1. Tile kontrolü
            if (!ground.HasTile(cell)) continue;

            // 2. Duvar kontrolü
            if (wallsSolid && wallsSolid.HasTile(cell)) continue;
            if (wallsBreakable && wallsBreakable.HasTile(cell)) continue;
            if (wallsHard && wallsHard.HasTile(cell)) continue;

            // 3. Spawn zone kontrolü (köşeler yasak)
            if (IsSpawnZone(cell, b)) continue;

            // 4. Player mesafe kontrolü
            if (hasPlayer)
            {
                int dist = Mathf.Abs(cell.x - pCell.x) + Mathf.Abs(cell.y - pCell.y);
                if (dist < minManhattanDistanceFromPlayer) continue;
            }

            // --- SPAWN İŞLEMİ ---
            Vector3 pos = ground.GetCellCenterWorld(cell);
            
            // Tek sefer Instantiate et
            var go = Instantiate(enemyPrefab, pos, Quaternion.identity);

            // Component'ı al ve AYNI component üzerinde işlemlerini yap
            var enemyCtrl = go.GetComponent<EnemyController>();
            
            if (enemyCtrl != null)
            {
                // Hem Tilemap referanslarını ver
                enemyCtrl.InjectTilemaps(ground, wallsSolid, wallsBreakable, wallsHard);
                // Hem de Strategy tipini ayarla
                enemyCtrl.SetStrategy(spawnStrategy);
                
                Debug.Log($"[EnemySpawnerRandom] Spawning enemy at {cell}. Strategy: {spawnStrategy}");
            }
            else
            {
                Debug.LogWarning("[EnemySpawnerRandom] Spawned enemy has no EnemyController component!");
            }

            return true;
        }

        return false;
    }

    private bool IsSpawnZone(Vector3Int cell, BoundsInt b)
    {
        int xMin = b.xMin;
        int yMin = b.yMin;
        int xMax = b.xMax - 1; 
        int yMax = b.yMax - 1;

        // Sol Alt Köşe (L Şekli)
        bool bottomLeft = (cell.x == xMin + 1 && cell.y == yMin + 1) || 
                          (cell.x == xMin + 1 && cell.y == yMin + 2) || 
                          (cell.x == xMin + 2 && cell.y == yMin + 1);
        
        // Sağ Alt
        bool bottomRight = (cell.x == xMax - 1 && cell.y == yMin + 1) || 
                           (cell.x == xMax - 1 && cell.y == yMin + 2) || 
                           (cell.x == xMax - 2 && cell.y == yMin + 1);
        
        // Sol Üst
        bool topLeft = (cell.x == xMin + 1 && cell.y == yMax - 1) || 
                       (cell.x == xMin + 1 && cell.y == yMax - 2) || 
                       (cell.x == xMin + 2 && cell.y == yMax - 1);
        
        // Sağ Üst
        bool topRight = (cell.x == xMax - 1 && cell.y == yMax - 1) || 
                        (cell.x == xMax - 1 && cell.y == yMax - 2) || 
                        (cell.x == xMax - 2 && cell.y == yMax - 1);

        return bottomLeft || bottomRight || topLeft || topRight;
    } // <-- BU PARANTEZ EKSİKTİ

    // -------------------------
    // SAFE HELPERS
    // -------------------------

    private static Tilemap FindTilemapSafe(string goName)
    {
        var go = GameObject.Find(goName);
        return go ? go.GetComponent<Tilemap>() : null;
    }

    private static int SafeUsedTilesCount(Tilemap tm)
    {
        if (tm == null) return 0;

        try
        {
            // Extension methodun projenin başka bir yerinde tanımlı olduğunu varsayıyoruz
            return tm.GetUsedTilesCount();
        }
        catch (MissingReferenceException)
        {
            return 0;
        }
    }
}