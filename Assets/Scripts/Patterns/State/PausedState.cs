using UnityEngine;

namespace DPBomberman.Patterns.State
{
    public class PausedState : IGameState
    {
        private readonly GameManager game;
        private readonly GameStateMachine machine;

        public PausedState(GameManager game, GameStateMachine machine)
        {
            this.game = game;
            this.machine = machine;
        }

        public void Enter()
        {
            Debug.Log("[STATE] Enter Paused");

            // UI Manager kontrolü
            if (game.uiManager == null)
                Debug.LogError("[PausedState] game.uiManager is NULL (BindSceneReferences çalıştı mı?)");
            else
                Debug.Log($"[PausedState] UIManager={game.uiManager.name} id={game.uiManager.GetInstanceID()}");

            // UI aç
            game.uiManager?.ShowPause();

            // Oyunu durdur: Input'u kes ve Zamanı durdur
            game.SetGameplayInput(false);
            Time.timeScale = 0f;
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit Paused");

            // UI kapat
            game.uiManager?.HidePause();

            // Oyunu devam ettir (TimeScale normale dönsün)
            Time.timeScale = 1f;
        }

        public void Tick(float deltaTime)
        {
            // Escape basılırsa oyuna dön (PlayingState)
            if (Input.GetKeyDown(KeyCode.Escape))
                machine.ChangeState(new PlayingState(game, machine));

            // M basılırsa Ana Menüye dön
            if (Input.GetKeyDown(KeyCode.M))
            {
                // Önlem: Sahne yüklerken zamanın akması iyidir
                Time.timeScale = 1f;
                game.GoToMainMenu();
            }
        }
    }
}