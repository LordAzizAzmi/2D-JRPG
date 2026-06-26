using UnityEngine;

namespace WannaBHero.Battle
{
    public class BattleEnemySpawner : MonoBehaviour
    {
        [Header("Spawn Point")]
        [SerializeField] private Transform spawnPoint;

        [Header("Fallback — HANYA untuk testing langsung dari Battle scene")]
        [SerializeField] private GameObject fallbackEnemyPrefab;

        public static System.Action<Transform> OnEnemySpawned;

        private void Start()
        {
            GameObject prefabToSpawn = GetPrefabToSpawn();

            if (prefabToSpawn == null)
            {
                Debug.LogError(
                    "[BattleEnemySpawner] Tidak ada prefab!\n" +
                    "Jika testing dari overworld: pastikan EnemyIdentifier ter-assign.\n" +
                    "Jika testing langsung dari Battle scene: isi Fallback Enemy Prefab.");
                return;
            }

            Vector3 pos = spawnPoint != null
                ? spawnPoint.position
                : transform.position;

            GameObject enemy = Instantiate(prefabToSpawn, pos, Quaternion.identity);
            Debug.Log($"[BattleEnemySpawner] Spawn: {prefabToSpawn.name} di {pos}");

            OnEnemySpawned?.Invoke(enemy.transform);
        }

        private GameObject GetPrefabToSpawn()
        {
            // Prioritas 1: datang dari overworld via BattleManager
            if (BattleManager.Instance != null)
            {
                if (BattleManager.Instance.enemyBattlePrefab != null)
                {
                    Debug.Log("[BattleEnemySpawner] Menggunakan prefab dari BattleManager.");
                    return BattleManager.Instance.enemyBattlePrefab;
                }
                else
                {
                    Debug.LogWarning(
                        "[BattleEnemySpawner] BattleManager ada tapi enemyBattlePrefab null.\n" +
                        "Kemungkinan EnemyIdentifier di enemy overworld belum di-assign.");
                }
            }

            // Prioritas 2: fallback untuk testing langsung
            if (fallbackEnemyPrefab != null)
            {
                Debug.Log("[BattleEnemySpawner] Menggunakan fallback prefab.");
                return fallbackEnemyPrefab;
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnPoint == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            UnityEditor.Handles.Label(
                spawnPoint.position + Vector3.up * 0.4f,
                "Enemy Spawn");
        }
#endif
    }
}