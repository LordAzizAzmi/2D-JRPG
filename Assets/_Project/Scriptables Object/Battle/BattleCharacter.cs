using UnityEngine;

namespace WannaBHero.Battle
{
    public class BattleCharacter
    {
        public readonly CharacterStatsData baseStats;

        public int CurrentHP { get; private set; }
        public float ActionGauge { get; set; }

        public string CharacterName => baseStats.characterName;
        public int MaxHP => baseStats.maxHP;
        public int Attack => baseStats.attack;
        public int Speed => baseStats.speed;
        public int Defense => baseStats.defense;

        public bool IsDead => CurrentHP <= 0;

        public BattleCharacter(CharacterStatsData data)
        {
            baseStats = data;
            CurrentHP = data.maxHP; // selalu mulai battle dengan HP penuh (dari template)
            ActionGauge = 0f;
        }

        /// Attack/Damage - Defense = Actual Damage
        /// minimal 1 
        public void TakeDamage(int rawDamage)
        {
            int actualDamage = Mathf.Max(1, rawDamage - Defense);
            CurrentHP = Mathf.Max(0, CurrentHP - actualDamage);
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }
    }
}