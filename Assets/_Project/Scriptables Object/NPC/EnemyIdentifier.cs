using UnityEngine;

namespace WannaBHero
{
    public class EnemyIdentifier : MonoBehaviour
    {
        [Tooltip("Drag form Prefabs Battle)\n")]
        public GameObject battlePrefab;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Deteksi jika battlePrefab adalah scene object, bukan prefab asset
            if (battlePrefab != null)
            {
                bool isPrefabAsset = UnityEditor.PrefabUtility
                    .GetPrefabAssetType(battlePrefab)
                    != UnityEditor.PrefabAssetType.NotAPrefab;

                if (!isPrefabAsset)
                {
                    Debug.LogError(
                        $"[EnemyIdentifier] '{name}': Battle Prefab adalah ",
                        this);

                    // Reset supaya tidak tersimpan sebagai scene object
                    battlePrefab = null;
                }
                else
                {
                    Debug.Log($"[EnemyIdentifier] '{name}': Battle Prefab OK → " +
                              battlePrefab.name);
                }
            }
        }
#endif
    }
}