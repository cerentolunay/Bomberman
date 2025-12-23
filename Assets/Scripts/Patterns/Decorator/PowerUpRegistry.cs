using System.Collections.Generic;
using UnityEngine;

namespace Patterns.Decorator
{
    public static class PowerUpRegistry
    {
        private static readonly Dictionary<Vector3Int, GameObject> map = new();

        public static bool Has(Vector3Int cell)
        {
            Cleanup(cell);
            return map.ContainsKey(cell);
        }

        public static bool Register(Vector3Int cell, GameObject obj)
        {
            if (obj == null) return false;

            Cleanup(cell);
            if (map.ContainsKey(cell)) return false;

            map[cell] = obj;
            return true;
        }

        public static void Unregister(Vector3Int cell)
        {
            Cleanup(cell);
            map.Remove(cell);
        }

        private static void Cleanup(Vector3Int cell)
        {
            if (map.TryGetValue(cell, out var obj) && obj == null)
                map.Remove(cell);
        }

        public static void ClearAll() => map.Clear();
    }
}
