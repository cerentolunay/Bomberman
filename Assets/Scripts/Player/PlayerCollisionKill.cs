using UnityEngine;
using DPBomberman.Controllers;
using DPBomberman.Patterns.Strategy;

public class PlayerCollisionKill : MonoBehaviour
{
    [Header("Safety")]
    [SerializeField] private float graceSeconds = 0.30f;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;

    private bool dead;
    private float bornTime;

    private void OnEnable()
    {
        // Restart/respawn gibi durumlarda tekrar düzgün baþlasýn
        dead = false;
        bornTime = Time.time;
    }

    private bool InGrace()
    {
        return (Time.time - bornTime) < graceSeconds;
    }

    private bool IsRealEnemy(Component c)
    {
        if (c == null) return false;
        if (!c.CompareTag("Enemy")) return false;

        // Collider child'ta olabilir; EnemyController parent/root'ta aranýr.
        return c.GetComponentInParent<EnemyController>() != null;
    }

    private void Log(string msg)
    {
        if (!verboseLogs) return;
        Debug.Log(msg);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead || InGrace()) return;

        Log($"[PlayerCollisionKill] COLLISION with {collision.collider.name} tag={collision.collider.tag} " +
            $"root={collision.collider.transform.root.name} pos={collision.collider.transform.root.position}");

        if (IsRealEnemy(collision.collider))
            Die("Collision");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || InGrace()) return;

        Log($"[PlayerCollisionKill] TRIGGER with {other.name} tag={other.tag} " +
            $"root={other.transform.root.name} pos={other.transform.root.position}");

        if (IsRealEnemy(other))
            Die("Trigger");
    }

    public void Die()
    {
        Die("Unknown");
    }

    public void Die(string reason)
    {
        if (dead) return;
        dead = true;

        Debug.Log($"[Player] DEAD (by {reason})");

        // Görsel kapat
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.enabled = false;

        // Hareket durdur
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        // Diðer scriptleri kapat
        foreach (var mb in GetComponents<MonoBehaviour>())
            if (mb != this) mb.enabled = false;

        // Collider kapat
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log("[Player] DEAD");

        var gm = FindFirstObjectByType<GameManager>();
        if (!gm)
        {
            Debug.LogError("[Player] GameManager not found in scene!");
            return;
        }

        gm.GoToGameOver();
    }
}
