using UnityEngine;

namespace WannaBHero.Player
{
    [RequireComponent(typeof(Animator))]
    public class SlashSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject slashPrefabAttack1;
        [SerializeField] private GameObject slashPrefabAttack2;

        [Header("Anchor per Arah — drag di Scene view")]
        [Tooltip("Geser & putar langsung di Scene view untuk adjust posisi & rotasi slash")]
        [SerializeField] private Transform anchorDown;
        [SerializeField] private Transform anchorUp;
        [SerializeField] private Transform anchorLeft;
        [SerializeField] private Transform anchorRight;

        [Header("Sorting")]
        [SerializeField] private string sortingLayerName = "Layer 1";
        [SerializeField] private int sortingOrderBoost = 10;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        // Dipanggil Animation Event Attack1
        public void SpawnSlash1() => SpawnSlash(slashPrefabAttack1);

        // Dipanggil Animation Event Attack2
        public void SpawnSlash2() =>
            SpawnSlash(slashPrefabAttack2 != null ? slashPrefabAttack2 : slashPrefabAttack1);

        private void SpawnSlash(GameObject prefab)
        {
            if (prefab == null) return;

            Transform anchor = GetAnchorForCurrentDirection();
            if (anchor == null) return;


            GameObject slash = Instantiate(
                prefab,
                anchor.position,   // posisi anchor = posisi slash
                anchor.rotation    // rotasi anchor = rotasi slash
            );

            // Lepas dari parent — tidak terpengaruh scale player
            slash.transform.SetParent(null);

            // Apply sorting
            SpriteRenderer sr = slash.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = GetPlayerSortingOrder() + sortingOrderBoost;
            }
        }

        private Transform GetAnchorForCurrentDirection()
        {
            float moveX = animator.GetFloat("moveX");
            float moveY = animator.GetFloat("moveY");

            // Tentukan arah dominan
            if (Mathf.Abs(moveX) >= Mathf.Abs(moveY))
            {
                return moveX >= 0f ? anchorRight : anchorLeft;
            }
            else
            {
                return moveY >= 0f ? anchorUp : anchorDown;
            }
        }

        private int GetPlayerSortingOrder()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            return sr != null ? sr.sortingOrder : 0;
        }

#if UNITY_EDITOR
        // Visualisasi anchor di Scene view
        private void OnDrawGizmosSelected()
        {
            DrawAnchorGizmo(anchorDown, Color.blue, "↓");
            DrawAnchorGizmo(anchorUp, Color.green, "↑");
            DrawAnchorGizmo(anchorLeft, Color.red, "←");
            DrawAnchorGizmo(anchorRight, Color.yellow, "→");
        }

        private void DrawAnchorGizmo(Transform anchor, Color color, string label)
        {
            if (anchor == null) return;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(anchor.position, 0.15f);
            // Garis arah hadap slash
            Gizmos.DrawLine(
                anchor.position,
                anchor.position + anchor.right * 0.4f
            );
        }
#endif
    }
}