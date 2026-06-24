using UnityEngine;
using UnityEngine.SceneManagement;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(TopDownCharacterController))]
    public class BattleTransition : MonoBehaviour
    {
        [Header("Deteksi Musuh")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private string enemyTag = "Enemies";

        [Header("Scene")]
        [SerializeField] private string battleScene = "Battle";

        [Header("Transisi (kosongkan dulu jika belum siap)")]
        [Tooltip("Assign script yang implements ISceneTransition.\n" +
                 "Kosong = langsung pindah scene tanpa efek.")]
        [SerializeField] private MonoBehaviour transitionEffect;

        private TopDownCharacterController charController;
        private GameObject pendingEnemy;  // enemy yang menunggu battle
        private bool isTransitioning;

        private void Awake()
        {
            charController = GetComponent<TopDownCharacterController>();
        }

        private void OnEnable()
        {
            // Subscribe saat aktif
            charController.OnAttackEnd += HandleAttackEnd;
        }

        private void OnDisable()
        {
            // Unsubscribe agar tidak memory leak
            charController.OnAttackEnd -= HandleAttackEnd;
        }

        private void Update()
        {
            // Selagi transisi berjalan, abaikan semua input
            if (isTransitioning) return;

            bool attackPressed = Input.GetKeyDown(KeyCode.Q)
                              || Input.GetMouseButtonDown(0)
                              || Input.GetKeyDown(KeyCode.E)
                              || Input.GetMouseButtonDown(1);

            if (attackPressed)
                CheckEnemyInRange();
        }

        private void CheckEnemyInRange()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position, attackRange);

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag(enemyTag)) continue;

                // Tandai enemy — tunggu animasi attack selesai
                pendingEnemy = hit.gameObject;
                return;
            }
        }

        // Dipanggil otomatis oleh event CharController.OnAttackEnd
        private void HandleAttackEnd()
        {
            // Tidak ada enemy dalam range saat attack → abaikan
            if (pendingEnemy == null) return;

            // Jaga agar tidak double-trigger
            if (isTransitioning) return;
            isTransitioning = true;

            // Cek apakah slot transisi terisi
            ISceneTransition transition = transitionEffect as ISceneTransition;

            if (transition != null)
            {
                // Ada transisi → jalankan dulu, pindah scene saat callback
                transition.PlayTransition(GoToBattle);
            }
            else
            {
                // Slot kosong → langsung pindah
                GoToBattle();
            }
        }

        private void GoToBattle()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            BattleManager.Instance.StartBattle(pendingEnemy, currentScene);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}