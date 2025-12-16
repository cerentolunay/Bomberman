using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Models.Map;

namespace DPBomberman.Patterns.Factory
{
    public class TileWallFactory
    {
        private Tilemap solidTilemap;
        private Tilemap breakableTilemap;
        private Tilemap hardTilemap; // opsiyonel

        public TileWallFactory(
            Tilemap solid,
            Tilemap breakable,
            Tilemap hard = null
        )
        {
            solidTilemap = solid;
            breakableTilemap = breakable;
            hardTilemap = hard;
        }

        public CellType GetCellType(Vector3Int pos)
        {
            if (hardTilemap != null && hardTilemap.HasTile(pos))
                return CellType.Hard;

            if (solidTilemap.HasTile(pos))
                return CellType.Unbreakable;

            if (breakableTilemap.HasTile(pos))
                return CellType.Breakable;

            return CellType.Ground;
        }
    }
}