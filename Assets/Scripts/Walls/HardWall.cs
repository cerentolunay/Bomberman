using UnityEngine;
using DPBomberman.Models.Map;

namespace DPBomberman.Walls
{
    public class HardWall : WallBase
    {
        private void Awake()
        {
            wallType = CellType.Hard;
            hitPoints = 3; // çoklu patlama ister
        }
    }
}