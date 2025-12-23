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

            // UI aç
            if (game.uiManager == null)
                Debug.LogError("[PausedState] game.uiManager is NULL (BindSceneReferences çalýþtý mý?)");
            else
                Debug.Log($"[PausedState] UIManager={game.uiManager.name} id={game.uiManager.GetInstanceID()}");

            game.uiManager?.ShowPause();

            // oyunu durdur
            Time.timeScale = 0f;
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit Paused");

            // UI kapat
            game.uiManager?.HidePause();

            // oyunu devam ettir
            Time.timeScale = 1f;
        }

        public void Tick(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                machine.ChangeState(new PlayingState(game, machine));

            if (Input.GetKeyDown(KeyCode.M))
            {
                Time.timeScale = 1f;
                game.GoToMainMenu();
            }
        }
    }
}