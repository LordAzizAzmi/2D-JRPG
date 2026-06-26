using UnityEngine;

namespace WannaBHero.Battle
{
    // add empty Object scene + add "EnemySpawnPoint"
    public class BattleEnemySpawner : MonoBehaviour
    {
        [Header("Spawn")]
        [Tooltip("Geser GO ini di Scene view untuk atur posisi enemy di battle")]
        [SerializeField] private Transform spawnPoint;

        // Event untuk notify BattlePlayerController setelah spawn
        public static System.Action<Transform> OnEnemySpawned;

        private void Start()
        {
            if (BattleManager.Instance == null)
            {
                Debug.LogWarning("[BattleEnemySpawner] BattleManager tidak ditemukan. " +
                                 "Mungkin sedang testing langsung dari battle scene.");
                return;
            }

            if (BattleManager.Instance.enemyBattlePrefab == null)
            {
                Debug.LogError("[BattleEnemySpawner] enemyBattlePrefab null — " +
                               "cek EnemyIdentifier di enemy overworld.");
                return;
            }

            Vector3 pos = spawnPoint != null
                ? spawnPoint.position
                : transform.position;

            GameObject enemy = Instantiate(
                BattleManager.Instance.enemyBattlePrefab,
                pos,
                Quaternion.identity);

            // Notify BattlePlayerController agar otomatis assign target
            OnEnemySpawned?.Invoke(enemy.transform);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 0.4f, "Enemy Spawn");
        }
#endif
    }
}