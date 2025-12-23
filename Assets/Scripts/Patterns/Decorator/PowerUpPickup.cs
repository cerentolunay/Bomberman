using UnityEngine;
using Patterns.Decorator;
using DPBomberman.Controllers;

public enum PowerUpType
{
    Speed,
    BombPower,
    BombCount
}

[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Type & Bonus")]
    public PowerUpType type;
    public int intBonus = 1;

    [Header("Pickup")]
    public KeyCode pickupKey = KeyCode.E;

    private Vector3Int registeredCell;
    private bool isRegistered;

    private PlayerStatsHolder touchingHolder;
    private bool picked;

    // MapGenerator/Registry çağırır
    public void RegisterCell(Vector3Int cell)
    {
        registeredCell = cell;
        isRegistered = true;
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Update()
    {
        if (picked) return;

        if (touchingHolder != null && Input.GetKeyDown(pickupKey))
        {
            Pick(touchingHolder);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        // Tag’e bağlı kalma
        var holder = other.GetComponentInParent<PlayerStatsHolder>();
        if (holder == null) holder = other.GetComponent<PlayerStatsHolder>();
        if (holder == null) return;

        touchingHolder = holder;
        // Debug.Log($"[PowerUpPickup] In range: {type}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var holder = other.GetComponentInParent<PlayerStatsHolder>();
        if (holder == null) holder = other.GetComponent<PlayerStatsHolder>();

        if (holder != null && holder == touchingHolder)
            touchingHolder = null;
    }

    private void Pick(PlayerStatsHolder holder)
    {
        picked = true;

        Apply(holder);

        // Registry temizliği (Destroy'dan önce)
        if (isRegistered)
        {
            PowerUpRegistry.Unregister(registeredCell);
            isRegistered = false;
        }

        // tekrar tetiklenmesin
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // ✅ SADECE bu powerup prefabını sil
        Destroy(gameObject);
    }

    private void Apply(PlayerStatsHolder holder)
    {
        switch (type)
        {
            case PowerUpType.Speed:
                holder.ApplySpeedBoostTimed(2.0f, 6f);
                break;

            case PowerUpType.BombPower:
                holder.ApplyBombPower(intBonus);
                break;

            case PowerUpType.BombCount:
                holder.ApplyBombCount(intBonus);
                break;
        }
    }

    private void OnDestroy()
    {
        // Eğer herhangi bir sebeple Pick çalışmadan destroy olduysa
        if (isRegistered)
            PowerUpRegistry.Unregister(registeredCell);
    }
}
