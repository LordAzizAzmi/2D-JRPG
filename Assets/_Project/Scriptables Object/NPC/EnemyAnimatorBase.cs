using UnityEngine;

namespace WannaBHero.Battle
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimatorBase : MonoBehaviour, IEnemyAnimator
    {
        [Header("Animator Parameter Names")]
        [Tooltip("Isi sesuai nama parameter di Animator Controller enemy ini")]
        [SerializeField] private string paramAttack = "isAttack";
        [SerializeField] private string paramHurt = "isHurt";
        [SerializeField] private string paramDie = "isDie";
        [SerializeField] private string paramMoving = "isMoving"; // ← tambah ini
        [SerializeField] private string paramMoveX = "moveX";   // ← tambah ini

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayAttack()
        {
            animator.SetBool(paramAttack, true);
        }

        public void PlayHurt()
        {
            animator.SetBool(paramHurt, true);
        }

        public void PlayDie()
        {
            animator.SetBool(paramDie, true);
        }

        public void PlayIdle()
        {
            animator.SetBool(paramAttack, false);
            animator.SetBool(paramHurt, false);
        }

        // Dipanggil Animation Event di frame TERAKHIR clip Attack
        public void OnAttackEnd()
        {
            animator.SetBool(paramAttack, false);
        }

        public void PlayWalk(float dirX)
        {
            animator.SetBool(paramMoving, true);
            animator.SetFloat(paramMoveX, dirX);
        }

        public void StopWalk()
        {
            animator.SetBool(paramMoving, false);
        }

        // Dipanggil Animation Event di frame TERAKHIR clip Hurt
        public void OnHurtEnd()
        {
            animator.SetBool(paramHurt, false);
        }
    }
}