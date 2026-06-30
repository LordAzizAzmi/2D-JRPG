using UnityEngine;
using Fungus;

namespace WannaBHero.Cutscene
{
    // Pasang 1 component ini PER TITIK dialog di Timeline.
    // Bisa di-attach di GameObject manapun (misal child kosong di bawah
    // GameObject Timeline, beri nama sesuai titiknya: "Cue_Dialog1", dst).
    //
    // Kenapa perlu wrapper ini:
    // UnityEvent (yang dipakai Signal Receiver) HANYA bisa bind method
    // tanpa parameter atau dengan 1 parameter primitif. Method
    // PauseForDialog(Flowchart, string) tidak akan pernah muncul di
    // dropdown Signal Receiver karena py 2 parameter custom.
    //
    // Solusi: tiap titik dialog simpan referensi Flowchart + nama Block
    // di sini, lalu Signal Receiver cukup panggil Trigger() (0 parameter)
    // yang otomatis muncul di dropdown.
    public class DialogCue : MonoBehaviour
    {
        [Tooltip("Flowchart yang berisi Block dialog untuk titik ini")]
        [SerializeField] private Flowchart flowchart;

        [Tooltip("Nama Block yang akan dijalankan saat Timeline pause di titik ini")]
        [SerializeField] private string blockName;

        // ── Method INI yang di-assign di Signal Receiver ──
        // 0 parameter → otomatis muncul di dropdown UnityEvent
        public void Trigger()
        {
            if (CutsceneDialogController.Instance == null)
            {
                Debug.LogError("[DialogCue] CutsceneDialogController.Instance null!", this);
                return;
            }

            CutsceneDialogController.Instance.PauseForDialog(flowchart, blockName);
        }
    }
}