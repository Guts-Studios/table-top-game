using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Combat
{
    /// <summary>
    /// ScriptableObject for weapon statistics and special rules
    /// Defines attack dice, damage, range, and special abilities
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Warslammer/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        #region Basic Properties
        [Header("Basic Info")]
        [Tooltip("Display name of the weapon")]
        public string weaponName = "Weapon";

        [Tooltip("Description of the weapon")]
        [TextArea(3, 5)]
        public string description = "";

        [Tooltip("Icon sprite for UI")]
        public Sprite icon;
        #endregion

        #region Range
        [Header("Range")]
        [Tooltip("Weapon range type")]
        public RangeType rangeType = RangeType.Melee;

        [Tooltip("Minimum range in inches (0 for no minimum)")]
        public float minRangeInches = 0f;

        [Tooltip("Maximum range in inches")]
        public float maxRangeInches = 1f;
        #endregion

        #region Attack
        [Header("Attack Stats")]
        [Tooltip("Number of attack dice to roll")]
        public int attackDice = 2;

        [Tooltip("Target number for hit (4+ means 4, 5, or 6 hit)")]
        [Range(2, 6)]
        public int toHitTarget = 4;

        [Tooltip("Base damage on successful hit")]
        public int baseDamage = 2;

        [Tooltip("Armor penetration value")]
        public int armorPenetration = 0;
        #endregion

        #region Special Rules
        [Header("Special Rules")]
        [Tooltip("Damage source type")]
        public DamageSource damageSource = DamageSource.Physical;

        [Tooltip("Can this weapon make critical hits?")]
        public bool allowsCriticals = true;

        [Tooltip("Attacks ignore cover")]
        public bool ignoresCover = false;

        [Tooltip("Attacks cannot be blocked")]
        public bool unblockable = false;

        [Tooltip("Weapon has Blast (AoE) effect")]
        public bool hasBlast = false;

        [Tooltip("Blast radius in inches")]
        public float blastRadius = 0f;

        [Tooltip("Bonus damage against specific unit type")]
        public bool hasBonusVsType = false;

        [Tooltip("Unit type to get bonus against")]
        public UnitType bonusVsUnitType = UnitType.Infantry;

        [Tooltip("Bonus damage amount")]
        public int bonusDamageAmount = 1;
        #endregion

        #region Status Effects
        [Header("Status Effects")]
        [Tooltip("Can inflict Bleed status")]
        public bool canInflictBleed = false;

        [Tooltip("Chance to inflict Bleed (0-1)")]
        [Range(0f, 1f)]
        public float bleedChance = 0.2f;

        [Tooltip("Can inflict Rooted status")]
        public bool canInflictRooted = false;

        [Tooltip("Chance to inflict Rooted (0-1)")]
        [Range(0f, 1f)]
        public float rootedChance = 0.2f;
        #endregion

        #region Attack Modifiers
        [Header("Attack Modifiers")]
        [Tooltip("Reroll attack dice that roll 1")]
        public bool rerollOnes = false;

        [Tooltip("Reroll all failed attack dice")]
        public bool rerollFailed = false;

        [Tooltip("Add this modifier to each attack die")]
        public int attackModifier = 0;
        #endregion

        #region Requirements
        [Header("Requirements")]
        [Tooltip("Requires two hands to use")]
        public bool requiresTwoHands = false;

        [Tooltip("Can only be used once per turn")]
        public bool limitedUse = false;

        [Tooltip("Number of uses per battle")]
        public int usesPerBattle = 0;
        #endregion

        #region Utility Methods
        /// <summary>
        /// Check if target is in range
        /// </summary>
        /// <param name="distance">Distance to target in inches</param>
        /// <returns>True if in range</returns>
        public bool IsInRange(float distance)
        {
            return distance >= minRangeInches && distance <= maxRangeInches;
        }

        /// <summary>
        /// Check if this weapon can target a specific unit type
        /// </summary>
        /// <param name="unitType">Unit type to check</param>
        /// <returns>True if can target</returns>
        public bool CanTargetUnitType(UnitType unitType)
        {
            // By default, all weapons can target all types
            // Override this for specific weapon restrictions
            return true;
        }

        /// <summary>
        /// Get damage bonus against a specific unit type
        /// </summary>
        /// <param name="unitType">Unit type</param>
        /// <returns>Bonus damage</returns>
        public int GetBonusDamageVsType(UnitType unitType)
        {
            if (hasBonusVsType && unitType == bonusVsUnitType)
                return bonusDamageAmount;
            return 0;
        }

        /// <summary>
        /// Create an attack dice pool for this weapon
        /// </summary>
        /// <returns>Dice pool configured for this weapon</returns>
        public DicePool CreateAttackPool()
        {
            DicePool pool = new DicePool(attackDice, toHitTarget, attackModifier);
            pool.SetCriticals(allowsCriticals);
            pool.SetRerollOnes(rerollOnes);
            pool.SetRerollFailed(rerollFailed);
            return pool;
        }

        /// <summary>
        /// Get weapon summary string
        /// </summary>
        public string GetSummary()
        {
            string summary = $"{weaponName} - ";
            
            if (rangeType == RangeType.Melee)
            {
                summary += $"Melee {maxRangeInches}\"";
            }
            else
            {
                summary += $"Ranged {maxRangeInches}\"";
            }

            summary += $", {attackDice}d6 ({toHitTarget}+), {baseDamage} dmg";

            if (armorPenetration > 0)
            {
                summary += $", AP {armorPenetration}";
            }

            return summary;
        }

        /// <summary>
        /// Get detailed stats string
        /// </summary>
        public string GetDetailedStats()
        {
            string stats = $"{weaponName}\n";
            stats += $"Range: {(rangeType == RangeType.Melee ? "Melee" : "Ranged")} {maxRangeInches}\"\n";
            stats += $"Attack: {attackDice}d6 ({toHitTarget}+)\n";
            stats += $"Damage: {baseDamage}\n";
            
            if (armorPenetration > 0)
                stats += $"AP: {armorPenetration}\n";

            if (hasBonusVsType)
                stats += $"+{bonusDamageAmount} vs {bonusVsUnitType}\n";

            if (!string.IsNullOrEmpty(description))
                stats += $"\n{description}\n";

            return stats;
        }
        #endregion
    }
}