using UnityEngine;
using DPBomberman.Models.Map;

namespace DPBomberman.Walls
{
    public class BreakableWall : WallBase
    {
        private void Awake()
        {
            wallType = CellType.Breakable;
            hitPoints = 1;
        }
    }
}