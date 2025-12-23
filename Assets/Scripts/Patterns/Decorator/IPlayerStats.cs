namespace Patterns.Decorator
{
    public interface IPlayerStats
    {
        float Speed { get; }
        int BombCount { get; }
        int BombPower { get; }
    }
}
