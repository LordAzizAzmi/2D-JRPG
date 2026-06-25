using System.Collections.Generic;
using System.Linq;

namespace WannaBHero.Battle
{
    /// <LogicTurnBased> Honkai Star Rail Wanna Be Ahh TurnBase
    /// Turn Battle by Action Gauge 
    /// 
    /// Contoh: Speed Player=100, Speed Boss=50 -> turn order otomatis:
    /// Player, Player, Boss, Player, Player, Boss, ... (rasio 2:1, sesuai rasio speed).

    public class TurnOrderManager
    {
        private const float ActionThreshold = 100f;

        private readonly List<BattleCharacter> combatants;

        public TurnOrderManager(List<BattleCharacter> combatants)
        {
            this.combatants = combatants;
        }

        /// check char (alive) for next turn.
        /// Mengisi Action Gauge semua karakter hidup sampai ada yang mencapai threshold
        public BattleCharacter GetNextTurn()
        {
            List<BattleCharacter> alive = combatants.Where(c => !c.IsDead).ToList();
            if (alive.Count == 0) return null;

            while (true)
            {
                // Cek dulu siapa yang SUDAH siap sebelum nambah gauge lagi --
                // ini penting supaya karakter yang "kalah tie-break" di panggilan
                // sebelumnya tidak kehilangan kelebihan gauge-nya / tidak dapat
                // tambahan gauge yang tidak semestinya.
                List<BattleCharacter> ready = alive
                    .Where(c => c.ActionGauge >= ActionThreshold)
                    .OrderByDescending(c => c.Speed)
                    .ToList();

                if (ready.Count > 0)
                {
                    BattleCharacter next = ready[0];
                    next.ActionGauge -= ActionThreshold; // sisa gauge dibawa ke ronde berikutnya
                    return next;
                }

                foreach (BattleCharacter c in alive)
                {
                    c.ActionGauge += c.Speed;
                }
            }
        }

        /// Helper untuk preview urutan giliran beberapa langkah ke depan tanpa
        /// benar-benar memproses aksi apapun -- berguna untuk UI "turn order preview"  NOTE : (FOR NEXT UPDATE)
        public List<BattleCharacter> PreviewNextTurns(int count)
        {
            // Simpan gauge asli, simulasikan di atas situ, lalu kembalikan.
            Dictionary<BattleCharacter, float> savedGauges = combatants.ToDictionary(c => c, c => c.ActionGauge);

            var result = new List<BattleCharacter>();
            for (int i = 0; i < count; i++)
            {
                BattleCharacter next = GetNextTurn();
                if (next == null) break;
                result.Add(next);
            }

            foreach (var kvp in savedGauges)
            {
                kvp.Key.ActionGauge = kvp.Value;
            }

            return result;
        }
    }
}