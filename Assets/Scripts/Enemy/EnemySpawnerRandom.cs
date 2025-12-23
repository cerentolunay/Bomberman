using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Controllers; // EnemyController namespace'in buysa kalsın; değilse kaldır.

public class EnemySpawnerRandom : MonoBehaviour
{
    public GameObject enemyPrefab;

    public Tilemap ground;
    public Tilemap wallsSolid;
    public Tilemap wallsBreakable;
    public Tilemap wallsHard;

    public int enemyCount = 2;
    public int maxAttemptsPerEnemy = 300;

    public Transform player;
    public int minManhattanDistanceFromPlayer = 4;

    IEnumerator Start()
    {
        if (!enemyPrefab)
        {
            Debug.LogError("[EnemySpawnerRandom] enemyPrefab NULL");
            yield break;
        }

        // Eğer inspector'da tilemap set edilmediyse otomatik bul
        if (!ground) ground = GameObject.Find("Ground")?.GetComponent<Tilemap>();
        if (!wallsSolid) wallsSolid = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();
        if (!wallsBreakable) wallsBreakable = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();
        if (!wallsHard) wallsHard = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();

        Debug.Log($"[EnemySpawnerRandom] ground={(ground ? ground.name : "NULL")} used={ground?.GetUsedTilesCount()}");
        Debug.Log($"[EnemySpawnerRandom] deco used={GameObject.Find("Decorations")?.GetComponent<Tilemap>()?.GetUsedTilesCount()}");

        // Harita çizilene kadar bekle (max ~ 2sn)
        int safety = 120;
        while (ground != null && ground.GetUsedTilesCount() == 0 && safety-- > 0)
            yield return null;

        if (ground == null)
        {
            Debug.LogError("[EnemySpawnerRandom] ground tilemap bulunamadı!");
            yield break;
        }

        int used = ground.GetUsedTilesCount();
        Debug.Log($"[EnemySpawnerRandom] Ground ready. usedTiles={used}");

        if (used == 0)
        {
            Debug.LogError("[EnemySpawnerRandom] Ground usedTiles STILL 0. MapGenerator ground'a çizmiyor veya yanlış tilemap seçili!");
            yield break;
        }

        ground.CompressBounds();
        Debug.Log($"[EnemySpawnerRandom] Bounds={ground.cellBounds}");

        int spawned = 0;
        for (int i = 0; i < enemyCount; i++)
            if (TrySpawnOne()) spawned++;

        Debug.Log($"[EnemySpawnerRandom] Spawn result: {spawned}/{enemyCount}");
    }

    private bool TrySpawnOne()
    {
        var b = ground.cellBounds;

        // Kenarları dışarıda bırak (0 ve max kenar duvar)
        int minX = b.xMin + 1;
        int maxX = b.xMax - 2;
        int minY = b.yMin + 1;
        int maxY = b.yMax - 2;

        // bounds beklenmedikse güvenlik
        if (minX > maxX || minY > maxY)
        {
            Debug.LogError("[EnemySpawnerRandom] Invalid bounds after inner clamp. Check ground tilemap bounds.");
            return false;
        }

        // Player cell (varsa)
        Vector3Int pCell = default;
        bool hasPlayer = player != null;
        if (hasPlayer) pCell = ground.WorldToCell(player.position);

        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            int x = Random.Range(minX, maxX + 1);
            int y = Random.Range(minY, maxY + 1);
            Vector3Int cell = new(x, y, 0);

            if (!ground.HasTile(cell)) continue;
            if (wallsSolid && wallsSolid.HasTile(cell)) continue;
            if (wallsBreakable && wallsBreakable.HasTile(cell)) continue;
            if (wallsHard && wallsHard.HasTile(cell)) continue;

            // Spawn zone (köşeler + 2 komşu) düşman için yasak
            if (IsSpawnZone(cell, b)) continue;

            // Player'a çok yakın olmasın
            if (hasPlayer)
            {
                int dist = Mathf.Abs(cell.x - pCell.x) + Mathf.Abs(cell.y - pCell.y);
                if (dist < minManhattanDistanceFromPlayer) continue;
            }

            Vector3 pos = ground.GetCellCenterWorld(cell);

            Debug.Log($"[EnemySpawnerRandom] Spawning enemy at cell={cell} worldPos={pos}");

            // Instantiate
            var go = Instantiate(enemyPrefab, pos, Quaternion.identity);

            // EnemyController varsa tilemap'leri inject et (temaya göre doğru tilemap refs)
            var enemyCtrl = go.GetComponent<DPBomberman.Controllers.EnemyController>();
            if (enemyCtrl != null)
            {
                enemyCtrl.InjectTilemaps(ground, wallsSolid, wallsBreakable, wallsHard);
            }

            return true;
        }

        return false;
    }

    // MapGenerator ile aynı spawn zone mantığı: 4 köşe + 2 komşu
    private bool IsSpawnZone(Vector3Int cell, BoundsInt b)
    {
        int xMin = b.xMin;
        int yMin = b.yMin;
        int xMax = b.xMax - 1;
        int yMax = b.yMax - 1;

        bool bottomLeft = (cell.x == xMin + 1 && cell.y == yMin + 1) || (cell.x == xMin + 1 && cell.y == yMin + 2) || (cell.x == xMin + 2 && cell.y == yMin + 1);
        bool bottomRight = (cell.x == xMax - 1 && cell.y == yMin + 1) || (cell.x == xMax - 1 && cell.y == yMin + 2) || (cell.x == xMax - 2 && cell.y == yMin + 1);
        bool topLeft = (cell.x == xMin + 1 && cell.y == yMax - 1) || (cell.x == xMin + 1 && cell.y == yMax - 2) || (cell.x == xMin + 2 && cell.y == yMax - 1);
        bool topRight = (cell.x == xMax - 1 && cell.y == yMax - 1) || (cell.x == xMax - 1 && cell.y == yMax - 2) || (cell.x == xMax - 2 && cell.y == yMax - 1);

        return bottomLeft || bottomRight || topLeft || topRight;
    }
}
