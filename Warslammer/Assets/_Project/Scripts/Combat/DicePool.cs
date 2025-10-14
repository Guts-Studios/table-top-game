using System;
using UnityEngine;

namespace Warslammer.Combat
{
    /// <summary>
    /// Represents a pool of dice to roll
    /// Includes modifiers and special rules
    /// </summary>
    [Serializable]
    public class DicePool
    {
        #region Properties
        [Tooltip("Number of dice in the pool")]
        public int diceCount;
        
        [Tooltip("Number of sides on each die (typically 6)")]
        public int diceSides;
        
        [Tooltip("Target number to count as a success (e.g., 4+ means 4, 5, or 6)")]
        public int targetNumber;
        
        [Tooltip("Modifier to add to each die result")]
        public int diceModifier;
        
        [Tooltip("Can this pool generate critical hits?")]
        public bool allowCriticals;
        
        [Tooltip("Can re-roll 1s?")]
        public bool rerollOnes;
        
        [Tooltip("Can re-roll all failed dice?")]
        public bool rerollFailed;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a dice pool with default settings
        /// </summary>
        public DicePool()
        {
            diceCount = 1;
            diceSides = 6;
            targetNumber = 4;
            diceModifier = 0;
            allowCriticals = true;
            rerollOnes = false;
            rerollFailed = false;
        }

        /// <summary>
        /// Create a dice pool with specified parameters
        /// </summary>
        /// <param name="count">Number of dice</param>
        /// <param name="target">Target number for success</param>
        /// <param name="modifier">Modifier to add to results</param>
        public DicePool(int count, int target = 4, int modifier = 0)
        {
            diceCount = Mathf.Max(0, count);
            diceSides = 6;
            targetNumber = Mathf.Clamp(target, 1, 6);
            diceModifier = modifier;
            allowCriticals = true;
            rerollOnes = false;
            rerollFailed = false;
        }
        #endregion

        #region Modifiers
        /// <summary>
        /// Add dice to the pool
        /// </summary>
        /// <param name="count">Number of dice to add (can be negative)</param>
        public void AddDice(int count)
        {
            diceCount = Mathf.Max(0, diceCount + count);
        }

        /// <summary>
        /// Add a modifier to dice results
        /// </summary>
        /// <param name="modifier">Modifier to add</param>
        public void AddModifier(int modifier)
        {
            diceModifier += modifier;
        }

        /// <summary>
        /// Set target number for successes
        /// </summary>
        /// <param name="target">Target number (1-6)</param>
        public void SetTargetNumber(int target)
        {
            targetNumber = Mathf.Clamp(target, 1, diceSides);
        }

        /// <summary>
        /// Enable or disable critical hits
        /// </summary>
        /// <param name="enabled">Allow criticals?</param>
        public void SetCriticals(bool enabled)
        {
            allowCriticals = enabled;
        }

        /// <summary>
        /// Enable re-rolling 1s
        /// </summary>
        /// <param name="enabled">Reroll 1s?</param>
        public void SetRerollOnes(bool enabled)
        {
            rerollOnes = enabled;
        }

        /// <summary>
        /// Enable re-rolling failed dice
        /// </summary>
        /// <param name="enabled">Reroll failed?</param>
        public void SetRerollFailed(bool enabled)
        {
            rerollFailed = enabled;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Clone this dice pool
        /// </summary>
        public DicePool Clone()
        {
            DicePool clone = new DicePool
            {
                diceCount = this.diceCount,
                diceSides = this.diceSides,
                targetNumber = this.targetNumber,
                diceModifier = this.diceModifier,
                allowCriticals = this.allowCriticals,
                rerollOnes = this.rerollOnes,
                rerollFailed = this.rerollFailed
            };
            return clone;
        }

        /// <summary>
        /// Get a string representation of this dice pool
        /// </summary>
        public override string ToString()
        {
            string result = $"{diceCount}d{diceSides}";
            if (diceModifier != 0)
            {
                result += $"{(diceModifier > 0 ? "+" : "")}{diceModifier}";
            }
            result += $" ({targetNumber}+)";
            return result;
        }
        #endregion
    }
}