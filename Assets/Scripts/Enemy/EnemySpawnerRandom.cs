using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        // Eğer hala 0 ise, kesin yanlış tilemap'e bakıyoruz veya map generator hiç çizmedi
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

    bool TrySpawnOne()
    {
        var b = ground.cellBounds;

        for (int attempt = 0; attempt < maxAttemptsPerEnemy; attempt++)
        {
            int x = Random.Range(b.xMin, b.xMax);
            int y = Random.Range(b.yMin, b.yMax);
            Vector3Int cell = new(x, y, 0);

            if (!ground.HasTile(cell)) continue;
            if (wallsSolid && wallsSolid.HasTile(cell)) continue;
            if (wallsBreakable && wallsBreakable.HasTile(cell)) continue;
            if (wallsHard && wallsHard.HasTile(cell)) continue;

            if (player)
            {
                var pCell = ground.WorldToCell(player.position);
                int dist = Mathf.Abs(cell.x - pCell.x) + Mathf.Abs(cell.y - pCell.y);
                if (dist < minManhattanDistanceFromPlayer) continue;
            }

            Vector3 pos = ground.GetCellCenterWorld(cell);
            Instantiate(enemyPrefab, pos, Quaternion.identity);
            return true;
        }

        return false;
    }
}
