using UnityEngine;

namespace WannaBHero.Battle
{
    /// Data Dasar (template) stats for Character (Player, Enemy Mob, Boss)
    /// ONLY FOR BASIC STATS HERE.
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "WannaBHero/Character Stats")]
    public class CharacterStatsData : ScriptableObject
    {
        [Header("Identitas")]
        public string characterName;

        [Header("Stat Dasar")]
        public int maxHP = 100;
        public int attack = 10;
        public int speed = 50;
        public int defense = 5;
    }
}