using System;
using UnityEngine;

namespace WannaBHero.Battle
{
    /// HOW WORKS 
    /// Pasang di Battle Scene. Membaca BattleManager.Instance (yang sudah
    /// diisi saat StartBattle() dipanggil dari Main Scene) untuk tahu enemy mana
    /// yang harus di-spawn, lalu broadcast OnEnemySpawned ke script lain
    /// (BattlePlayerController) supaya otomatis tahu targetnya.
    public class BattleEnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform enemySpawnPoint;

        public static event Action<Transform> OnEnemySpawned;

        private void Start()
        {
            SpawnEncounteredEnemy();
        }

        private void SpawnEncounteredEnemy()
        {
            BattleManager battleManager = BattleManager.Instance;

            if (battleManager == null || battleManager.enemyBattlePrefab == null)
            {
                Debug.LogError("[BattleEnemySpawner] BattleManager.Instance atau enemyBattlePrefab " +
                                "kosong -- kemungkinan scene Battle dibuka langsung tanpa lewat " +
                                "BattleTransition di Main Scene.", this);
                return;
            }

            Vector3 spawnPosition = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            GameObject enemyInstance = Instantiate(battleManager.enemyBattlePrefab, spawnPosition, Quaternion.identity);

            Debug.Log($"[BattleEnemySpawner] Musuh ter-spawn: {enemyInstance.name} " +
                      $"(stats: {battleManager.enemyStats?.characterName})");

            OnEnemySpawned?.Invoke(enemyInstance.transform);
        }
    }
}