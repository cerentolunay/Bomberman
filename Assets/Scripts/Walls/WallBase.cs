using UnityEngine;
using DPBomberman.Models.Map;

namespace DPBomberman.Walls
{
    public abstract class WallBase : MonoBehaviour, IDamageable
    {
        [Header("Wall Settings")]
        [SerializeField] protected CellType wallType;
        [SerializeField] protected int hitPoints;

        public CellType WallType => wallType;
        public bool IsDestroyed => hitPoints <= 0;

        public virtual void ApplyDamage(int amount)
        {
            if (hitPoints <= 0)
                return;

            hitPoints -= amount;
            Debug.Log($"{name} damaged, HP left: {hitPoints}");

            if (hitPoints <= 0)
            {
                OnDestroyed();
            }
        }

        protected virtual void OnDestroyed()
        {
            Debug.Log($"{name} destroyed");
            Destroy(gameObject);
        }
    }
}