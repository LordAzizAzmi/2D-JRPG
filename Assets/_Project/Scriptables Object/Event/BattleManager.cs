using UnityEngine;
using UnityEngine.SceneManagement;

namespace WannaBHero
{
    // DontDestroyOnLoad — bertahan antar scene
    // Simpan data enemy yang akan diperangi & scene asal untuk kembali
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [HideInInspector] public GameObject enemyToBattle;
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

        public void StartBattle(GameObject enemy, string returnScene)
        {
            enemyToBattle = enemy;
            previousSceneName = returnScene;
            SceneManager.LoadScene("Battle");
        }

        public void EndBattle()
        {
            if (enemyToBattle != null)
                Destroy(enemyToBattle);   // hapus enemy yg sudah kalah

            SceneManager.LoadScene(previousSceneName);
        }
    }
}