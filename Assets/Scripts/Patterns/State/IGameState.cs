namespace DPBomberman.Patterns.State
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Tick(float deltaTime);
    }
}