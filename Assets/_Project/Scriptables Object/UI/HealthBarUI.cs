using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WannaBHero.Battle
{
    /// Script For "Health Bar" 
    public class HealthBarUI : MonoBehaviour
    {
        private enum TargetType { Player, Enemy }

        [Header("Target")]
        [Tooltip("Player = baca dari BattlePlayerController (cari via tag Player).\n" +
                 "Enemy = baca dari EnemyBattleController (di-assign otomatis saat enemy spawn).")]
        [SerializeField] private TargetType targetType = TargetType.Player;

        [Header("UI Refs")]
        [Tooltip("Slider 'Bar_MP' di bawah Health Bar ini")]
        [SerializeField] private Slider slider;

        [Tooltip("Text (TMP) untuk angka HP, misal '100 / 100'. Boleh dikosongkan kalau tidak perlu.")]
        [SerializeField] private TMP_Text hpText;

        [Header("Opsional")]
        [Tooltip("Animasi halus saat bar berkurang/bertambah. 0 = langsung snap tanpa animasi.")]
        [SerializeField] private float smoothSpeed = 8f;

        private BattlePlayerController playerController;
        private EnemyBattleController enemyController;
        private float targetSliderValue = 1f;
        private bool useSmoothing => smoothSpeed > 0f;

        private void Awake()
        {
            // Auto-grab kalau belum di-assign manual di Inspector
            if (slider == null) slider = GetComponentInChildren<Slider>();
            if (hpText == null)
            {
                var texts = GetComponentsInChildren<TMP_Text>(true);
                if (texts.Length > 0) hpText = texts[texts.Length - 1];
            }
        }

        private void OnEnable()
        {
            if (targetType == TargetType.Player)
            {
                // Player sudah ada di scene dari awal (bukan spawn runtime).
                playerController = FindAnyObjectByType<BattlePlayerController>();
                if (playerController != null)
                {
                    playerController.OnHPChanged += HandleHPChanged;
                }
                else
                {
                    Debug.LogWarning("[HealthBarUI] BattlePlayerController tidak ditemukan di scene.");
                }
            }
            else
            {
                // Enemy spawn setelah BattleEnemySpawner jalan, jadi subscribe ke event spawn-nya.
                BattleEnemySpawner.OnEnemySpawned += HandleEnemySpawned;
                enemyController = FindAnyObjectByType<EnemyBattleController>();
                if (enemyController != null)
                    SubscribeEnemy(enemyController);
            }
        }

        private void OnDisable()
        {
            if (playerController != null)
                playerController.OnHPChanged -= HandleHPChanged;

            if (enemyController != null)
                enemyController.OnHPChanged -= HandleHPChanged;

            BattleEnemySpawner.OnEnemySpawned -= HandleEnemySpawned;
        }

        private void Update()
        {
            // Animasi halus slider menuju value sebenarnya (boleh dimatikan via smoothSpeed = 0)
            if (useSmoothing && slider != null && !Mathf.Approximately(slider.value, targetSliderValue))
            {
                slider.value = Mathf.MoveTowards(slider.value, targetSliderValue, smoothSpeed * Time.deltaTime);
            }
        }

        private void HandleEnemySpawned(Transform spawnedEnemy)
        {
            EnemyBattleController ctrl = spawnedEnemy.GetComponent<EnemyBattleController>();
            if (ctrl == null)
            {
                Debug.LogWarning("[HealthBarUI] Enemy yang spawn tidak punya EnemyBattleController.");
                return;
            }
            SubscribeEnemy(ctrl);
        }

        private void SubscribeEnemy(EnemyBattleController ctrl)
        {
            // Lepas subscription lama kalau ada (jaga-jaga ganti target)
            if (enemyController != null)
                enemyController.OnHPChanged -= HandleHPChanged;

            enemyController = ctrl;
            enemyController.OnHPChanged += HandleHPChanged;
        }

        private void HandleHPChanged(int currentHP, int maxHP)
        {
            float normalized = maxHP > 0 ? (float)currentHP / maxHP : 0f;

            if (slider != null)
            {
                // Pastikan slider pakai range 0..1 (Min Value=0, Max Value=1 di Inspector,
                // sesuai default Slider Unity). Kalau mau slider pakai range 0..maxHP
                // langsung, tinggal ganti baris ini jadi: slider.maxValue = maxHP; slider.value = currentHP;
                if (useSmoothing)
                    targetSliderValue = normalized;
                else
                    slider.value = normalized;
            }

            if (hpText != null)
            {
                hpText.text = $"{currentHP} / {maxHP}";
            }
        }
    }
}