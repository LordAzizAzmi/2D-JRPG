using UnityEngine;
using UnityEngine.SceneManagement;
using WannaBHero.Battle;

namespace WannaBHero
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [HideInInspector] public GameObject overworldEnemy;   // Hide setelah battle
        [HideInInspector] public GameObject enemyBattlePrefab; // Spawn in battle
        [HideInInspector] public CharacterStatsData enemyStats; // Stat musuh yang diserang

        private string previousSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        public void StartBattle(GameObject overworldEnemyGO, string returnScene)
        {
            overworldEnemy = overworldEnemyGO;
            previousSceneName = returnScene;

            // Cari EnemyIdentifier di GO itu sendiri ATAU parent-nya
            // karena hit.gameObject bisa jadi child collider
            EnemyIdentifier id = overworldEnemyGO.GetComponent<EnemyIdentifier>()
                              ?? overworldEnemyGO.GetComponentInParent<EnemyIdentifier>();

            if (id != null && id.battlePrefab != null)
            {
                enemyBattlePrefab = id.battlePrefab;
                Debug.Log($"[BattleManager] Enemy battle prefab: {enemyBattlePrefab.name}");
            }
            else
            {
                Debug.LogError(
                    $"[BattleManager] '{overworldEnemyGO.name}' tidak punya EnemyIdentifier " +
                    $"atau battlePrefab kosong!\n" +
                    $"Cek: apakah EnemyIdentifier ada di ROOT prefab, bukan di child.");
            }

            overworldEnemyGO.SetActive(false);
            SceneManager.LoadScene("Battle");
        }

        public void EndBattle(bool enemyDefeated)
        {
            if (enemyDefeated && overworldEnemy != null)
            {
                // Win → destroy enemy from main map
                Destroy(overworldEnemy);
            }
            else if (overworldEnemy != null)
            {
                // Lose/Draw → enemy stay on main map
                overworldEnemy.SetActive(true);
            }

            enemyBattlePrefab = null;
            enemyStats = null;
            SceneManager.LoadScene(previousSceneName);
        }
    }
}