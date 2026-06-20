using UnityEngine;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class CharController : MonoBehaviour
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

        [Header("Attack Safety Net")]
        [Tooltip("Jaring pengaman: durasi maksimal lock attack sebelum dipaksa lepas, " +
                 "untuk mencegah stuck permanen kalau Animation Event lupa dipasang di clip. " +
                 "Set sedikit lebih lama dari durasi clip attack terlama (detik).")]
        [SerializeField] private float maxAttackLockDuration = 1f;

        private Animator animator;
        private Rigidbody2D rb;

        // True selagi animasi attack berjalan. Selama true,
        // input movement & attack baru diabaikan total (lock).
        private bool isAttacking;

        // Waktu (Time.time) saat attack mulai, dipakai fallback timer di bawah.
        private float attackStartTime;

        // Arah hadap terakhir, dipertahankan saat berhenti gerak
        // supaya Blend Tree Idle/Attack tetap menghadap arah yang benar.
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
                // Fallback: kalau Animation Event lupa dipasang di clip attack,
                // lock dilepas otomatis setelah durasi ini, supaya tidak stuck selamanya.
                if (Time.time - attackStartTime >= maxAttackLockDuration)
                {
                    OnAttackAnimationEnd();
                }

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

            // moveX/moveY tetap berisi arah TERAKHIR walau sedang diam,
            // supaya Blend Tree Idle dan Attack tahu harus menghadap ke mana.
            animator.SetFloat(paramMoveX, lastDirection.x);
            animator.SetFloat(paramMoveY, lastDirection.y);
            animator.SetBool(paramIsMoving, isMoving);

            rb.linearVelocity = speed * dir;
        }

        private void HandleAttackInput()
        {
            // Selagi animasi attack berjalan, input attack baru diabaikan total.
            // Inilah yang mencegah spam klik memotong animasi yang sedang berjalan.
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
            attackStartTime = Time.time;
            animator.SetBool(paramIsAttacking, true);
            animator.SetTrigger(triggerName);
        }

        /// <summary>
        /// Dipanggil otomatis oleh Animator lewat transition "Has Exit Time"
        /// dari state Attack1/Attack2 balik ke Idle. Pasang sebagai
        /// StateMachineBehaviour ATAU panggil manual via Animation Event
        /// di frame terakhir clip attack (lihat catatan di bawah).
        /// </summary>
        public void OnAttackAnimationEnd()
        {
            isAttacking = false;
            animator.SetBool(paramIsAttacking, false);
        }
    }
}