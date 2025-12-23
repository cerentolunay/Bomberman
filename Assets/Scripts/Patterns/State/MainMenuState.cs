using UnityEngine;
using UnityEngine.SceneManagement;

namespace DPBomberman.Patterns.State
{
    public class MainMenuState : IGameState
    {
        private readonly GameManager game;
        private readonly GameStateMachine machine;

        public MainMenuState(GameManager game, GameStateMachine machine)
        {
            this.game = game;
            this.machine = machine;
        }

        public void Enter()
        {
            Debug.Log("[STATE] Enter MainMenu");

            // Güvenlik: Pause/GameOver'dan geldiysek oyun akmasın diye bırakılan şeyleri temizle
            Time.timeScale = 1f;

            var ui = Object.FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                ui.HidePause();
                ui.HideGameOver();
            }

            // Eğer zaten MainMenu sahnesinde değilsek, sahneyi yükle
            var sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();
            if (!sceneName.Contains("mainmenu"))
            {
                game.GoToMainMenu();
                return; // Sahne yüklenince bu state yeniden oluşturulacak, o yüzden burayı kesiyoruz.
            }

            // Buraya geliyorsa zaten MainMenu sahnesindesin.
            // İstersen burada MainMenu UI enable/disable işleri yapılır.
            
            game.SetGameplayInput(false);
            
            // Not: Time.timeScale yukarıda yapıldığı için tekrar yazmaya gerek yok.
            // TODO (Faz 2+): UI aç, menü inputlarını aktif et
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit MainMenu");
        }

        public void Tick(float deltaTime)
        {
            // Test amaçlı P ile oyuna geçmek istiyorsan:
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Direkt state değiştirmek yerine sahne yüklet:
                // (GameManager.OnSceneLoaded zaten PlayingState'e geçiriyor)
                game.StartGame(game.selectedTheme);
            }
        }
    }
}