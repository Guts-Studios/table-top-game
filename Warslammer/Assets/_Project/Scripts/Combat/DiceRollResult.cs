using System.Collections.Generic;
using UnityEngine;

namespace Warslammer.Combat
{
    /// <summary>
    /// Result of a dice roll
    /// Contains individual die results, successes, and critical hits
    /// </summary>
    public class DiceRollResult
    {
        #region Properties
        /// <summary>
        /// Individual die results (before modifiers)
        /// </summary>
        public List<int> DiceRolls { get; private set; }

        /// <summary>
        /// Modified die results (after applying modifiers)
        /// </summary>
        public List<int> ModifiedRolls { get; private set; }

        /// <summary>
        /// Total number of successes
        /// </summary>
        public int Successes { get; private set; }

        /// <summary>
        /// Number of critical hits (natural max face rolls)
        /// </summary>
        public int Criticals { get; private set; }

        /// <summary>
        /// Did any dice roll critically?
        /// </summary>
        public bool HasCritical => Criticals > 0;

        /// <summary>
        /// Target number used for this roll
        /// </summary>
        public int TargetNumber { get; private set; }

        /// <summary>
        /// Modifier applied to rolls
        /// </summary>
        public int Modifier { get; private set; }

        /// <summary>
        /// Which dice were rerolled?
        /// </summary>
        public List<int> RerolledDice { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new dice roll result
        /// </summary>
        public DiceRollResult(int targetNumber, int modifier)
        {
            DiceRolls = new List<int>();
            ModifiedRolls = new List<int>();
            RerolledDice = new List<int>();
            TargetNumber = targetNumber;
            Modifier = modifier;
            Successes = 0;
            Criticals = 0;
        }
        #endregion

        #region Adding Results
        /// <summary>
        /// Add a die roll to the result
        /// </summary>
        /// <param name="roll">Die result</param>
        /// <param name="maxFace">Maximum face value (for critical detection)</param>
        public void AddRoll(int roll, int maxFace)
        {
            DiceRolls.Add(roll);
            int modifiedRoll = roll + Modifier;
            ModifiedRolls.Add(modifiedRoll);

            // Check for success
            if (modifiedRoll >= TargetNumber)
            {
                Successes++;
            }

            // Check for critical (natural max face)
            if (roll == maxFace)
            {
                Criticals++;
                // Critical adds an extra success
                Successes++;
            }
        }

        /// <summary>
        /// Mark a die as rerolled
        /// </summary>
        /// <param name="index">Index of die that was rerolled</param>
        public void MarkRerolled(int index)
        {
            if (!RerolledDice.Contains(index))
            {
                RerolledDice.Add(index);
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get average roll value
        /// </summary>
        public float GetAverageRoll()
        {
            if (DiceRolls.Count == 0)
                return 0f;

            float sum = 0f;
            foreach (int roll in DiceRolls)
            {
                sum += roll;
            }
            return sum / DiceRolls.Count;
        }

        /// <summary>
        /// Get highest roll
        /// </summary>
        public int GetHighestRoll()
        {
            if (DiceRolls.Count == 0)
                return 0;

            int highest = DiceRolls[0];
            foreach (int roll in DiceRolls)
            {
                if (roll > highest)
                    highest = roll;
            }
            return highest;
        }

        /// <summary>
        /// Get lowest roll
        /// </summary>
        public int GetLowestRoll()
        {
            if (DiceRolls.Count == 0)
                return 0;

            int lowest = DiceRolls[0];
            foreach (int roll in DiceRolls)
            {
                if (roll < lowest)
                    lowest = roll;
            }
            return lowest;
        }

        /// <summary>
        /// Get a formatted string of all rolls
        /// </summary>
        public string GetRollsString()
        {
            if (DiceRolls.Count == 0)
                return "No rolls";

            string result = "[";
            for (int i = 0; i < DiceRolls.Count; i++)
            {
                if (i > 0) result += ", ";
                
                int roll = DiceRolls[i];
                if (RerolledDice.Contains(i))
                {
                    result += $"({roll})"; // Rerolled dice in parentheses
                }
                else if (roll == 6) // Assuming d6
                {
                    result += $"*{roll}*"; // Criticals with asterisks
                }
                else
                {
                    result += roll.ToString();
                }
            }
            result += "]";
            return result;
        }

        /// <summary>
        /// Get a string representation of this result
        /// </summary>
        public override string ToString()
        {
            return $"{GetRollsString()} = {Successes} successes" + (HasCritical ? $" ({Criticals} crits)" : "");
        }
        #endregion
    }
}