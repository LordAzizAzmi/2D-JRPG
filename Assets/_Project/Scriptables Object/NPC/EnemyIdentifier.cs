using UnityEngine;
using WannaBHero.Battle;

namespace WannaBHero
{
    public class EnemyIdentifier : MonoBehaviour
    {
        [Tooltip("Drag form Prefabs Battle)\n")]
        public GameObject battlePrefab;
        internal CharacterStatsData stats;

        [Header("Unique ID")]
        [Tooltip("Dipakai BattleManager ")]
        [SerializeField] private string enemyId;

        public string EnemyId
        {
            get
            {
                if (string.IsNullOrEmpty(enemyId))
                    enemyId = System.Guid.NewGuid().ToString();
                return enemyId;
            }
        }

        private void Awake()
        {
            // ── ID Enemies ──
            if (BattleManager.IsDefeated(EnemyId))
            {
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ── Validasi battlePrefab harus prefab asset, bukan scene object ──
            if (battlePrefab != null)
            {
                bool isPrefabAsset = UnityEditor.PrefabUtility
                    .GetPrefabAssetType(battlePrefab)
                    != UnityEditor.PrefabAssetType.NotAPrefab;

                if (!isPrefabAsset)
                {
                    Debug.LogError(
                        $"[EnemyIdentifier] '{name}': Battle Prefab harus berupa Prefab Asset, " +
                        "bukan object dari scene. Reset ke null.", this);

                    // Reset supaya tidak tersimpan sebagai scene object
                    battlePrefab = null;
                }
                else
                {
                    Debug.Log($"[EnemyIdentifier] '{name}': Battle Prefab OK → " +
                              battlePrefab.name);
                }
            }

            // ── Auto-generate enemyId SEKALI saja ──
            if (string.IsNullOrEmpty(enemyId))
            {
                enemyId = System.Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[EnemyIdentifier] '{name}': EnemyId baru di-generate → {enemyId}\n" +
                          "PENTING: Save scene supaya ID ini tersimpan permanen!");
            }
        }
#endif
    }
}