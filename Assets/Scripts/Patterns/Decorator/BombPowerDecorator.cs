namespace Patterns.Decorator
{
    public class BombPowerDecorator : PlayerStatsDecorator
    {
        private readonly int bonus;

        public BombPowerDecorator(IPlayerStats inner, int bonus) : base(inner)
        {
            this.bonus = bonus;
        }

        public override int BombPower => inner.BombPower + bonus;
        // BombCount ve Speed otomatik inner’dan geliyor ✅
    }
}
