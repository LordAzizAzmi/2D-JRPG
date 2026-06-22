using UnityEngine;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSort : MonoBehaviour
    {
        [SerializeField] private int pixelsPerUnit = 32;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }


        private void LateUpdate()
        {
            // Offset 1000 beri ruang aman di atas sortingOrder tilemap
            sr.sortingOrder = 1000 - (int)(transform.position.y * pixelsPerUnit);
        }
    }
}