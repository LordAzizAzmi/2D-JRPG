using UnityEngine;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class TopDownCharacterController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 5f;

        [Header("Animator Parameter Names")]
        [Tooltip("Harus sama persis (huruf besar/kecil) dengan nama parameter di Animator Controller")]
        [SerializeField] private string paramMoveX = "moveX";
        [SerializeField] private string paramMoveY = "moveY";
        [SerializeField] private string paramIsMoving = "isMoving";
        [SerializeField] private string paramIsAttacking = "isAttacking";
        [SerializeField] private string paramAttack1Trigger = "isAttack1";
        [SerializeField] private string paramAttack2Trigger = "isAttack2";

        private Animator animator;
        private Rigidbody2D rb;
        private bool isAttacking;

        public bool IsAttacking => isAttacking;

        public event System.Action OnAttackEnd;

        // Hold Direction, dipertahankan saat berhenti gerak 
        private Vector2 lastDirection = Vector2.down;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            HandleAttackInput();

            if (isAttacking)
            {
                // Selagi attack, paksa diam dan jangan proses movement sama sekali.
                rb.linearVelocity = Vector2.zero;
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector2 dir = Vector2.zero;

            if (Input.GetKey(KeyCode.A)) dir.x = -1f;
            else if (Input.GetKey(KeyCode.D)) dir.x = 1f;

            if (Input.GetKey(KeyCode.W)) dir.y = 1f;
            else if (Input.GetKey(KeyCode.S)) dir.y = -1f;

            dir.Normalize();
            bool isMoving = dir.magnitude > 0f;

            if (isMoving)
            {
                lastDirection = dir;
            }

            // moveX/moveY tetap value arah TERAKHIR walau sedang diam,
            // supaya Blend Tree Idle dan Attack tahu harus menghadap ke mana.
            animator.SetFloat(paramMoveX, lastDirection.x);
            animator.SetFloat(paramMoveY, lastDirection.y);
            animator.SetBool(paramIsMoving, isMoving);

            rb.linearVelocity = speed * dir;
        }

        private void HandleAttackInput()
        {
            // Selagi animasi attack berjalan, input attack baru diabaikan total.
            // Agar tidak spam attack
            if (isAttacking) return;

            bool attack1Pressed = Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0);
            bool attack2Pressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1);

            if (attack1Pressed)
            {
                StartAttack(paramAttack1Trigger);
            }
            else if (attack2Pressed)
            {
                StartAttack(paramAttack2Trigger);
            }
        }

        private void StartAttack(string triggerName)
        {
            isAttacking = true;
            animator.SetBool(paramIsAttacking, true);
            animator.SetTrigger(triggerName);
        }

        public void OnAttackAnimationEnd()
        {
            isAttacking = false;
            animator.SetBool(paramIsAttacking, false);
            OnAttackEnd?.Invoke();
        }
    }
}