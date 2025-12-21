namespace Patterns.Decorator
{
    public abstract class PlayerStatsDecorator : IPlayerStats
    {
        protected readonly IPlayerStats inner;

        protected PlayerStatsDecorator(IPlayerStats inner)
        {
            this.inner = inner;
        }

        public virtual float Speed => inner.Speed;
        public virtual int BombPower => inner.BombPower;
        public virtual int BombCount => inner.BombCount;
    }
}
