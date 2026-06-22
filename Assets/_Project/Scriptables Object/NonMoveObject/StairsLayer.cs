using UnityEngine;

namespace WannaBHero.Player
{
    // OnTriggerEnter: kalau player masuk dari sisi BAWAH tangga → langsung set Upper
    //                 (supaya sorting visual berubah begitu player injak anak tangga pertama)
    //
    // OnTriggerExit:  posisi player SAAT EXIT menentukan layer final:
    //                 - keluar dari sisi BAWAH → player balik turun → set Lower
    //                 - keluar dari sisi ATAS  → player berhasil naik → set Upper
    [RequireComponent(typeof(Collider2D))]
    public class StairsLayer : MonoBehaviour
    {
        [Header("Arah tangga ini menghadap")]
        public Direction direction;

        [Header("Saat di ATAS tangga")]
        public string physicsLayerUpper;
        public string sortingLayerUpper;

        [Header("Saat di BAWAH tangga")]
        public string physicsLayerLower;
        public string sortingLayerLower;

        private Collider2D col;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            if (!col.isTrigger)
            {
                Debug.LogWarning($"[StairsLayer] Collider2D di '{name}' bukan trigger. Dipaksa jadi trigger.", this);
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsValidTarget(other)) return;

            // Hanya set saat masuk dari bawah supaya sorting visual langsung tepat
            if (IsOnLowerSide(other))
            {
                Apply(other.gameObject, physicsLayerUpper, sortingLayerUpper);
            }
            // Masuk dari atas (mau turun): tidak set apa-apa dulu, tunggu exit.
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsValidTarget(other)) return;

            // Posisi player saat exit = sisi mana dia keluar = layer yang benar.
            // Ini menangani semua skenario:
            //   naik berhasil     → exit dari atas  → Upper ✓
            //   naik lalu balik   → exit dari bawah → Lower ✓  (perbaiki temporary Upper dari Enter)
            //   turun berhasil    → exit dari bawah → Lower ✓
            //   turun lalu balik  → exit dari atas  → Upper ✓
            if (IsOnLowerSide(other))
            {
                Apply(other.gameObject, physicsLayerLower, sortingLayerLower);
            }
            else
            {
                Apply(other.gameObject, physicsLayerUpper, sortingLayerUpper);
            }
        }

        // Pakai bounds.center collider (bukan transform.position pivot) sebagai titik
        private bool IsOnLowerSide(Collider2D other)
        {
            Vector2 center = col.bounds.center;
            return direction switch
            {
                Direction.South => other.transform.position.y < center.y,
                Direction.North => other.transform.position.y > center.y,
                Direction.West => other.transform.position.x < center.x,
                Direction.East => other.transform.position.x > center.x,
                _ => false
            };
        }

        private bool IsValidTarget(Collider2D other)
        {
            return other.CompareTag("Player");
        }

        private void Apply(GameObject target, string physicsLayer, string sortingLayer)
        {
            if (!string.IsNullOrEmpty(physicsLayer))
            {
                int layerIndex = LayerMask.NameToLayer(physicsLayer);
                if (layerIndex == -1)
                {
                    Debug.LogError($"[StairsLayer] '{name}': Physics Layer '{physicsLayer}' tidak ditemukan. " +
                                   "Cek penulisan nama di Project Settings > Tags and Layers.", this);
                }
                else
                {
                    target.layer = layerIndex;
                }
            }

            if (string.IsNullOrEmpty(sortingLayer))
            {
                Debug.LogError($"[StairsLayer] '{name}': Sorting Layer kosong, isi di Inspector.", this);
                return;
            }

            SpriteRenderer[] srs = target.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (SpriteRenderer sr in srs)
            {
                sr.sortingLayerName = sortingLayer;
            }
        }

        public enum Direction
        {
            North,
            South,
            West,
            East
        }
    }
}