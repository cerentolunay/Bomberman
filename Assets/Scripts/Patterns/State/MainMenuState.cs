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

            // Güvenlik: Pause/GameOver'dan geldiysek oyun akmasýn diye býrakýlan þeyleri temizle
            Time.timeScale = 1f;

            var ui = Object.FindFirstObjectByType<UIManager>();
            if (ui != null)
            {
                ui.HidePause();
                ui.HideGameOver();
            }

            // Eðer zaten MainMenu sahnesinde deðilsek, sahneyi yükle
            var sceneName = SceneManager.GetActiveScene().name.ToLowerInvariant();
            if (!sceneName.Contains("mainmenu"))
            {
                game.GoToMainMenu();
                return;
            }

            // Buraya geliyorsa zaten MainMenu sahnesindesin.
            // Ýstersen burada MainMenu UI enable/disable iþleri yapýlýr.
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit MainMenu");
        }

        public void Tick(float deltaTime)
        {
            // Test amaçlý P ile oyuna geçmek istiyorsan:
            if (Input.GetKeyDown(KeyCode.P))
            {
                // Direkt state deðiþtirmek yerine sahne yüklet:
                // (OnSceneLoaded zaten PlayingState'e geçiriyor)
                game.StartGame(game.selectedTheme);
            }
        }
    }
}
