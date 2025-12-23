using UnityEngine;

namespace DPBomberman.Patterns.State
{
    public class PlayingState : IGameState
    {
        private readonly GameManager game;
        private readonly GameStateMachine machine;

        public PlayingState(GameManager game, GameStateMachine machine)
        {
            this.game = game;
            this.machine = machine;
        }

        public void Enter()
        {
            Debug.Log("[STATE] Enter Playing");
            // TODO (Faz 1-2): MapSpawner çaðýr, oyuncu spawn et (Unity tarafý baðlayacak)
            
            if (game.mapGenerator == null)
            {
                Debug.LogError("[PlayingState] mapGenerator is NULL. Scene' de MapGenerator yok veya aktif deðil.");
                return;
            }

            if (game.mapLogicAdapter == null)
            {
                Debug.LogError("[PlayingState] mapLogicAdapter is NULL. Scene' de MapLogicAdapter yok veya aktif deðil.");
                return;
            }
            // Factory'yi GameManager'dan alýp MapGenerator'a parametre olarak veriyoruz
            game.mapGenerator.GenerateMap(game.CurrentTileFactory);

            game.mapLogicAdapter.BuildLogicMap(
                game.mapGenerator.width,
                game.mapGenerator.height
            );
        }

        public void Exit()
        {
            Debug.Log("[STATE] Exit Playing");
        }

        public void Tick(float deltaTime)
        {
            // Test amaçlý: ESC basýnca pause
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                machine.ChangeState(new PausedState(game, machine));
            }

            // Test amaçlý: M basýnca menüye dön
            if (Input.GetKeyDown(KeyCode.M))
            {
                machine.ChangeState(new MainMenuState(game, machine));
            }
        }
    }
}