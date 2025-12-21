using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models.Map;
using DPBomberman.Patterns.Factory;
using Patterns.Decorator;

namespace DPBomberman.Controllers
{
    public class TilemapDamageSystem : MonoBehaviour
    {
        [Header("Tilemaps")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Header("Hard Wall Settings")]
        [Min(1)] public int hardWallHp = 3;

        [Header("Explosion Tracking")]
        public ExplosionAreaTracker explosionTracker;

        [Header("PowerUp Drop Chances")]
        // İsteğin üzerine oranları güncelledim
        [Range(0f, 1f)] public float breakableDropChance = 0.40f; // %40
        [Range(0f, 1f)] public float hardDropChance = 0.60f;      // %60

        [Header("PowerUp Prefabs (MUTLAKA ATANMALI)")]
        public GameObject bombPowerPowerUpPrefab; // En çok çıkmasını istediğin (Bomba Gücü)
        public GameObject bombCountPowerUpPrefab; // Ekstra Bomba
        public GameObject speedPowerUpPrefab;     // Hız

        private TileWallFactory factory;
        private readonly Dictionary<Vector3Int, int> hardHp = new();
        private bool warnedMissingPowerUpPrefabs;

        private void Awake()
        {
            AutoAssignTilemaps();
            RebuildFactoryIfNeeded();
        }

        private void OnValidate()
        {
            if (hardWallHp < 1) hardWallHp = 1;
            breakableDropChance = Mathf.Clamp01(breakableDropChance);
            hardDropChance = Mathf.Clamp01(hardDropChance);
        }

        private void AutoAssignTilemaps()
        {
            if (groundTilemap == null) groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();
            if (solidTilemap == null) solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();
            if (breakableTilemap == null) breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();
            if (hardTilemap == null) hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();
        }

        private void RebuildFactoryIfNeeded()
        {
            if (factory == null && solidTilemap != null)
                factory = new TileWallFactory(solidTilemap, breakableTilemap, hardTilemap);
        }

        public void Explode(Vector3Int origin, int range, MapLogicAdapter logicAdapter)
        {
            if (range < 1) range = 1;
            AutoAssignTilemaps();
            RebuildFactoryIfNeeded();

            if (groundTilemap == null || factory == null) return;

            // Patlama görseli ve mantığı
            var cells = new List<Vector3Int> { origin };
            CollectRay(origin, Vector3Int.right, range, cells);
            CollectRay(origin, Vector3Int.left, range, cells);
            CollectRay(origin, Vector3Int.up, range, cells);
            CollectRay(origin, Vector3Int.down, range, cells);

            explosionTracker?.ActivateCells(cells);

            // Hasar verme işlemi
            ApplyExplosionToCell(origin, logicAdapter);
            Propagate(origin, Vector3Int.right, range, logicAdapter);
            Propagate(origin, Vector3Int.left, range, logicAdapter);
            Propagate(origin, Vector3Int.up, range, logicAdapter);
            Propagate(origin, Vector3Int.down, range, logicAdapter);
        }

        private void Propagate(Vector3Int origin, Vector3Int dir, int range, MapLogicAdapter logicAdapter)
        {
            for (int step = 1; step <= range; step++)
            {
                Vector3Int cell = origin + dir * step;
                if (groundTilemap != null && !groundTilemap.HasTile(cell)) break;
                if (ApplyExplosionToCell(cell, logicAdapter)) break;
            }
        }

        private void CollectRay(Vector3Int origin, Vector3Int dir, int range, List<Vector3Int> cells)
        {
            for (int step = 1; step <= range; step++)
            {
                Vector3Int cell = origin + dir * step;
                if (groundTilemap != null && !groundTilemap.HasTile(cell)) break;

                cells.Add(cell);

                CellType type = factory.GetCellType(cell);
                if (type == CellType.Unbreakable || type == CellType.Breakable || type == CellType.Hard)
                    break;
            }
        }

        private bool ApplyExplosionToCell(Vector3Int cell, MapLogicAdapter logicAdapter)
        {
            if (factory == null) return false;
            CellType type = factory.GetCellType(cell);

            if (type == CellType.Unbreakable) return true;

            // BREAKABLE DUVAR
            if (type == CellType.Breakable)
            {
                if (breakableTilemap != null) breakableTilemap.SetTile(cell, null);
                logicAdapter?.GetMapGrid()?.SetCell(cell.x, cell.y, CellType.Ground);

                // %40 ihtimalle düşür
                TrySpawnPowerUp(cell, breakableDropChance);
                return true;
            }

            // HARD DUVAR
            if (type == CellType.Hard)
            {
                int hpLeft = ApplyHardDamage(cell);
                if (hpLeft <= 0)
                {
                    if (hardTilemap != null) hardTilemap.SetTile(cell, null);
                    logicAdapter?.GetMapGrid()?.SetCell(cell.x, cell.y, CellType.Ground);

                    // %60 ihtimalle düşür
                    TrySpawnPowerUp(cell, hardDropChance);
                }
                return true;
            }

            return false;
        }

        // ==========================================
        // DÜZELTİLEN KISIM BURASI
        // ==========================================
        private void TrySpawnPowerUp(Vector3Int cell, float dropChance)
        {
            if (groundTilemap == null) return;

            // O karede zaten bir power-up varsa işlem yapma
            if (PowerUpRegistry.Has(cell)) return;

            // 1. ADIM: Power-up düşecek mi? (Duvarın kendi şansı %40 veya %60)
            // Eğer zar, şanstan büyük gelirse power-up düşmez, fonksiyon biter.
            if (Random.value > dropChance) return;


            // 2. ADIM: Hangisi düşecek? (HEPSİ EŞİT İHTİMAL)
            // 3 çeşit power-up var. Unity 0, 1 veya 2 sayısını tutacak.
            int roll = Random.Range(0, 3);

            GameObject prefabToSpawn = null;

            if (roll == 0)
            {
                prefabToSpawn = bombPowerPowerUpPrefab;
            }
            else if (roll == 1)
            {
                prefabToSpawn = bombCountPowerUpPrefab;
            }
            else // roll == 2
            {
                prefabToSpawn = speedPowerUpPrefab;
            }


            // Eğer inspector'da prefabı boş bıraktıysan hata vermesin diye kontrol
            if (prefabToSpawn == null)
            {
                if (!warnedMissingPowerUpPrefabs)
                {
                    Debug.LogWarning("TilemapDamageSystem: PowerUp Prefabları eksik! PowerUp düşüremiyorum.");
                    warnedMissingPowerUpPrefabs = true;
                }
                return;
            }

            // Power-up'ı yarat
            Vector3 worldPos = groundTilemap.GetCellCenterWorld(cell);
            GameObject obj = Instantiate(prefabToSpawn, worldPos, Quaternion.identity);

            // Sisteme kaydet (Registry ve Pickup)
            if (!PowerUpRegistry.Register(cell, obj))
            {
                Destroy(obj); // Kayıt başarısızsa (üst üste bindiyse) sil
                return;
            }

            var pickup = obj.GetComponent<PowerUpPickup>();
            if (pickup != null)
            {
                pickup.RegisterCell(cell);
            }
        }

        private int ApplyHardDamage(Vector3Int cell)
        {
            if (!hardHp.TryGetValue(cell, out int hp)) hp = hardWallHp;
            hp -= 1;
            if (hp <= 0)
            {
                hardHp.Remove(cell);
                return 0;
            }
            hardHp[cell] = hp;
            return hp;
        }
    }
}