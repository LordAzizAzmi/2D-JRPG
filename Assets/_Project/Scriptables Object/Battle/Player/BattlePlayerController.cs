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

        [Header("Stats")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int attackPower = 20;

        private int currentHP;
        private bool inputEnabled;

        [Header("Stats dari ScriptableObject")]
        [SerializeField] private CharacterStatsData stats;

        // IBattleEntity properties — baca dari SO
        public string EntityName => stats != null ? stats.characterName : "Player";
        public int MaxHP => stats != null ? stats.maxHP : 100;
        public int AttackPower => stats != null ? stats.attack : 20;
        public int Speed => stats != null ? stats.speed : 50;
        public bool IsAlive => currentHP > 0;
        public int CurrentHP => currentHP;


        private Animator animator;
        private Vector3 startPos;
        private bool isActing;    // lock input selagi aksi berjalan
        private bool attackDone;  // flag dari Animation Event

        public System.Action OnActionEnd;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            currentHP = maxHP;
        }

        private void Start()
        {
            startPos = transform.position;
            SetIdleRight();
        }

        private void Update()
        {
            if (!inputEnabled) return;

            if (isActing) return;

            if (Input.GetKeyDown(KeyCode.Q))
                StartCoroutine(AttackRoutine(paramAttack1));
            else if (Input.GetKeyDown(KeyCode.E))
                StartCoroutine(AttackRoutine(paramAttack2));
        }

        private void OnEnable()
        {
            BattleEnemySpawner.OnEnemySpawned += HandleEnemySpawned;
        }

        private void OnDisable()
        {
            BattleEnemySpawner.OnEnemySpawned -= HandleEnemySpawned;
        }

        private void HandleEnemySpawned(Transform spawnedEnemy)
        {
            enemyTransform = spawnedEnemy;
            Debug.Log($"[BattlePlayer] Enemy target set: {spawnedEnemy.name}");
        }

        // ─────────────────────────────────────
        //  CORE ROUTINE
        // ─────────────────────────────────────

        // Ganti AttackRoutine yang sudah ada
        private IEnumerator AttackRoutine(string attackTrigger)
        {
            isActing = true;

            // Step 1 — Jalan maju
            Vector3 attackPos = enemyTransform.position + Vector3.right * stopDistance;
            yield return StartCoroutine(WalkTo(attackPos, moveRight: true));

            // Step 2 — Animasi attack
            attackDone = false;
            animator.SetFloat(paramMoveX, 1f);
            animator.SetFloat(paramMoveY, 0f);
            animator.SetBool(paramIsAttacking, true);
            animator.SetTrigger(attackTrigger);

            // Tunggu Animation Event ATAU timeout 3 detik
            float elapsed = 0f;
            while (!attackDone && elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!attackDone)
                Debug.LogWarning("[BattlePlayer] Timeout! Pasang Animation Event " +
                                 "'OnAttackAnimationEnd' di frame terakhir clip Attack player.");

            // Step 3 — Deal damage ke enemy
            IBattleEntity enemy = enemyTransform.GetComponent<IBattleEntity>();
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(attackPower);
                BattleTurnManager.Instance?.NotifyDamage(enemy.EntityName, attackPower);
            }

            // Step 4 — Balik ke posisi asal
            yield return StartCoroutine(WalkTo(startPos, moveRight: false));

            // Step 5 — Idle & enable input giliran berikutnya
            SetIdleRight();
            isActing = false;

            Debug.Log("[BattlePlayer] Selesai menyerang.");
            OnActionEnd?.Invoke(); // ← WAJIB ada ini
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

        public void EnableInput(bool enable)
        {
            inputEnabled = enable;
        }

        // Take Damage dari musuh
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[Player] Kena {amount} damage. HP: {currentHP}/{maxHP}");
            // TODO: play hurt animation & update UI HP
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