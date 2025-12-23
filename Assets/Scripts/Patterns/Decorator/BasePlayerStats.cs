using UnityEngine;

namespace Patterns.Decorator
{
    [System.Serializable]
    public class BasePlayerStats : IPlayerStats
    {
        [SerializeField] private float speed = 3.5f;
        [SerializeField] private int bombCount = 1;
        [SerializeField] private int bombPower = 1;

        public float Speed => speed;
        public int BombCount => bombCount;
        public int BombPower => bombPower;

        public BasePlayerStats(float speed, int bombCount, int bombPower)
        {
            this.speed = speed;
            this.bombCount = bombCount;
            this.bombPower = bombPower;
        }

        // Inspector'dan da set edebilin diye boþ ctor da býrakabilirsin
        public BasePlayerStats() { }
    }
}
