using System.Collections.Generic;
using UnityEngine;

namespace WannaBHero.Battle
{
    /// <summary>
    /// SCRIPT TESTING SAJA
    public class TurnOrderTester : MonoBehaviour
    {
        [SerializeField] private CharacterStatsData playerStats;
        [SerializeField] private CharacterStatsData bossStats;
        [SerializeField] private int turnsToSimulate = 10;

        private void Start()
        {
            var player = new BattleCharacter(playerStats);
            var boss = new BattleCharacter(bossStats);

            var manager = new TurnOrderManager(new List<BattleCharacter> { player, boss });

            string sequence = "";
            for (int i = 0; i < turnsToSimulate; i++)
            {
                BattleCharacter next = manager.GetNextTurn();
                sequence += next.CharacterName + " -> ";
            }

            Debug.Log($"[TurnOrderTester] Urutan {turnsToSimulate} giliran: {sequence}");
            Debug.Log($"[TurnOrderTester] Player Speed={player.Speed}, Boss Speed={boss.Speed}, " +
                      $"rasio seharusnya {player.Speed / (float)boss.Speed:F1}:1");
        }
    }
}