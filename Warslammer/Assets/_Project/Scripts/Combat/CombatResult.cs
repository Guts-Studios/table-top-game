using Warslammer.Core;
using Warslammer.Units;

namespace Warslammer.Combat
{
    /// <summary>
    /// Stores the outcome of a combat encounter
    /// Includes attacker, defender, damage dealt, and special effects
    /// </summary>
    public class CombatResult
    {
        #region Properties
        /// <summary>
        /// Attacking unit
        /// </summary>
        public Unit Attacker { get; private set; }

        /// <summary>
        /// Defending unit
        /// </summary>
        public Unit Defender { get; private set; }

        /// <summary>
        /// Weapon used in the attack
        /// </summary>
        public WeaponData Weapon { get; private set; }

        /// <summary>
        /// Attack roll result
        /// </summary>
        public DiceRollResult AttackRoll { get; private set; }

        /// <summary>
        /// Defense roll result
        /// </summary>
        public DiceRollResult DefenseRoll { get; private set; }

        /// <summary>
        /// Final damage dealt
        /// </summary>
        public int DamageDealt { get; private set; }

        /// <summary>
        /// Was the attack a hit?
        /// </summary>
        public bool WasHit { get; private set; }

        /// <summary>
        /// Were there any critical hits?
        /// </summary>
        public bool HadCritical { get; private set; }

        /// <summary>
        /// Number of critical hits
        /// </summary>
        public int CriticalCount { get; private set; }

        /// <summary>
        /// Did the defender die?
        /// </summary>
        public bool WasKilled { get; private set; }

        /// <summary>
        /// Damage source type
        /// </summary>
        public DamageSource DamageSource { get; private set; }

        /// <summary>
        /// Distance of attack in inches
        /// </summary>
        public float AttackDistance { get; private set; }

        /// <summary>
        /// Was this a melee attack?
        /// </summary>
        public bool WasMelee { get; private set; }

        /// <summary>
        /// Additional combat modifiers applied
        /// </summary>
        public string ModifierSummary { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new combat result
        /// </summary>
        public CombatResult(Unit attacker, Unit defender, WeaponData weapon)
        {
            Attacker = attacker;
            Defender = defender;
            Weapon = weapon;
            DamageDealt = 0;
            WasHit = false;
            HadCritical = false;
            CriticalCount = 0;
            WasKilled = false;
            DamageSource = DamageSource.Physical;
            AttackDistance = 0f;
            WasMelee = false;
            ModifierSummary = "";
        }
        #endregion

        #region Setters
        /// <summary>
        /// Set the attack roll result
        /// </summary>
        public void SetAttackRoll(DiceRollResult result)
        {
            AttackRoll = result;
            if (result != null)
            {
                HadCritical = result.HasCritical;
                CriticalCount = result.Criticals;
            }
        }

        /// <summary>
        /// Set the defense roll result
        /// </summary>
        public void SetDefenseRoll(DiceRollResult result)
        {
            DefenseRoll = result;
        }

        /// <summary>
        /// Set the damage dealt
        /// </summary>
        public void SetDamage(int damage)
        {
            DamageDealt = damage;
            WasHit = damage > 0;
        }

        /// <summary>
        /// Set whether defender was killed
        /// </summary>
        public void SetKilled(bool killed)
        {
            WasKilled = killed;
        }

        /// <summary>
        /// Set damage source type
        /// </summary>
        public void SetDamageSource(DamageSource source)
        {
            DamageSource = source;
        }

        /// <summary>
        /// Set attack distance
        /// </summary>
        public void SetAttackDistance(float distance, bool melee)
        {
            AttackDistance = distance;
            WasMelee = melee;
        }

        /// <summary>
        /// Set modifier summary
        /// </summary>
        public void SetModifierSummary(string summary)
        {
            ModifierSummary = summary;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a formatted combat log string
        /// </summary>
        public string GetCombatLogString()
        {
            string log = $"{Attacker?.GetUnitName() ?? "Unknown"} attacks {Defender?.GetUnitName() ?? "Unknown"}";
            
            if (Weapon != null)
            {
                log += $" with {Weapon.weaponName}";
            }

            log += $" at {AttackDistance:F1}\\\"\\n";

            if (AttackRoll != null)
            {
                log += $"  Attack: {AttackRoll}\\n";
            }

            if (DefenseRoll != null)
            {
                log += $"  Defense: {DefenseRoll}\\n";
            }

            if (WasHit)
            {
                log += $"  → Hit for {DamageDealt} damage!";
                if (HadCritical)
                {
                    log += $" ({CriticalCount} CRITICAL!)";
                }
                log += "\\n";

                if (WasKilled)
                {
                    log += $"  → {Defender?.GetUnitName()} was killed!\\n";
                }
            }
            else
            {
                log += "  → Miss!\\n";
            }

            if (!string.IsNullOrEmpty(ModifierSummary))
            {
                log += $"  Modifiers: {ModifierSummary}\\n";
            }

            return log;
        }

        /// <summary>
        /// Get a short summary string
        /// </summary>
        public string GetSummary()
        {
            if (WasHit)
            {
                string summary = $"{DamageDealt} damage";
                if (HadCritical)
                    summary += " (CRIT!)";
                if (WasKilled)
                    summary += " [KILLED]";
                return summary;
            }
            else
            {
                return "Miss";
            }
        }

        /// <summary>
        /// Get detailed result string
        /// </summary>
        public override string ToString()
        {
            return GetCombatLogString();
        }
        #endregion
    }
}