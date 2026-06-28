using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using WannaBHero.Battle;

namespace WannaBHero
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [HideInInspector] public GameObject overworldEnemy;
        [HideInInspector] public GameObject enemyBattlePrefab;
        [HideInInspector] public CharacterStatsData enemyStats;

        private string previousSceneName;
        private Vector3 savedPlayerPosition;
        private bool enemyWasDefeated;

        // ID milik enemy yang SEDANG di-battle saat ini (diisi di StartBattle,
        // dibaca lagi di EndBattle/HandleReturnRoutine).
        private string currentEnemyId;
        private static readonly HashSet<string> defeatedEnemyIds = new HashSet<string>();

        public static bool IsDefeated(string enemyId)
        {
            return !string.IsNullOrEmpty(enemyId) && defeatedEnemyIds.Contains(enemyId);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  START BATTLE
        // ─────────────────────────────────────────────────────────────────────────

        public void StartBattle(GameObject overworldEnemyGO, string returnScene)
        {
            overworldEnemy = overworldEnemyGO;
            previousSceneName = returnScene;

            // Ambil EnemyIdentifier untuk dapat battlePrefab DAN EnemyId permanen
            EnemyIdentifier id = overworldEnemyGO.GetComponent<EnemyIdentifier>()
                              ?? overworldEnemyGO.GetComponentInParent<EnemyIdentifier>();

            if (id != null)
            {
                currentEnemyId = id.EnemyId;
            }
            else
            {
                currentEnemyId = null;
                Debug.LogError(
                    $"[BattleManager] EnemyIdentifier tidak ditemukan di '{overworldEnemyGO.name}'! " +
                    "Enemy ini tidak akan bisa 'diingat' sudah dikalahkan.");
            }

            // Simpan posisi player untuk di-restore setelah battle
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                savedPlayerPosition = player.transform.position;

            if (id != null && id.battlePrefab != null)
            {
                enemyBattlePrefab = id.battlePrefab;
            }
            else
            {
                Debug.LogError(
                    $"[BattleManager] EnemyIdentifier atau battlePrefab " +
                    $"tidak ditemukan di '{overworldEnemyGO.name}'!\n" +
                    "Pastikan EnemyIdentifier ter-assign di root prefab enemy overworld.");
            }

            overworldEnemyGO.transform.SetParent(null);
            DontDestroyOnLoad(overworldEnemyGO);
            overworldEnemyGO.SetActive(false);

            SceneManager.LoadScene("Battle");
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  END BATTLE — Call BattleTurnManager setelah menang/kalah
        // ─────────────────────────────────────────────────────────────────────────

        public void EndBattle(bool playerWon)
        {
            enemyWasDefeated = playerWon;

            if (playerWon && !string.IsNullOrEmpty(currentEnemyId))
            {
                defeatedEnemyIds.Add(currentEnemyId);
                Debug.Log($"[BattleManager] Enemy '{currentEnemyId}' dicatat sebagai DEFEATED. " +
                          $"Total musuh dikalahkan sejauh ini: {defeatedEnemyIds.Count}");
            }

            enemyBattlePrefab = null;
            enemyStats = null;

            SceneManager.sceneLoaded += OnOverworldSceneLoaded;
            SceneManager.LoadScene(previousSceneName);
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  CALLBACK: overworld scene selesai load
        // ─────────────────────────────────────────────────────────────────────────

        private void OnOverworldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnOverworldSceneLoaded;
            StartCoroutine(HandleReturnRoutine());
        }

        private IEnumerator HandleReturnRoutine()
        {
            // Tunggu 1 frame agar semua Awake/Start di scene baru sudah selesai
            yield return null;

            // Restore posisi player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                player.transform.position = savedPlayerPosition;

            RemoveAllDefeatedEnemiesInScene();

            if (!enemyWasDefeated)
            {
                // Player kalah — kembalikan enemy yang TADI di-battle ke overworld
                // supaya encounter bisa terjadi ulang (dia belum dianggap defeated).
                if (overworldEnemy != null)
                {
                    SceneManager.MoveGameObjectToScene(overworldEnemy, SceneManager.GetActiveScene());
                    overworldEnemy.SetActive(true);
                    Debug.Log("[BattleManager] Player kalah — enemy dikembalikan ke overworld.");
                }
            }
            else
            {
                // Player menang — enemy dihapus dari overworld (karena sudah defeated).
                if (overworldEnemy != null)
                {
                    Destroy(overworldEnemy);
                }
            }

            // Bersihkan referensi
            overworldEnemy = null;
            currentEnemyId = null;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  Hapus SEMUA enemy overworld di scene aktif yang ID-nya sudah defeated.
        // ─────────────────────────────────────────────────────────────────────────
        public static void RemoveAllDefeatedEnemiesInScene()
        {
            if (defeatedEnemyIds.Count == 0) return;

            // FindObjectsByType lebih cepat & tidak deprecated dibanding FindObjectsOfType.
            EnemyIdentifier[] allEnemies = Object.FindObjectsByType<EnemyIdentifier>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            int removed = 0;
            foreach (EnemyIdentifier enemy in allEnemies)
            {
                if (IsDefeated(enemy.EnemyId))
                {
                    Object.Destroy(enemy.gameObject);
                    removed++;
                }
            }

            if (removed > 0)
                Debug.Log($"[BattleManager] Membersihkan {removed} enemy yang sudah pernah dikalahkan dari scene.");
        }
    }
}