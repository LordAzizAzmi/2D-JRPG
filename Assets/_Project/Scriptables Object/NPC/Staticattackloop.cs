using UnityEngine;

namespace WannaBHero.Characters
{
    //FOR NPC STATIS , yang cuma punya 1 animasi serangan, dan looping terus menerus.

    [RequireComponent(typeof(Animator))]
    public class StaticAttackLoop : MonoBehaviour
    {
        [Header("Animator")]
        [Tooltip("Trigger value di Animator for Attack")]
        [SerializeField] private string attackTriggerName = "Attack1";

        [Tooltip("Trigger value di Animator for Idle")]
        [SerializeField] private string idleStateName = "Idle";

        [Header("Timing")]
        [Tooltip("Hold idle before attacking again." +
                 "0 = looping")]
        [SerializeField] private float delayBetweenAttacks = 0f;

        [Header("Arah Hadap")]
        [Tooltip("Direction" + "Centang kalau sprite asetnya secara default menghadap KANAN, " +
                 "dan kamu ingin NPC ini menghadap KIRI. Sprite akan di-flip otomatis saat Start.")]
        [SerializeField] private bool facingLeft = false;

        private Animator animator;
        private bool waitingForAttackToFinish;
        private float delayTimer;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (facingLeft)
            {
                // Flip sprite secara horizontal. if default asset right direction
                Vector3 scale = transform.localScale;
                transform.localScale = new Vector3(-Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }

        private void Update()
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            bool inIdle = state.IsName(idleStateName);

            if (inIdle)
            {
                if (waitingForAttackToFinish)
                {
                    // Back to Idle, trigger attack again after delay
                    waitingForAttackToFinish = false;
                    delayTimer = delayBetweenAttacks;
                }

                if (delayTimer > 0f)
                {
                    delayTimer -= Time.deltaTime;
                    return;
                }

                animator.SetTrigger(attackTriggerName);
                waitingForAttackToFinish = true; // no trigger dobel di frame yang sama
            }
        }
    }
}