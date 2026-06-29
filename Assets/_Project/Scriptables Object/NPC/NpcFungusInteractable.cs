using UnityEngine;
using Fungus;

namespace WannaBHero.Dialogue
{
    public class NpcFungusInteractable : MonoBehaviour
    {
        [Header("Fungus")]
        [SerializeField] private BlockReference block;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.Space;

        [Header("Tag Player")]
        [SerializeField] private string playerTag = "Player";

        [Header("UI Prompt (opsional)")]
        [SerializeField] private GameObject interactPrompt;

        private bool playerInRange;

        private void Awake()
        {
            if (block.block == null)
                Debug.LogError($"[NpcFungusInteractable] '{name}': Block belum dipilih! " +
                                "Pilih Flowchart & Block lewat dropdown di field 'block' pada Inspector.");

            SetPromptVisible(false);
        }

        private void Update()
        {
            if (!playerInRange) return;
            if (block.block == null) return;

            // Selagi Block sedang berjalan (dialog masih tampil di layar),
            // jangan terima input interact baru.
            if (block.block.IsExecuting()) return;

            if (Input.GetKeyDown(interactKey))
            {
                StartDialogue();
            }
        }

        private void StartDialogue()
        {
            block.Execute();
            Debug.Log($"[NpcFungusInteractable] '{name}': Menjalankan Block '{block.block.BlockName}'.");
        }

        // ─────────────────────────────────────
        //  TRIGGER — deteksi player masuk/keluar area NPC
        // ─────────────────────────────────────
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            playerInRange = true;
            SetPromptVisible(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;

            playerInRange = false;
            SetPromptVisible(false);
        }

        private void SetPromptVisible(bool visible)
        {
            if (interactPrompt != null)
                interactPrompt.SetActive(visible);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var col = GetComponent<Collider2D>();
            if (col == null) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + (Vector3)col.offset, col.bounds.size);
        }
#endif
    }
}