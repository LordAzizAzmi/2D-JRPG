namespace WannaBHero.Battle
{
    public interface IEnemyAnimator
    {
        void PlayAttack();
        void PlayHurt();
        void PlayDie();
        void PlayIdle();
    }
}