using System.Collections.Generic;
using UnityEngine;

namespace Warslammer.Combat
{
    /// <summary>
    /// Manages stacked modifiers from various sources
    /// Tracks modifiers from abilities, terrain, equipment, etc.
    /// </summary>
    public class ModifierStack
    {
        #region Modifier Entry
        /// <summary>
        /// Individual modifier entry with source tracking
        /// </summary>
        public class ModifierEntry
        {
            public string Source { get; set; }
            public int DiceModifier { get; set; }
            public int ResultModifier { get; set; }
            public int DiceCountModifier { get; set; }
            public bool AllowsRerollOnes { get; set; }
            public bool AllowsRerollFailed { get; set; }

            public ModifierEntry(string source)
            {
                Source = source;
                DiceModifier = 0;
                ResultModifier = 0;
                DiceCountModifier = 0;
                AllowsRerollOnes = false;
                AllowsRerollFailed = false;
            }
        }
        #endregion

        #region Properties
        private List<ModifierEntry> _modifiers;

        /// <summary>
        /// All active modifiers
        /// </summary>
        public List<ModifierEntry> Modifiers => _modifiers;

        /// <summary>
        /// Total dice count modifier
        /// </summary>
        public int TotalDiceCountModifier
        {
            get
            {
                int total = 0;
                foreach (var mod in _modifiers)
                {
                    total += mod.DiceCountModifier;
                }
                return total;
            }
        }

        /// <summary>
        /// Total dice result modifier
        /// </summary>
        public int TotalDiceModifier
        {
            get
            {
                int total = 0;
                foreach (var mod in _modifiers)
                {
                    total += mod.DiceModifier;
                }
                return total;
            }
        }

        /// <summary>
        /// Total result modifier (applied after rolls)
        /// </summary>
        public int TotalResultModifier
        {
            get
            {
                int total = 0;
                foreach (var mod in _modifiers)
                {
                    total += mod.ResultModifier;
                }
                return total;
            }
        }

        /// <summary>
        /// Can reroll 1s?
        /// </summary>
        public bool CanRerollOnes
        {
            get
            {
                foreach (var mod in _modifiers)
                {
                    if (mod.AllowsRerollOnes)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Can reroll failed dice?
        /// </summary>
        public bool CanRerollFailed
        {
            get
            {
                foreach (var mod in _modifiers)
                {
                    if (mod.AllowsRerollFailed)
                        return true;
                }
                return false;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new modifier stack
        /// </summary>
        public ModifierStack()
        {
            _modifiers = new List<ModifierEntry>();
        }
        #endregion

        #region Adding Modifiers
        /// <summary>
        /// Add a dice count modifier
        /// </summary>
        /// <param name="source">Source of modifier</param>
        /// <param name="count">Number of dice to add/remove</param>
        public void AddDiceCountModifier(string source, int count)
        {
            ModifierEntry entry = GetOrCreateEntry(source);
            entry.DiceCountModifier += count;
        }

        /// <summary>
        /// Add a dice result modifier
        /// </summary>
        /// <param name="source">Source of modifier</param>
        /// <param name="modifier">Modifier to add to each die result</param>
        public void AddDiceModifier(string source, int modifier)
        {
            ModifierEntry entry = GetOrCreateEntry(source);
            entry.DiceModifier += modifier;
        }

        /// <summary>
        /// Add a result modifier (applied after rolling)
        /// </summary>
        /// <param name="source">Source of modifier</param>
        /// <param name="modifier">Modifier to add to final result</param>
        public void AddResultModifier(string source, int modifier)
        {
            ModifierEntry entry = GetOrCreateEntry(source);
            entry.ResultModifier += modifier;
        }

        /// <summary>
        /// Enable reroll 1s
        /// </summary>
        /// <param name="source">Source of ability</param>
        public void AddRerollOnes(string source)
        {
            ModifierEntry entry = GetOrCreateEntry(source);
            entry.AllowsRerollOnes = true;
        }

        /// <summary>
        /// Enable reroll failed
        /// </summary>
        /// <param name="source">Source of ability</param>
        public void AddRerollFailed(string source)
        {
            ModifierEntry entry = GetOrCreateEntry(source);
            entry.AllowsRerollFailed = true;
        }

        /// <summary>
        /// Get or create a modifier entry for a source
        /// </summary>
        private ModifierEntry GetOrCreateEntry(string source)
        {
            foreach (var mod in _modifiers)
            {
                if (mod.Source == source)
                    return mod;
            }

            ModifierEntry entry = new ModifierEntry(source);
            _modifiers.Add(entry);
            return entry;
        }
        #endregion

        #region Removing Modifiers
        /// <summary>
        /// Remove all modifiers from a specific source
        /// </summary>
        /// <param name="source">Source to remove</param>
        public void RemoveSource(string source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
        }

        /// <summary>
        /// Clear all modifiers
        /// </summary>
        public void Clear()
        {
            _modifiers.Clear();
        }
        #endregion

        #region Application
        /// <summary>
        /// Apply this modifier stack to a dice pool
        /// </summary>
        /// <param name="pool">Dice pool to modify</param>
        public void ApplyToPool(DicePool pool)
        {
            if (pool == null)
                return;

            // Apply dice count modifiers
            pool.AddDice(TotalDiceCountModifier);

            // Apply dice result modifiers
            pool.AddModifier(TotalDiceModifier);

            // Apply reroll abilities
            if (CanRerollOnes)
                pool.SetRerollOnes(true);

            if (CanRerollFailed)
                pool.SetRerollFailed(true);
        }

        /// <summary>
        /// Create a modified copy of a dice pool
        /// </summary>
        /// <param name="basePool">Base pool to copy and modify</param>
        /// <returns>Modified dice pool</returns>
        public DicePool CreateModifiedPool(DicePool basePool)
        {
            if (basePool == null)
                return new DicePool();

            DicePool modified = basePool.Clone();
            ApplyToPool(modified);
            return modified;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a summary of all modifiers
        /// </summary>
        public string GetSummary()
        {
            if (_modifiers.Count == 0)
                return "No modifiers";

            string summary = "Modifiers:\n";
            foreach (var mod in _modifiers)
            {
                summary += $"  {mod.Source}:";
                if (mod.DiceCountModifier != 0)
                    summary += $" {(mod.DiceCountModifier > 0 ? "+" : "")}{mod.DiceCountModifier} dice";
                if (mod.DiceModifier != 0)
                    summary += $" {(mod.DiceModifier > 0 ? "+" : "")}{mod.DiceModifier} to rolls";
                if (mod.ResultModifier != 0)
                    summary += $" {(mod.ResultModifier > 0 ? "+" : "")}{mod.ResultModifier} to result";
                if (mod.AllowsRerollOnes)
                    summary += " [Reroll 1s]";
                if (mod.AllowsRerollFailed)
                    summary += " [Reroll Failed]";
                summary += "\n";
            }
            return summary;
        }

        /// <summary>
        /// Check if any modifiers are active
        /// </summary>
        public bool HasModifiers()
        {
            return _modifiers.Count > 0;
        }

        /// <summary>
        /// Get number of modifier sources
        /// </summary>
        public int GetSourceCount()
        {
            return _modifiers.Count;
        }
        #endregion
    }
}