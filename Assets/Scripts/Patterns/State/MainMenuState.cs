using UnityEngine;

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
            // TODO (Faz 2+): UI aç, menü inputlarýný aktif et
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit MainMenu");
            // TODO (Faz 2+): UI kapat, temizle
        }

        public void Tick(float deltaTime)
        {
            // Test amaçlý: P basýnca oyuna geç
            if (Input.GetKeyDown(KeyCode.P))
            {
                machine.ChangeState(new PlayingState(game, machine));
            }
        }
    }
}