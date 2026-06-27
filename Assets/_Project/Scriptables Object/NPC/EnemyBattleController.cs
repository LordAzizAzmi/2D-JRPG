using System.Collections;
using UnityEngine;

namespace WannaBHero.Battle
{
    // Pasang di root prefab enemy battle
    // Butuh: EnemyAnimatorBase (untuk animasi)
    [RequireComponent(typeof(EnemyAnimatorBase))]
    public class EnemyBattleController : MonoBehaviour, IBattleEntity
    {
        [Header("Stats")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int attackPower = 15;

        [Header("Gerakan Maju-Mundur")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float stopDistance = 1.2f;


        [Header("Stats dari ScriptableObject")]
        [SerializeField] private CharacterStatsData stats;

        public string EntityName => stats != null ? stats.characterName : gameObject.name;
        public int MaxHP => stats != null ? stats.maxHP : 100;
        public int AttackPower => stats != null ? stats.attack : 15;
        public int Speed => stats != null ? stats.speed : 25;
        public bool IsAlive => currentHP > 0;
        public int CurrentHP => currentHP;


        // Event — notify BattleTurnManager setelah aksi selesai
        public System.Action OnActionEnd;

        private int currentHP;
        private EnemyAnimatorBase enemyAnim; // ← IEnemyAnimator dipakai di sini
        private Vector3 startPos;
        private bool attackAnimDone;

        private void Awake()
        {
            enemyAnim = GetComponent<EnemyAnimatorBase>();
            currentHP = maxHP;
        }

        private void Start()
        {
            startPos = transform.position;

            // Auto-find player via tag
            if (playerTransform == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null)
                {
                    playerTransform = p.transform;
                    Debug.Log($"[EnemyBattle] Player ditemukan: {p.name}");
                }
                else
                {
                    Debug.LogError("[EnemyBattle] Player tidak ditemukan! " +
                                   "Pastikan Player tag = 'Player'");
                }
            }
        }

        // Dipanggil oleh BattleTurnManager saat giliran enemy
        public void ExecuteAttack()
        {
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            // Step 1 — Jalan ke player DENGAN animasi walk
            Vector3 attackPos = playerTransform.position + Vector3.right * stopDistance;
            yield return StartCoroutine(WalkTo(attackPos, walkingRight: false));

            // Step 2 — Stop walk, mulai attack
            enemyAnim.PlayIdle();
            yield return new WaitForSeconds(0.1f);

            attackAnimDone = false;
            enemyAnim.PlayAttack();

            // Tunggu Animation Event ATAU timeout
            float elapsed = 0f;
            while (!attackAnimDone && elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!attackAnimDone)
                Debug.LogWarning("[EnemyBattle] Timeout! Pasang Animation Event " +
                                 "'OnAttackAnimationEnd' di frame terakhir clip Attack enemy.");

            // Step 3 — Deal damage
            IBattleEntity player = playerTransform.GetComponent<IBattleEntity>();
            if (player != null && player.IsAlive)
            {
                player.TakeDamage(attackPower);
                BattleTurnManager.Instance?.NotifyDamage(player.EntityName, attackPower);
            }

            // Step 4 — Reset attack state
            enemyAnim.OnAttackEnd();

            // Step 5 — Balik ke posisi asal DENGAN animasi walk
            yield return StartCoroutine(WalkTo(startPos, walkingRight: true));

            // Step 6 — Idle & notify
            enemyAnim.PlayIdle();
            Debug.Log("[EnemyBattle] Selesai menyerang, balik ke posisi.");
            OnActionEnd?.Invoke();
        }

        // WalkTo dengan parameter arah untuk animasi
        private IEnumerator WalkTo(Vector3 target, bool walkingRight)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetBool("isMoving", true);
                anim.SetFloat("moveX", walkingRight ? 1f : -1f);
            }

            while (Vector3.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target,
                    walkSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;

            if (anim != null)
                anim.SetBool("isMoving", false);
        }

        // Dipanggil Animation Event di frame terakhir clip Attack enemy
        public void OnAttackAnimationEnd()
        {
            attackAnimDone = true;
            enemyAnim.OnAttackEnd();
        }

        // IBattleEntity — menerima damage
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            currentHP = Mathf.Max(0, currentHP - amount);
            Debug.Log($"[Enemy] {EntityName} kena {amount} damage. HP: {currentHP}/{maxHP}");

            if (!IsAlive)
            {
                StartCoroutine(DieRoutine());
            }
            else
            {
                StartCoroutine(HurtRoutine());
            }
        }

        private IEnumerator HurtRoutine()
        {
            enemyAnim.PlayHurt(); // ← IEnemyAnimator dipakai di sini
            yield return new WaitForSeconds(0.5f);
            enemyAnim.OnHurtEnd();
        }

        private IEnumerator DieRoutine()
        {
            enemyAnim.PlayDie(); // ← IEnemyAnimator dipakai di sini
            yield return new WaitForSeconds(1.5f);
            gameObject.SetActive(false);
        }

        private IEnumerator WalkTo(Vector3 target)
        {
            Animator anim = GetComponent<Animator>();

            // Gunakan isMoving saja — tanpa moveX
            if (anim != null) anim.SetBool("isMoving", true);

            while (Vector3.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target,
                    walkSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = target;
            if (anim != null) anim.SetBool("isMoving", false);
        }
    }
}