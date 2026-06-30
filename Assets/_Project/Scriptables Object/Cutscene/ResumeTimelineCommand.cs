using UnityEngine;
using Fungus;
using WannaBHero.Cutscene;

namespace WannaBHero.Fungus
{
    // Custom Fungus Command — muncul di menu "Add Command" Fungus dengan
    // nama "Resume Timeline" di kategori "Cutscene".
    //
    // Cara pakai:
    // Di Block Fungus yang dipanggil Timeline, taruh command ini paling
    // BAWAH (command terakhir, setelah semua Say/Menu/dialog selesai).
    // Begitu Block selesai dieksekusi sampai command ini, Timeline lanjut.
    [CommandInfo("Cutscene",
                 "Resume Timeline",
                 "Resume PlayableDirector yang sebelumnya di-pause untuk dialog ini. " +
                 "Taruh sebagai command TERAKHIR di Block.")]
    public class ResumeTimelineCommand : Command
    {
        public override void OnEnter()
        {
            if (CutsceneDialogController.Instance != null)
            {
                CutsceneDialogController.Instance.ResumeTimeline();
            }
            else
            {
                Debug.LogWarning("[ResumeTimelineCommand] CutsceneDialogController.Instance null — " +
                                 "pastikan scene cutscene punya GameObject dengan CutsceneDialogController.");
            }

            Continue(); // lanjut ke command berikutnya (biasanya tidak ada lagi setelah ini)
        }
    }
}