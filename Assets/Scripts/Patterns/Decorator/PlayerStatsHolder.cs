using UnityEngine;

namespace Patterns.Decorator
{
    public class PlayerStatsHolder : MonoBehaviour
    {
        [Header("Base Stats (Inspector)")]
        [Tooltip("Inspector'dan ayarla. Boş bırakırsan alttaki fallback değerleri kullanır.")]
        [SerializeField] private BasePlayerStats baseStats;

        [Header("Fallback (BaseStats null ise kullanılır)")]
        [SerializeField] private float fallbackSpeed = 3.5f;
        [SerializeField] private int fallbackBombPower = 1;
        [SerializeField] private int fallbackBombCount = 1;

        public IPlayerStats Current { get; private set; }

        // Timed speed boost (multiplier)
        private float speedMultiplier = 1f;
        private float speedBoostEndTime = -1f;

        // --- READ-ONLY "effective" values ---
        public float Speed => (Current != null ? Current.Speed : fallbackSpeed) * speedMultiplier;
        public int BombPower => Current != null ? Current.BombPower : fallbackBombPower;
        public int BombCount => Current != null ? Current.BombCount : fallbackBombCount;

        private void Awake()
        {
            // Inspector'da baseStats atanmadıysa fallback oluştur
            if (baseStats == null)
                baseStats = new BasePlayerStats(fallbackSpeed, 1, 1);

            Current = baseStats;

            // Timed speed reset
            speedMultiplier = 1f;
            speedBoostEndTime = -1f;
        }


        private void Update()
        {
            // Timed speed boost süresi dolunca sıfırla
            if (speedBoostEndTime > 0f && Time.time >= speedBoostEndTime)
            {
                speedMultiplier = 1f;
                speedBoostEndTime = -1f;
                Debug.Log("[PlayerStatsHolder] Timed speed boost bitti.");
            }
        }

        // ----------------------------
        // POWERUP API
        // ----------------------------

        /// <summary>
        /// Süreli hız çarpanı uygular. Örn multiplier=1.5f, duration=10f
        /// Aynı boost tekrar gelirse süreyi uzatır ve çarpanı günceller.
        /// </summary>
        public void ApplySpeedBoostTimed(float multiplier, float duration)
        {
            if (multiplier <= 0f) multiplier = 1f;
            if (duration <= 0f) duration = 0f;

            speedMultiplier = multiplier;
            speedBoostEndTime = (duration > 0f) ? Time.time + duration : -1f;

            Debug.Log($"[PlayerStatsHolder] Timed Speed Boost: x{multiplier} for {duration}s");
        }

        /// <summary>
        /// Kalıcı hız bonusu (decorator üzerinden). İstersen bunu hiç kullanmayabilirsin.
        /// </summary>
        public void ApplySpeedBoostPermanent(float bonus)
        {
            Current = new SpeedBoostDecorator(Current, bonus);
            Debug.Log($"[PlayerStatsHolder] Permanent Speed Bonus: +{bonus} => BaseSpeedNow={Current.Speed}");
        }

        /// <summary>
        /// Kalıcı bomba menzili artırır.
        /// </summary>
        public int maxBombPower = 3;

        public void ApplyBombPower(int bonus)
        {
            if (bonus == 0) return;

            int before = BombPower;
            Current = new BombPowerDecorator(Current, bonus);

            // cap
            while (Current.BombPower > maxBombPower)
                Current = new BombPowerDecorator(Current, -1);

            Debug.Log($"[PlayerStatsHolder] BombPower {before} -> {Current.BombPower}");
        }

        /// <summary>
        /// Kalıcı bomba hakkı artırır.
        /// </summary>
        public void ApplyBombCount(int bonus)
        {
            if (bonus == 0) return;
            Current = new BombCountDecorator(Current, bonus);
            Debug.Log($"[PlayerStatsHolder] BombCount +{bonus} => {Current.BombCount}");
        }

        /// <summary>
        /// (Opsiyonel) Boostları temizlemek istersen.
        /// </summary>
        public void ResetToBase()
        {
            Current = baseStats;
            speedMultiplier = 1f;
            speedBoostEndTime = -1f;
            Debug.Log("[PlayerStatsHolder] ResetToBase yapıldı.");
        }
      
    }
}
