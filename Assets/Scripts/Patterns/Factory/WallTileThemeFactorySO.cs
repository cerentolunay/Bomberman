using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models;

namespace DPBomberman.Patterns.Factory
{
    [CreateAssetMenu(menuName = "DPBomberman/Factory/Wall Tile Theme Factory")]
    public class WallTileThemeFactorySO : ScriptableObject, IWallTileFactory
    {
        [SerializeField] private ThemeType theme;

        [Header("Tiles")]
        public TileBase groundTile;
        public TileBase solidTile;      // Unbreakable
        public TileBase breakableTile;
        public TileBase hardTile;

        public ThemeType Theme => theme;

        public TileBase GetTile(WallType type)
        {
            return type switch
            {
                WallType.Ground => groundTile,
                WallType.Unbreakable => solidTile,
                WallType.Breakable => breakableTile,
                WallType.Hard => hardTile,
                _ => null
            };
        }
    }
}
