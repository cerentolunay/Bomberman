using UnityEngine;
using DPBomberman.Controllers;
using DPBomberman.Commands;
using System.Collections;         // Coroutine için gerekli (Replay)
using System.Collections.Generic; // List için gerekli (History)

namespace DPBomberman.InputSystem
{
    public class InputHandler : MonoBehaviour
    {
        [Header("Key Bindings")]
        public KeyCode upKey = KeyCode.W;
        public KeyCode downKey = KeyCode.S;
        public KeyCode leftKey = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode bombKey = KeyCode.Space;
        public KeyCode replayKey = KeyCode.R;

        [Header("Receiver")]
        [SerializeField] private PlayerController player;

        [Header("State Gate")]
        [SerializeField] private bool inputEnabled = true;

        // --- B KİŞİSİ EKLEMESİ: Replay Değişkenleri ---
        private List<ICommand> commandHistory = new List<ICommand>(); // 
        private bool isReplaying = false;
        private Vector3 startPosition; // Replay başlayınca oyuncuyu buraya alacağız [cite: 144]
        // ----------------------------------------------

        private ICommand moveUp;
        private ICommand moveDown;
        private ICommand moveLeft;
        private ICommand moveRight;
        private ICommand placeBomb;

        private void Awake()
        {
            if (player == null) player = Object.FindFirstObjectByType<PlayerController>();

            // Oyun başladığındaki konumu kaydet (Replay için gerekli)
            if (player != null) startPosition = player.transform.position;

            BuildCommands();
        }

        private void BuildCommands()
        {
            // EĞER OYUNCU YOKSA PES ETME, SADECE BEKLE.
            if (player == null) return;

            moveUp = new MoveUpCommand(player);
            moveDown = new MoveDownCommand(player);
            moveLeft = new MoveLeftCommand(player);
            moveRight = new MoveRightCommand(player);
            placeBomb = new PlaceBombCommand(player);
        }

        public void SetInputEnabled(bool enabledValue)
        {
            inputEnabled = enabledValue;
        }

        // --- B KİŞİSİ EKLEMESİ: Komut Çalıştırma ve Kaydetme ---
        // Her seferinde hem çalıştırıp hem listeye eklemek için bu yardımcı metodu kullanacağız.
        private void ExecuteAndRecord(ICommand command)
        {
            command.Execute();                  // Komutu çalıştır
            commandHistory.Add(command);        // Listeye kaydet 
            // Debug.Log($"History Count: {commandHistory.Count}"); // İstersen açabilirsin [cite: 132]
        }
        // -------------------------------------------------------

        private void Update()
        {
            // 1. OYUNCU KONTROLÜ (EKSİKSE BULMAYA ÇALIŞ)
            if (player == null)
            {
                player = Object.FindFirstObjectByType<PlayerController>();

                // Eğer şimdi bulduysak hemen komutları kur
                if (player != null)
                {
                    BuildCommands();
                    // Replay için başlangıç pozisyonunu da alalım
                    startPosition = player.transform.position;
                }
                else
                {
                    // Hala oyuncu yoksa (Main Menu'deyizdir), işlem yapma
                    return;
                }
            }

            // OYUNCU ÖLDÜYSE VEYA REPLAY VARSA DUR
            if (isReplaying) return;
            if (!inputEnabled) return;
            if (player.IsDead()) return;

            // --- B KİŞİSİ EKLEMESİ: Replay Tetikleyici ---
            if (Input.GetKeyDown(replayKey))
            {
                StartCoroutine(StartReplayRoutine());
                return;
            }

            // HAREKETLER (Komutlar null değilse çalıştır)
            if (moveUp != null && (Input.GetKeyDown(upKey) || Input.GetKeyDown(KeyCode.UpArrow)))
                ExecuteAndRecord(moveUp);

            else if (moveDown != null && (Input.GetKeyDown(downKey) || Input.GetKeyDown(KeyCode.DownArrow)))
                ExecuteAndRecord(moveDown);

            else if (moveLeft != null && (Input.GetKeyDown(leftKey) || Input.GetKeyDown(KeyCode.LeftArrow)))
                ExecuteAndRecord(moveLeft);

            else if (moveRight != null && (Input.GetKeyDown(rightKey) || Input.GetKeyDown(KeyCode.RightArrow)))
                ExecuteAndRecord(moveRight);

            // BOMBA
            if (placeBomb != null && Input.GetKeyDown(bombKey))
                ExecuteAndRecord(placeBomb);
        }

        // --- B-2 KİŞİSİ EKLEMESİ: Replay Mantığı (Coroutine) ---

        private IEnumerator StartReplayRoutine()
        {
            isReplaying = true;
            Debug.Log("Replay Başlıyor! Komut Sayısı: " + commandHistory.Count);

            // --- BURAYI EKLE: SAHA TEMİZLİĞİ ---
            // Sahnedeki patlamaya hazır bekleyen tüm bombaları bul
            BombController[] activeBombs = Object.FindObjectsByType<BombController>(FindObjectsSortMode.None);

            // Hepsini tek tek yok et ki oyuncu replay yaparken onlara çarpmasın
            foreach (var bomb in activeBombs)
            {
                if (bomb != null) Destroy(bomb.gameObject);
            }
            // ------------------------------------

            // 1. Oyuncuyu en başa ışınla
            player.transform.position = startPosition;

            // 2. Biraz bekle (Görsel olarak algılamak için)
            yield return new WaitForSeconds(0.5f);

            // 3. Tarihçedeki tüm komutları sırayla çalıştır
            foreach (ICommand cmd in commandHistory)
            {
                cmd.Execute();

                // Her hareket arasında bekle ki animasyon gibi görünsün
                yield return new WaitForSeconds(0.2f);
            }

            Debug.Log("Replay Bitti.");
            isReplaying = false;

            // Replay bitince listeyi temizliyoruz
            commandHistory.Clear();
        }

        // --- B-3: INPUT POLISHING (DEBUG UI) ---
        // Bu fonksiyon Unity'de Canvas kullanmadan hızlıca ekrana yazı yazmanı sağlar.
        // Amaç: Sistemin çalışıp çalışmadığını gözlemlemek (Kontrol Listesi B-3)
        private void OnGUI()
        {
            // Sol üst köşede 250x120 boyutunda bir alan aç
            GUILayout.BeginArea(new Rect(10, 10, 250, 150), GUI.skin.box);

            GUILayout.Label("<b>COMMAND PATTERN DEBUG</b>"); // Başlık

            // Durum Göstergesi
            if (isReplaying)
            {
                GUI.color = Color.red; // Replay sırası kırmızı yazı
                GUILayout.Label($"STATUS: REPLAYING ↺");
            }
            else
            {
                GUI.color = Color.green; // Normal mod yeşil yazı
                GUILayout.Label($"STATUS: RECORDING ●");
            }
            GUI.color = Color.white; // Rengi normale döndür

            // İstatistikler
            GUILayout.Label($"Total Commands: {commandHistory.Count}");

            // Son komutu gösterme (Opsiyonel Cila)
            if (commandHistory.Count > 0)
            {
                // Son eklenen komutun tipini yazdır (örn: MoveUpCommand)
                string lastCmd = commandHistory[commandHistory.Count - 1].GetType().Name;
                GUILayout.Label($"Last Action: {lastCmd}");
            }
            else
            {
                GUILayout.Label("Last Action: None");
            }

            GUILayout.Space(10);
            GUILayout.Label("Press 'R' to Start Replay");

            GUILayout.EndArea();
        }
        
    }
}