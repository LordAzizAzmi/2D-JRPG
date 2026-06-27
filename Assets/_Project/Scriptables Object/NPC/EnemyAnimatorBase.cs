using UnityEngine;

namespace WannaBHero.Battle
{
    [RequireComponent(typeof(Animator))]
    public class EnemyAnimatorBase : MonoBehaviour, IEnemyAnimator
    {
        [Header("Animator Parameter Names")]
        [Tooltip("MUST BE parameter tipe Trigger di Animator Controller, ")]
        [SerializeField] private string paramAttack = "isAttack";
        [SerializeField] private string paramHurt = "isHurt";
        [SerializeField] private string paramDie = "isDie";

        [Tooltip("Dua parameter ini TETAP Bool/Float seperti biasa -- gerakan jalan " +
                 "memang perlu bertahan selama beberapa frame, beda dari attack/hurt/die " +
                 "yang sifatnya sesaat (one-shot).")]
        [SerializeField] private string paramMoving = "isMoving";
        [SerializeField] private string paramMoveX = "moveX";

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void PlayAttack() => animator.SetTrigger(paramAttack);
        public void PlayHurt() => animator.SetTrigger(paramHurt);
        public void PlayDie() => animator.SetTrigger(paramDie);

        public void PlayIdle()
        {
            // idle
        }

        // Trigger sudah auto-reset 
        public void OnAttackEnd() { }
        public void OnHurtEnd() { }

        public void PlayWalk(float dirX)
        {
            animator.SetBool(paramMoving, true);
            animator.SetFloat(paramMoveX, dirX);
        }

        public void StopWalk()
        {
            animator.SetBool(paramMoving, false);
        }
    }
}