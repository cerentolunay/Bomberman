namespace DPBomberman.Walls
{
    public interface IDamageable
    {
        void ApplyDamage(int amount);
        bool IsDestroyed { get; }
    }
}