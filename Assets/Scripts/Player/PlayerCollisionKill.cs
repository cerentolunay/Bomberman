using UnityEngine;

public class PlayerCollisionKill : MonoBehaviour
{
    private bool dead;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (dead) return;

        if (collision.collider.CompareTag("Enemy"))
            Die();
    }

    // Enemy Trigger ise bunu kullan (Collision çalýþmazsa)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead) return;

        if (other.CompareTag("Enemy"))
            Die();
    }

    public  void Die()
    {
        dead = true;

        // 1) görünmesin
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.enabled = false;

        // 2) hareket etmesin
        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        // 3) scriptleri kapat (PlayerMovement vs)
        foreach (var mb in GetComponents<MonoBehaviour>())
        {
            if (mb != this) mb.enabled = false;
        }

        // 4) collider kapat ki tekrar tetiklenmesin
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        Debug.Log("[Player] DEAD");
        var ui = FindFirstObjectByType<UIManager>();
        if (!ui)
        {
            Debug.LogError("[Player] UIManager not found in scene!");
            return;
        }

        ui.ShowGameOver();
    }
}
