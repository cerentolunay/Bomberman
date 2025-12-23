namespace Patterns.Decorator
{
    public class BombCountDecorator : PlayerStatsDecorator
    {
        private readonly int bonus;

        public BombCountDecorator(IPlayerStats inner, int bonus) : base(inner)
        {
            this.bonus = bonus;
        }

        public override int BombCount => inner.BombCount + bonus;
    }
}
