using System.Collections;
using UnityEngine;

namespace WannaBHero.Battle
{
    [RequireComponent(typeof(Animator))]
    public class BattlePlayerController : MonoBehaviour
    {
        [Header("Referensi")]
        [Tooltip("Assign Transform root enemy di scene")]
        [SerializeField] private Transform enemyTransform;

        [Header("Gerakan")]
        [SerializeField] private float walkSpeed = 6f;
        [Tooltip("Jarak berhenti sebelum menyentuh enemy")]
        [SerializeField] private float stopDistance = 1.0f;

        [Header("Animator Parameter Names")]
        [SerializeField] private string paramMoveX = "moveX";
        [SerializeField] private string paramMoveY = "moveY";
        [SerializeField] private string paramIsMoving = "isMoving";
        [SerializeField] private string paramIsAttacking = "isAttacking";
        [SerializeField] private string paramAttack1 = "isAttack1";
        [SerializeField] private string paramAttack2 = "isAttack2";

        private Animator animator;
        private Vector3 startPos;
        private bool isActing;    // lock input selagi aksi berjalan
        private bool attackDone;  // flag dari Animation Event

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            startPos = transform.position;
            SetIdleRight();
        }

        private void Update()
        {
            if (isActing) return;

            if (Input.GetKeyDown(KeyCode.Q))
                StartCoroutine(AttackRoutine(paramAttack1));
            else if (Input.GetKeyDown(KeyCode.E))
                StartCoroutine(AttackRoutine(paramAttack2));
        }

        private void OnEnable()
        {
            BattleEnemySpawner.OnEnemySpawned += OnEnemySpawned;
        }

        private void OnDisable()
        {
            BattleEnemySpawner.OnEnemySpawned -= OnEnemySpawned;
        }

        private void OnEnemySpawned(Transform spawnedEnemy)
        {
            // Auto-assign target enemy setelah di-spawn
            enemyTransform = spawnedEnemy;
            Debug.Log($"[BattlePlayer] Enemy target: {spawnedEnemy.name}");
        }

        // ─────────────────────────────────────
        //  CORE ROUTINE
        // ─────────────────────────────────────

        private IEnumerator AttackRoutine(string attackTrigger)
        {
            isActing = true;

            // Step 1 — Move to Front of Enemy
            Vector3 attackPos = enemyTransform.position + Vector3.left * stopDistance;
            yield return StartCoroutine(WalkTo(attackPos, moveRight: true));

            // Step 2 — Trigger attack
            attackDone = false;
            animator.SetFloat(paramMoveX, 1f);   
            animator.SetFloat(paramMoveY, 0f);
            animator.SetBool(paramIsAttacking, true);
            animator.SetTrigger(attackTrigger);

            // Step 3 — Wait Animation Event OnAttackAnimationEnd()
            yield return new WaitUntil(() => attackDone);

            // Step 4 — Back to Start Position
            yield return StartCoroutine(WalkTo(startPos, moveRight: false));

            // Step 5 — Idle
            SetIdleRight();
            isActing = false;
        }

        // ─────────────────────────────────────
        //  MOVEMENT
        // ─────────────────────────────────────

        private IEnumerator WalkTo(Vector3 target, bool moveRight)
        {
            animator.SetFloat(paramMoveX, moveRight ? 1f : -1f);
            animator.SetFloat(paramMoveY, 0f);
            animator.SetBool(paramIsMoving, true);

            while (Vector3.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    walkSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            animator.SetBool(paramIsMoving, false);
        }

        // ─────────────────────────────────────
        //  ANIMATION EVENT
        //  Pasang di frame TERAKHIR clip Attack1Right & Attack2Right
        // ─────────────────────────────────────

        public void OnAttackAnimationEnd()
        {
            attackDone = true;
            animator.SetBool(paramIsAttacking, false);
        }

        // ─────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────

        private void SetIdleRight()
        {
            animator.SetFloat(paramMoveX, 1f);
            animator.SetFloat(paramMoveY, 0f);
            animator.SetBool(paramIsMoving, false);
            animator.SetBool(paramIsAttacking, false);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (enemyTransform == null) return;
            // Visualisasi titik berhenti attack
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                enemyTransform.position + Vector3.left * stopDistance, 0.2f);
        }
#endif
    }
}