namespace Patterns.Decorator
{
    public class SpeedBoostDecorator : PlayerStatsDecorator
    {
        private readonly float bonus;

        public SpeedBoostDecorator(IPlayerStats inner, float bonus) : base(inner)
        {
            this.bonus = bonus;
        }

        public override float Speed => inner.Speed + bonus;
    }
}
