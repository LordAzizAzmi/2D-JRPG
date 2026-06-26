using UnityEngine;
using UnityEngine.SceneManagement;

namespace WannaBHero
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [HideInInspector] public GameObject overworldEnemy;   // Hide setelah battle
        [HideInInspector] public GameObject enemyBattlePrefab; // Spawn in battle

        private string previousSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartBattle(GameObject overworldEnemyGO, string returnScene)
        {
            overworldEnemy = overworldEnemyGO;
            previousSceneName = returnScene;

            // take From EnemyIdentifier
            EnemyIdentifier id = overworldEnemyGO.GetComponent<EnemyIdentifier>();
            if (id != null && id.battlePrefab != null)
            {
                enemyBattlePrefab = id.battlePrefab;
            }
            else
            {
                Debug.LogError($"[BattleManager] Enemy '{overworldEnemyGO.name}' " +
                               "tidak punya EnemyIdentifier atau battlePrefab kosong!");
            }

            // Hide Enemy when battling
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
            SceneManager.LoadScene(previousSceneName);
        }
    }
}