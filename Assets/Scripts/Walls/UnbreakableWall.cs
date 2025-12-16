using UnityEngine;
using DPBomberman.Models.Map;

namespace DPBomberman.Walls
{
    public class UnbreakableWall : WallBase
    {
        private void Awake()
        {
            wallType = CellType.Unbreakable;
            hitPoints = int.MaxValue;
        }

        public override void ApplyDamage(int amount)
        {
            // Kýrýlamaz ? hasarý yok say
            Debug.Log($"{name} is unbreakable");
        }
    }
}