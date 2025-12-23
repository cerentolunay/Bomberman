using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DPBomberman.Controllers
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Tooltip("Bir hücreden diðerine geçiþ süresi")]
        public float stepDuration = 0.14f;

        [Header("AI")]
        [Tooltip("Her adým arasýnda bekleme (çok hýzlý karar deðiþtirmesin)")]
        public float moveInterval = 0.10f;
        private float nextMoveTime = 0f;

        [Header("Death")]
        public ExplosionAreaTracker explosionTracker;
        public DamageableActor actor;

        private bool isMoving;
        private Vector3Int currentCell;

        /// <summary>
        /// Spawner tarafýndan sahnedeki tilemap referanslarý enjekte edilir.
        /// (City/Desert/Forest fark etmeksizin NULL sorununu çözer.)
        /// </summary>
        public void InjectTilemaps(Tilemap ground, Tilemap solid, Tilemap breakable, Tilemap hard)
        {
            groundTilemap = ground;
            solidTilemap = solid;
            breakableTilemap = breakable;
            hardTilemap = hard;
        }

        private void Start()
        {
            // Actor / tracker auto-wire (varsa)
            if (actor == null)
                actor = GetComponent<DamageableActor>();

            if (explosionTracker == null)
                explosionTracker = FindFirstObjectByType<ExplosionAreaTracker>();

            // Eðer spawner inject etmediyse, sahneden isimle bulmayý dene (opsiyonel güvenlik aðý)
            EnsureTilemapsBound();

            if (groundTilemap == null)
            {
                Debug.LogError("[EnemyController] groundTilemap is NULL. (Spawner InjectTilemaps çaðýrmýyor olabilir)");
                enabled = false;
                return;
            }

            // Baþlangýç hücresi
            currentCell = groundTilemap.WorldToCell(transform.position);

            // (0,0) gibi kenara düþtüyse haritanýn iç bounds'una çek (kenar duvar riskini azaltýr)
            currentCell = ClampToInnerBounds(currentCell);

            // Spawn zone’a denk geldiyse (player spawn civarý), rastgele güvenli hücreye kaç
            if (IsCornerSpawnZone(currentCell))
            {
                currentCell = FindRandomFreeCell();
            }

            SnapToCell(currentCell);

            // Spawn duvarýn üstündeyse yakýndaki boþ hücreye kaydýr
            if (!TryRelocateIfBlocked())
            {
                // Komþu yoksa da rastgele güvenli hücreye kaç
                currentCell = FindRandomFreeCell();
                SnapToCell(currentCell);
            }
        }

        private void Update()
        {
            if (!enabled) return;
            if (groundTilemap == null) return;

            if (actor != null && actor.IsDead) return;

            // Patlama alanýnda mý?
            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                // actor yoksa bile patlamada NRE yemesin
                if (actor != null) actor.Kill();
                return;
            }

            if (isMoving) return;

            // Çok sýk yön deðiþtirmesin
            if (Time.time < nextMoveTime) return;
            nextMoveTime = Time.time + moveInterval;

            Vector3Int dir = PickRandomDirection();
            Vector3Int target = currentCell + dir;

            // blokluysa birkaç deneme yap
            int attempts = 0;
            while (IsBlocked(target) && attempts < 8)
            {
                dir = PickRandomDirection();
                target = currentCell + dir;
                attempts++;
            }

            if (IsBlocked(target))
                return;

            StartCoroutine(MoveCellTo(target));
        }

        private void EnsureTilemapsBound()
        {
            // Buradaki isimler sahnedeki GameObject isimleriyle ayný olmalý:
            // Ground, Walls_Solid, Walls_Breakable, Walls_Hard
            if (groundTilemap == null)
                groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();

            if (solidTilemap == null)
                solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<Tilemap>();

            if (breakableTilemap == null)
                breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<Tilemap>();

            if (hardTilemap == null)
                hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<Tilemap>();
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

            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;

            // ground yoksa harita dýþý
            if (!groundTilemap.HasTile(cell)) return true;

            return false;
        }

        /// <summary>
        /// Enemy'nin baþlangýç hücresini tilemap bounds içinde ve kenarlardan uzak tutar.
        /// </summary>
        private Vector3Int ClampToInnerBounds(Vector3Int cell)
        {
            if (groundTilemap == null) return cell;

            BoundsInt b = groundTilemap.cellBounds;

            // Kenarlarý dýþarýda býrak: +1 / -2
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

            int xMin = b.xMin;
            int yMin = b.yMin;
            int xMax = b.xMax - 1;
            int yMax = b.yMax - 1;

            // Spawn zone hücreleri (MapGenerator ile ayný mantýk): köþe + 2 komþu
            bool bottomLeft = (cell.x == xMin + 1 && cell.y == yMin + 1) || (cell.x == xMin + 1 && cell.y == yMin + 2) || (cell.x == xMin + 2 && cell.y == yMin + 1);
            bool bottomRight = (cell.x == xMax - 1 && cell.y == yMin + 1) || (cell.x == xMax - 1 && cell.y == yMin + 2) || (cell.x == xMax - 2 && cell.y == yMin + 1);
            bool topLeft = (cell.x == xMin + 1 && cell.y == yMax - 1) || (cell.x == xMin + 1 && cell.y == yMax - 2) || (cell.x == xMin + 2 && cell.y == yMax - 1);
            bool topRight = (cell.x == xMax - 1 && cell.y == yMax - 1) || (cell.x == xMax - 1 && cell.y == yMax - 2) || (cell.x == xMax - 2 && cell.y == yMax - 1);

            return bottomLeft || bottomRight || topLeft || topRight;
        }

        private Vector3Int FindRandomFreeCell(int attempts = 200)
        {
            BoundsInt b = groundTilemap.cellBounds;

            int minX = b.xMin + 1;
            int maxX = b.xMax - 2;
            int minY = b.yMin + 1;
            int maxY = b.yMax - 2;

            for (int i = 0; i < attempts; i++)
            {
                int x = Random.Range(minX, maxX + 1);
                int y = Random.Range(minY, maxY + 1);
                var cell = new Vector3Int(x, y, 0);

                if (IsCornerSpawnZone(cell)) continue;
                if (IsBlocked(cell)) continue;

                return cell;
            }

            // Bulamazsa mevcut cell'i döndür (en azýndan crash olmaz)
            return currentCell;
        }


        private bool TryRelocateIfBlocked()
        {
            if (!IsBlocked(currentCell))
                return true;

            Vector3Int[] neighbors =
            {
                currentCell + Vector3Int.right,
                currentCell + Vector3Int.left,
                currentCell + Vector3Int.up,
                currentCell + Vector3Int.down
            };

            foreach (var n in neighbors)
            {
                if (!IsBlocked(n))
                {
                    currentCell = n;
                    SnapToCell(currentCell);
                    Debug.Log($"[EnemyController] Spawn was blocked, relocated to {currentCell}");
                    return true;
                }
            }

            Debug.LogWarning($"[EnemyController] Spawn is blocked and no free neighbor found at {currentCell}");
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
