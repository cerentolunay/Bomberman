using UnityEngine;

namespace DPBomberman.Patterns.State
{
    public class GameOverState : IGameState
    {
        private readonly GameManager game;
        private readonly GameStateMachine machine;

        public GameOverState(GameManager game, GameStateMachine machine)
        {
            this.game = game;
            this.machine = machine;
        }

        public void Enter()
        {
            Debug.Log("[STATE] Enter GameOver");
            Time.timeScale = 0f; //oyunu durdurur
            game.uiManager?.ShowGameOver();

        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit GameOver");
            Time.timeScale = 1f;          // çýkarken geri aç
        }

        public void Tick(float deltaTime)
        {
            // Ýstersen burada input ile menüye dön/yeniden baþlat ekleriz.
        }
    }
}
