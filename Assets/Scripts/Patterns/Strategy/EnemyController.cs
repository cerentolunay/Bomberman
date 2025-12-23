using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Patterns.Strategy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Tooltip("Bir hücreden diğerine geçiş süresi")]
        public float stepDuration = 0.14f;

        [Header("AI Timing")]
        public float moveInterval = 0.10f;
        private float nextMoveTime = 0f;

        [Header("AI Strategy")]
        public EnemyStrategyType strategyType = EnemyStrategyType.Random;

        public Transform player;
        private IEnemyMovementStrategy strategy;

        [Header("Death")]
        // Bu sınıflar Controller namespace'inde olduğu için tam yol belirttik, doğru.
        public DPBomberman.Controllers.ExplosionAreaTracker explosionTracker;
        public DPBomberman.Controllers.DamageableActor actor;

        private bool isMoving;
        private Vector3Int currentCell;

        public void InjectTilemaps(Tilemap ground, Tilemap solid, Tilemap breakable, Tilemap hard)
        {
            groundTilemap = ground;
            solidTilemap = solid;
            breakableTilemap = breakable;
            hardTilemap = hard;
        }

        public void SetStrategy(EnemyStrategyType type)
        {
            strategyType = type;
            strategy = CreateStrategy(strategyType);
        }

        private void Start()
        {
            // Componentleri otomatik bul
            if (actor == null) actor = GetComponent<DPBomberman.Controllers.DamageableActor>();
            if (explosionTracker == null) explosionTracker = FindFirstObjectByType<DPBomberman.Controllers.ExplosionAreaTracker>();

            if (player == null)
            {
                var p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) player = p.transform;
            }

            EnsureTilemapsBound();
            if (groundTilemap == null)
            {
                Debug.LogError("[EnemyController] Ground Tilemap yok, AI kapatılıyor.");
                enabled = false;
                return;
            }

            // Stratejiyi oluştur
            strategy = CreateStrategy(strategyType);

            // Başlangıç hücresini hesapla
            currentCell = groundTilemap.WorldToCell(transform.position);

            // (0,0) gibi dış kenara düştüyse haritanın iç sınırlarına çek (duvar riskini azaltır)
            currentCell = ClampToInnerBounds(currentCell);

            // Spawn zone'a denk geldiyse (oyuncu başlangıç noktası), rastgele güvenli hücreye kaç
            if (IsCornerSpawnZone(currentCell))
            {
                currentCell = FindRandomFreeCell();
            }

            // Hücreye tam oturt
            SnapToCell(currentCell);

            // Eğer doğduğumuz yer duvarın üstüyse yakındaki boş hücreye kaydır
            if (!TryRelocateIfBlocked())
            {
                // Komşu da yoksa mecburen rastgele güvenli bir yere ışınla
                currentCell = FindRandomFreeCell();
                SnapToCell(currentCell);
            }
        }

        private void Update()
        {
            if (!enabled || groundTilemap == null) return;
            if (actor != null && actor.IsDead) return;

            // Patlama kontrolü: Bastığım kare tehlikeli mi?
            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                if (actor != null) actor.Kill();
                return;
            }

            // Hareket halindeysek veya zaman gelmediyse bekle
            if (isMoving) return;
            if (Time.time < nextMoveTime) return;
            nextMoveTime = Time.time + moveInterval;

            // Stratejiden yön iste
            Vector3Int dir = PickDirectionFromStrategy();
            Vector3Int target = currentCell + dir;

            // Eğer hedef duvar ise, stratejiden yeni yön iste (En fazla 8 deneme)
            int attempts = 0;
            while (IsBlocked(target) && attempts < 8)
            {
                dir = PickDirectionFromStrategy(); // Tekrar rastgele/stratejik yön seç
                target = currentCell + dir;
                attempts++;
            }

            // Hala blokluysa bu tur bekle
            if (IsBlocked(target)) return;

            StartCoroutine(MoveCellTo(target));
        }

        private Vector3Int PickDirectionFromStrategy()
        {
            // Eğer oyuncu öldüyse veya strateji yoksa rastgele gez
            if (strategy == null || player == null) return PickRandomDirection();

            var ctx = new EnemyContext(
                transform.position,
                player.position,
                transform.up,
                Time.deltaTime
            );

            Vector2Int move = strategy.GetNextMove(ctx);
            
            // Strateji (0,0) dönerse hareket etmiyor demektir, rastgele bir yere gitmeye zorla
            if (move == Vector2Int.zero) return PickRandomDirection();

            return new Vector3Int(move.x, move.y, 0);
        }

        private IEnemyMovementStrategy CreateStrategy(EnemyStrategyType type)
        {
            return type switch
            {
                EnemyStrategyType.Random => new RandomMoveStrategy(),
                EnemyStrategyType.ChasePlayer => new ChasePlayerStrategy(),
                _ => new RandomMoveStrategy()
            };
        }

        private void EnsureTilemapsBound()
        {
            if (groundTilemap == null) groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();
            if (solidTilemap == null) solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();
            if (breakableTilemap == null) breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();
            if (hardTilemap == null) hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();
        }

        private Vector3Int PickRandomDirection()
        {
            int r = Random.Range(0, 4);
            return r switch
            {
                0 => Vector3Int.right,
                1 => Vector3Int.left,
                2 => Vector3Int.up,
                _ => Vector3Int.down,
            };
        }

        private bool IsBlocked(Vector3Int cell)
        {
            if (groundTilemap == null) return true;
            // Zemin yoksa boşluktur, yürünemez
            if (!groundTilemap.HasTile(cell)) return true;

            // Duvarlar varsa blokludur
            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;
            
            return false;
        }

        // Enemy'nin başlangıç hücresini tilemap sınırları içinde tutar
        private Vector3Int ClampToInnerBounds(Vector3Int cell)
        {
            if (groundTilemap == null) return cell;

            BoundsInt b = groundTilemap.cellBounds;

            // Kenarları dışarıda bırak: +1 / -2 (Duvar payı)
            int minX = b.xMin + 1;
            int maxX = b.xMax - 2;
            int minY = b.yMin + 1;
            int maxY = b.yMax - 2;

            cell.x = Mathf.Clamp(cell.x, minX, maxX);
            cell.y = Mathf.Clamp(cell.y, minY, maxY);

            return cell;
        }

        private bool IsCornerSpawnZone(Vector3Int cell)
        {
            if (groundTilemap == null) return false;

            BoundsInt b = groundTilemap.cellBounds;
            int xMin = b.xMin; int yMin = b.yMin;
            int xMax = b.xMax - 1; int yMax = b.yMax - 1;

            // 4 Köşe ve komşuları (Spawn Zone)
            bool bottomLeft = (cell.x <= xMin + 2 && cell.y <= yMin + 2);
            bool bottomRight = (cell.x >= xMax - 2 && cell.y <= yMin + 2);
            bool topLeft = (cell.x <= xMin + 2 && cell.y >= yMax - 2);
            bool topRight = (cell.x >= xMax - 2 && cell.y >= yMax - 2);

            return bottomLeft || bottomRight || topLeft || topRight;
        }

        private Vector3Int FindRandomFreeCell(int attempts = 100)
        {
            BoundsInt b = groundTilemap.cellBounds;
            int minX = b.xMin + 1; int maxX = b.xMax - 2;
            int minY = b.yMin + 1; int maxY = b.yMax - 2;

            for (int i = 0; i < attempts; i++)
            {
                int x = Random.Range(minX, maxX + 1);
                int y = Random.Range(minY, maxY + 1);
                var cell = new Vector3Int(x, y, 0);

                if (IsCornerSpawnZone(cell)) continue;
                if (IsBlocked(cell)) continue;

                return cell;
            }
            return currentCell; // Bulamazsa olduğu yerde kalsın
        }

        private bool TryRelocateIfBlocked()
        {
            if (!IsBlocked(currentCell)) return true;

            // 4 yöne bak, boş yer varsa oraya kay
            Vector3Int[] neighbors = { currentCell + Vector3Int.right, currentCell + Vector3Int.left, currentCell + Vector3Int.up, currentCell + Vector3Int.down };
            foreach (var n in neighbors)
            {
                if (!IsBlocked(n))
                {
                    currentCell = n;
                    SnapToCell(currentCell);
                    return true;
                }
            }
            return false;
        }

        private IEnumerator MoveCellTo(Vector3Int targetCell)
        {
            if (groundTilemap == null) yield break;
            isMoving = true;
            
            Vector3 startPos = transform.position;
            Vector3 targetPos = groundTilemap.GetCellCenterWorld(targetCell);
            
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.001f, stepDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }
            
            currentCell = targetCell;
            SnapToCell(currentCell);
            isMoving = false;
        }

        private void SnapToCell(Vector3Int cell)
        {
            if (groundTilemap == null) return;
            transform.position = groundTilemap.GetCellCenterWorld(cell);
        }

        public Vector3Int GetCurrentCell() => currentCell;
    }
}