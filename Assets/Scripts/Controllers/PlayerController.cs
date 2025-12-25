using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using Patterns.Decorator;

namespace DPBomberman.Controllers
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Grid Movement")]
        public Tilemap groundTilemap;
        public Tilemap solidTilemap;
        public Tilemap breakableTilemap;
        public Tilemap hardTilemap;

        [Header("Bomb")]
        [SerializeField] private BombSystem bombSystem;

        [Header("Death")]
        public ExplosionAreaTracker explosionTracker;
        public DamageableActor actor;

        [Tooltip("Bir hücreden diğerine geçiş süresi (sn). 0.08-0.15 arası iyi.)")]
        public float stepDuration = 0.12f;

        private bool isMoving;
        private Vector3Int currentCell;

        private PlayerStatsHolder stats;
        private float baseStepDuration;

        private void Awake()
        {
            stats = GetComponent<PlayerStatsHolder>();
            if (bombSystem == null) bombSystem = Object.FindFirstObjectByType<BombSystem>();
            if (actor == null) actor = GetComponent<DamageableActor>();
        }

        private void Start()
        {
            // Sadece bu karakterin sahibiysen (senin karakterinse) bu işlemleri yap
            if (!IsOwner) return;

            // --- OTOMATİK BULMA SİSTEMİ ---
            // Eğer kutular boşsa (ki prefabdan doğduğu için boş olacak), isimle bulalım:
            if (groundTilemap == null)
                groundTilemap = GameObject.Find("Ground")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();

            if (solidTilemap == null)
                solidTilemap = GameObject.Find("Walls_Solid")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();

            if (breakableTilemap == null)
                breakableTilemap = GameObject.Find("Walls_Breakable")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();

            if (hardTilemap == null)
                hardTilemap = GameObject.Find("Walls_Hard")?.GetComponent<UnityEngine.Tilemaps.Tilemap>();

            // BombSystem ve Actor zaten Awake içinde GetComponent ile kendisini buluyor, onlara dokunma.

            // Pozisyonu hücreye sabitle
            if (groundTilemap != null)
            {
                currentCell = groundTilemap.WorldToCell(transform.position);
                SnapToCell(currentCell);
            }
            else
            {
                Debug.LogError("DİKKAT: Sahnede 'Ground' isminde bir Tilemap bulunamadı!");
            }
        }

        private void Update()
        {
            // Sadece bu karakterin sahibiysen Update çalışsın
            if (!IsOwner) return; // <--- EKLE

            // Input YOK: sadece ölüm/danger kontrolü var
            if (actor != null && actor.IsDead) return;

            if (explosionTracker != null && explosionTracker.IsCellDangerous(currentCell))
            {
                actor?.Kill();
                return;
            }
        }

        // TryMove ve TryPlaceBomb kısımları Command Pattern ile dışarıdan (InputHandler'dan)
        // çağırıldığı için InputHandler'da da "IsOwner" kontrolü yapmamız gerekecek.
        public void TryMove(Vector3Int dir)
        {
            if (!IsOwner) return; // <--- EKLE
            if (actor != null && actor.IsDead) return;
            if (isMoving) return;
            if (dir == Vector3Int.zero) return;

            Vector3Int targetCell = currentCell + dir;
            if (IsBlocked(targetCell)) return;

            StartCoroutine(MoveCellTo(targetCell));
        }

        // ✅ Command'lerin çağıracağı giriş noktası
        public void TryPlaceBomb()
        {
            if (!IsOwner) return;

            if (actor != null && actor.IsDead) return;
            bombSystem?.TryPlaceBomb();
        }

        // ✅ InputHandler için basit kontrol
        public bool IsDead()
        {
            return actor != null && actor.IsDead;
        }

        private bool IsBlocked(Vector3Int cell)
        {
            if (solidTilemap != null && solidTilemap.HasTile(cell)) return true;
            if (hardTilemap != null && hardTilemap.HasTile(cell)) return true;
            if (breakableTilemap != null && breakableTilemap.HasTile(cell)) return true;
            if (!groundTilemap.HasTile(cell)) return true;
            return false;
        }

        private System.Collections.IEnumerator MoveCellTo(Vector3Int targetCell)
        {
            isMoving = true;

            Vector3 startPos = transform.position;
            Vector3 targetPos = groundTilemap.GetCellCenterWorld(targetCell);

            float speedValue = (stats != null) ? Mathf.Max(0.1f, stats.Speed) : 1f;
            float effectiveStepDuration = baseStepDuration / speedValue;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.001f, effectiveStepDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            currentCell = targetCell;
            SnapToCell(currentCell);

            isMoving = false;
        }

        private void SnapToCell(Vector3Int cell)
        {
            transform.position = groundTilemap.GetCellCenterWorld(cell);
        }

        public Vector3Int GetCurrentCell() => currentCell;
    }
}
