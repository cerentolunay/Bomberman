using UnityEngine;
using UnityEngine.Tilemaps;
using Patterns.Decorator;
using System.Collections.Generic;

namespace DPBomberman.Controllers
{
    public class BombSystem : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject bombPrefab;

        [Header("Refs")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private TilemapDamageSystem damageSystem;
        [SerializeField] private MapLogicAdapter mapLogicAdapter;

        [Header("Settings")]
        [SerializeField] private float fuseSeconds = 2.0f;
        [SerializeField] private int defaultRange = 1;

        private PlayerStatsHolder stats;
        private readonly List<BombController> myBombs = new List<BombController>();

        private void Awake() => AutoWire();

        private void AutoWire()
        {
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();
            if (damageSystem == null) damageSystem = Object.FindFirstObjectByType<TilemapDamageSystem>();
            if (mapLogicAdapter == null) mapLogicAdapter = Object.FindFirstObjectByType<MapLogicAdapter>();

            if (groundTilemap == null)
            {
                var groundGO = GameObject.Find("Ground");
                if (groundGO != null) groundTilemap = groundGO.GetComponent<Tilemap>();
            }

            // ✅ Stats sadece PLAYER üstünden alınmalı (başka holder'a kaymasın)
            if (player != null) stats = player.GetComponent<PlayerStatsHolder>();
        }

        public bool TryPlaceBomb()
        {

            if (bombPrefab == null) return false;

            if (player == null || groundTilemap == null || damageSystem == null)
                AutoWire();

            if (player == null || groundTilemap == null || damageSystem == null)
                return false;

            // stats güncel tut
            if (stats == null) stats = player.GetComponent<PlayerStatsHolder>();

            // patlamış bombaları temizle
            myBombs.RemoveAll(b => b == null);

            // ✅ Varsayılanlar: başlangıçta 1 bomba, default menzil
            int maxBombs = 1;
            int dynamicRange = Mathf.Max(1, defaultRange);

            // ✅ PowerUp alındıysa burada artar
            if (stats != null)
            {
                maxBombs = Mathf.Max(1, stats.BombCount);
                dynamicRange = Mathf.Max(1, stats.BombPower);
            }

            Debug.Log($"[BombSystem] myBombs={myBombs.Count}, maxBombs={maxBombs}, range={dynamicRange}");

            // ✅ aynı anda maxBombs kadar bomba
            if (myBombs.Count >= maxBombs) return false;

            Vector3Int cell = player.GetCurrentCell();

            // aynı hücreye ikinci bomba koyma
            for (int i = 0; i < myBombs.Count; i++)
            {
                if (myBombs[i] == null) continue;

                Vector3Int bombCell = groundTilemap.WorldToCell(myBombs[i].transform.position);
                if (bombCell == cell) return false;
            }

            // powerup üstüne bomba koyma
            if (PowerUpRegistry.Has(cell)) return false;

            // spawn
            var bombObj = Instantiate(bombPrefab);
            var bomb = bombObj.GetComponent<BombController>();
            if (bomb == null)
            {
                Destroy(bombObj);
                return false;
            }

            bomb.fuseSeconds = fuseSeconds;
            bomb.range = dynamicRange;
            bomb.groundTilemap = groundTilemap;
            bomb.damageSystem = damageSystem;
            bomb.mapLogicAdapter = mapLogicAdapter;

            myBombs.Add(bomb);
            bomb.Arm(cell);

            // fallback: fuse bitince listeden düş
            StartCoroutine(RemoveAfterFuse(bomb, fuseSeconds + 0.25f));

            return true;
        }

        private System.Collections.IEnumerator RemoveAfterFuse(BombController bomb, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            myBombs.Remove(bomb);
        }
    }
}
