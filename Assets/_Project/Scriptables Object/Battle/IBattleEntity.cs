namespace WannaBHero.Battle
{
    public interface IBattleEntity
    {
        string EntityName { get; }
        int CurrentHP { get; }
        int MaxHP { get; }
        int AttackPower { get; }
        bool IsAlive { get; }

        void TakeDamage(int amount);
    }
}