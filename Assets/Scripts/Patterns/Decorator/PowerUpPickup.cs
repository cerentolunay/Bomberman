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

    // ARTIK TUŞA GEREK YOK
    // public KeyCode pickupKey = KeyCode.E; 

    private Vector3Int registeredCell;
    private bool isRegistered;
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

    // UPDATE FONKSİYONUNU SİLDİK (Tuş beklemiyoruz)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        // PlayerStatsHolder'ı bulmaya çalış
        var holder = other.GetComponentInParent<PlayerStatsHolder>();
        if (holder == null) holder = other.GetComponent<PlayerStatsHolder>();

        // Eğer çarpan şey Player ise ve StatsHolder'ı varsa:
        if (holder != null)
        {
            Pick(holder); // DİREKT AL!
        }
    }

    // OnTriggerExit SİLİNDİ (Artık bekleme yok)

    private void Pick(PlayerStatsHolder holder)
    {
        picked = true;

        Apply(holder);

        // Registry temizliği
        if (isRegistered)
        {
            PowerUpRegistry.Unregister(registeredCell);
            isRegistered = false;
        }

        // Collider kapat
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Görseli kapat (SpriteRenderer) - Ses çalacaksan destroy hemen olmamalı
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite) sprite.enabled = false;

        // Obje siliniyor
        Destroy(gameObject, 0.1f); // Ses varsa biraz gecikmeli silinebilir
    }

    private void Apply(PlayerStatsHolder holder)
    {
        switch (type)
        {
            case PowerUpType.Speed:
                holder.ApplySpeedBoostTimed(2.0f, 6f);
                Debug.Log("HIZLANDI!");
                break;

            case PowerUpType.BombPower:
                holder.ApplyBombPower(intBonus);
                Debug.Log("GÜÇLENDİ!");
                break;

            case PowerUpType.BombCount:
                holder.ApplyBombCount(intBonus);
                Debug.Log("BOMBA ARTTI!");
                break;
        }
    }

    private void OnDestroy()
    {
        if (isRegistered)
            PowerUpRegistry.Unregister(registeredCell);
    }
}