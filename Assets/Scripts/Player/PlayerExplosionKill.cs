using UnityEngine;
using UnityEngine.Tilemaps;
using DPBomberman.Controllers; // ExplosionAreaTracker burada

public class PlayerExplosionKill : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private ExplosionAreaTracker explosionTracker;
    [SerializeField] private Tilemap groundTilemap;

    [Header("Optional")]
    [SerializeField] private PlayerCollisionKill playerKill; // sende zaten var, Die() çaðýrmak için

    private bool dead;

    private void Awake()
    {
        if (!playerKill) playerKill = GetComponent<PlayerCollisionKill>();
    }

    private void Update()
    {
        if (dead) return;
        if (!explosionTracker || !groundTilemap) return;

        Vector3Int cell = groundTilemap.WorldToCell(transform.position);

        if (explosionTracker.IsCellDangerous(cell))
        {
            dead = true;

            // Eðer PlayerCollisionKill içinde Die() varsa:
            if (playerKill) playerKill.Die();
            else
            {
                // yoksa en minimal: hareketi kapat
                var move = GetComponent<PlayerMovement>();
                if (move) move.enabled = false;

                Debug.Log("[Player] DEAD (Explosion)");
            }
        }
    }
}
