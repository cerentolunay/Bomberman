namespace DPBomberman.Models.Map
{
    public enum CellType
    {
        Empty,          // Hiçbir þey yok (geçilebilir)
        Ground,         // Zemin (üstüne oyuncu basabilir)
        Unbreakable,    // Kýrýlamaz duvar
        Breakable,      // Tek patlamada kýrýlan duvar
        Hard            // Birden fazla patlama isteyen duvar
    }
}
/* Neden enum?

Haritayý Unity objelerine baðlamadan tanýmlýyoruz

Tilemap, prefab, sprite deðiþse bile oyun mantýðý deðiþmez

Patlama “hangi hücreye ne oldu?” sorusunu çok net cevaplar */