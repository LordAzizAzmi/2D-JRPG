using UnityEngine;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SlashEffect : MonoBehaviour
    {
        // Dipanggil via Animation Event di frame TERAKHIR clip slash
        public void OnSlashEnd()
        {
            Destroy(gameObject);
        }
    }
}