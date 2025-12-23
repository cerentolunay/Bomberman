using UnityEngine.Tilemaps;
using DPBomberman.Models;

namespace DPBomberman.Patterns.Factory
{
    public interface IWallTileFactory
    {
        TileBase GetTile(WallType type);
        ThemeType Theme { get; }
    }
}