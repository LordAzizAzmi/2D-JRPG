using UnityEngine;
using WannaBHero.Battle;

namespace WannaBHero
{
    /// Pasang di SETIAP GameObject enemy attackable (Mob/Boss) di Main Scene.
    public class EnemyIdentifier : MonoBehaviour
    {
        [Tooltip("Prefab yang akan di-spawn di Battle Scene saat enemy ini diserang.")]
        public GameObject battlePrefab;

        [Tooltip("Data stat (HP/Attack/Speed/Defense) milik enemy ini -- " +
                 "drag MobStats.asset / BossStats.asset / dst.")]
        public CharacterStatsData stats;
    }
}