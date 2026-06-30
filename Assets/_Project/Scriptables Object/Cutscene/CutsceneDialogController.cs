using UnityEngine;
using UnityEngine.Playables;
using Fungus;

namespace WannaBHero.Cutscene
{
    // Pasang script ini di GameObject yang sama dengan PlayableDirector (Timeline).
    //
    // Cara kerja:
    // 1. Signal Emitter di Timeline trigger PauseForDialog(flowchart, blockName)
    //    lewat Signal Receiver (di-assign manual di Inspector Signal Receiver).
    // 2. Method ini pause Timeline + jalankan Fungus Block tertentu.
    // 3. Block terakhir di Fungus harus diakhiri dengan command "Call Method" /
    //    custom command ResumeTimelineCommand (lihat ResumeTimelineCommand.cs)
    //    yang manggil ResumeTimeline() di sini.
    //
    // Kenapa pakai parameter (flowchart, blockName) bukan satu Flowchart fix:
    // Supaya satu Timeline bisa pause untuk dialog BERBEDA-BEDA di titik
    // berbeda — tiap Signal Emitter bisa assign Block Fungus yang berbeda.
    [RequireComponent(typeof(PlayableDirector))]
    public class CutsceneDialogController : MonoBehaviour
    {
        public static CutsceneDialogController Instance { get; private set; }

        private PlayableDirector director;

        private void Awake()
        {
            director = GetComponent<PlayableDirector>();
            Instance = this; // single cutscene aktif dalam satu waktu — cukup satu instance
        }

        // ── Dipanggil dari Signal Receiver (drag method ini di Inspector) ──────────
        /// Pause Timeline lalu jalankan Fungus Block tertentu.
        /// Assign method ini di Signal Receiver → Signal Emitted → drag Flowchart
        /// dan isi nama Block sebagai parameter.
        public void PauseForDialog(Flowchart flowchart, string blockName)
        {
            if (flowchart == null)
            {
                Debug.LogError("[CutsceneDialogController] Flowchart belum di-assign di Signal Emitter.", this);
                return;
            }

            director.Pause();
            Debug.Log($"[CutsceneDialogController] Timeline paused. Menjalankan Fungus block '{blockName}'.");

            Block block = flowchart.FindBlock(blockName);
            if (block == null)
            {
                Debug.LogError($"[CutsceneDialogController] Block '{blockName}' tidak ditemukan di Flowchart '{flowchart.name}'.", this);
                director.Resume(); // jangan sampai stuck pause selamanya kalau block salah nama
                return;
            }

            flowchart.ExecuteBlock(block);
        }

        // ── Dipanggil dari command terakhir di Fungus Block (lihat ResumeTimelineCommand.cs) ──

        public void ResumeTimeline()
        {
            director.Resume();
            Debug.Log("[CutsceneDialogController] Dialog selesai — Timeline resume.");
        }
    }
}