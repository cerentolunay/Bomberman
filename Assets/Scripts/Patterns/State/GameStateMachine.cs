using DPBomberman.Patterns.State;

namespace DPBomberman.Patterns.State
{
    public class GameStateMachine
    {
        public IGameState CurrentState { get; private set; }

        public void ChangeState(IGameState newState)
        {
            if (newState == null) return;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Tick(float deltaTime)
        {
            CurrentState?.Tick(deltaTime);
        }
    }
}