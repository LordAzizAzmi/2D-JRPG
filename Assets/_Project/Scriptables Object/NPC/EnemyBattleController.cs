using System.Collections;
using UnityEngine;

namespace WannaBHero.Battle
{
    // Pasang di root prefab enemy battle.
    // Req: EnemyAnimatorBase (untuk animasi)
    [RequireComponent(typeof(EnemyAnimatorBase))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBattleController : MonoBehaviour, IBattleEntity
    {
        [Header("Gerakan Maju-Mundur")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float walkSpeed = 5f;
        [Tooltip("Jarak tambahan (gap) di luar lebar sprite, supaya tidak nempel Player.")]
        [SerializeField] private float extraGap = 0.3f;

        [Header("Arah Hadap Default")]
        [Tooltip("Atur Rotasi Sprite ")]
        [SerializeField] private bool defaultFacingRight = false;

        [Header("Stats dari ScriptableObject")]
        [SerializeField] private CharacterStatsData stats;

        public string EntityName => stats != null ? stats.characterName : gameObject.name;
        public int MaxHP => stats != null ? stats.maxHP : 100;
        public int AttackPower => stats != null ? stats.attack : 15;
        public int Speed => stats != null ? stats.speed : 25;
        public int Defense => stats != null ? stats.defense : 0;
        public bool IsAlive => currentHP > 0;
        public int CurrentHP => currentHP;

        // Event — notify BattleTurnManager setelah aksi selesai
        public System.Action OnActionEnd;

        // Dipanggil setiap kali HP berubah (damage/heal/battle mulai).
        // Param: (currentHP, maxHP). Dipakai HealthBarUI untuk update Slider + teks.
        public System.Action<int, int> OnHPChanged;

        private int currentHP;
        private Rigidbody2D rb;
        private EnemyAnimatorBase enemyAnim;
        private Vector3 startPos;
        private bool attackAnimDone;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            enemyAnim = GetComponent<EnemyAnimatorBase>();
            currentHP = MaxHP;
        }

        private void Start()
        {
            // Kirim HP awal ke UI begitu battle mulai (bar full).
            OnHPChanged?.Invoke(currentHP, MaxHP);

            startPos = transform.position;

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

            // Mulai dengan menghadap Player (idle awal di battle).
            FaceTarget(playerTransform);
        }

        // Dipanggil oleh BattleTurnManager saat giliran enemy
        public void ExecuteAttack()
        {
            StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            // Step 1 — Jalan maju ke depan Player (masih menghadap Player, natural).
            float gap = GetSpriteHalfWidth(transform) + GetSpriteHalfWidth(playerTransform) + extraGap;
            Vector3 attackPos = new Vector3(playerTransform.position.x + gap, transform.position.y, transform.position.z);
            yield return StartCoroutine(WalkTo(attackPos));

            // Step 2 — Stop walk, mulai attack
            enemyAnim.StopWalk();
            yield return new WaitForSeconds(0.1f);

            attackAnimDone = false;
            enemyAnim.PlayAttack();

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
                player.TakeDamage(AttackPower);
                BattleTurnManager.Instance?.NotifyDamage(player.EntityName, AttackPower);
            }

            // Step 4 — Putar badan menghadap arah BALIK (menjauhi Player)
            FaceDirection(awayFromPlayer: true);
            yield return StartCoroutine(WalkTo(startPos));

            // Step 5 — Sampai di tempat: putar lagi menghadap Player, baru Idle.
            FaceTarget(playerTransform);
            enemyAnim.StopWalk();
            enemyAnim.PlayIdle();

            Debug.Log("[EnemyBattle] Selesai menyerang, balik ke posisi.");
            OnActionEnd?.Invoke();
        }

        /// Jalan ke target TANPA peduli arah hadap 
        private IEnumerator WalkTo(Vector3 target)
        {
            enemyAnim.PlayWalk(transform.localScale.x); // animasi walk, arah hadap sudah diatur sebelumnya

            while (Vector3.Distance(transform.position, target) > 0.02f)
            {
                Vector3 nextPos = Vector3.MoveTowards(transform.position, target, walkSpeed * Time.deltaTime);
                rb.MovePosition(nextPos);
                yield return null;
            }

            rb.MovePosition(target);
        }

        ///Rotasi badan menghadap (atau membelakangi) target tertentu.
        private void FaceTarget(Transform target)
        {
            if (target == null) return;
            bool targetIsToTheRight = target.position.x > transform.position.x;
            SetFacing(faceRight: targetIsToTheRight);
        }

        ///Rotasi badan menjauhi (atau menghadap) Player secara eksplisit.
        private void FaceDirection(bool awayFromPlayer)
        {
            bool playerIsToTheLeft = playerTransform.position.x < transform.position.x;
            // Kalau Player di kiri, "menjauh" berarti hadap kanan, dan sebaliknya.
            bool faceRight = awayFromPlayer ? playerIsToTheLeft : !playerIsToTheLeft;
            SetFacing(faceRight);
        }

        private void SetFacing(bool faceRight)
        {
            Vector3 scale = transform.localScale;
            float absX = Mathf.Abs(scale.x);
            // defaultFacingRight menentukan tanda mana yang berarti "kanan" untuk sprite ini.
            scale.x = (faceRight == defaultFacingRight) ? absX : -absX;
            transform.localScale = scale;
        }

        private float GetSpriteHalfWidth(Transform t)
        {
            var sr = t.GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.bounds.extents.x : 0.5f;
        }

        // Dipanggil Animation Event di frame terakhir clip Attack enemy
        public void OnAttackAnimationEnd()
        {
            attackAnimDone = true;
        }

        // IBattleEntity — menerima damage
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            int actualDamage = Mathf.Max(1, amount - Defense); // minimal 1 damage, sisanya diserap Defense
            currentHP = Mathf.Max(0, currentHP - actualDamage);
            Debug.Log($"[Enemy] {EntityName} kena {amount} damage (Defense {Defense} → actual {actualDamage}). HP: {currentHP}/{MaxHP}");
            OnHPChanged?.Invoke(currentHP, MaxHP);

            if (!IsAlive)
                StartCoroutine(DieRoutine());
            else
                StartCoroutine(HurtRoutine());
        }

        private IEnumerator HurtRoutine()
        {
            enemyAnim.PlayHurt();
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator DieRoutine()
        {
            enemyAnim.PlayDie();
            yield return new WaitForSeconds(1.5f);
            gameObject.SetActive(false);
        }
    }
}