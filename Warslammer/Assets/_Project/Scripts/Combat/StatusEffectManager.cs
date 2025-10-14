using System.Collections.Generic;
using UnityEngine;
using Warslammer.Units;

namespace Warslammer.Combat
{
    /// <summary>
    /// Manages status effects on a unit
    /// Handles application, removal, and turn-based updates
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        #region Properties
        [Header("Active Effects")]
        [SerializeField]
        [Tooltip("List of active status effects")]
        private List<StatusEffect> _activeEffects = new List<StatusEffect>();

        /// <summary>
        /// All active status effects
        /// </summary>
        public List<StatusEffect> ActiveEffects => _activeEffects;

        /// <summary>
        /// Number of active effects
        /// </summary>
        public int EffectCount => _activeEffects.Count;

        private Unit _unit;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _unit = GetComponent<Unit>();
            if (_unit == null)
            {
                Debug.LogError("[StatusEffectManager] No Unit component found!");
            }
        }
        #endregion

        #region Adding/Removing Effects
        /// <summary>
        /// Add a status effect to the unit
        /// </summary>
        /// <param name="effect">Effect to add</param>
        public void AddEffect(StatusEffect effect)
        {
            if (effect == null)
                return;

            // Check if effect already exists
            StatusEffect existing = GetEffect(effect.Type);
            if (existing != null)
            {
                // If effect can stack, add a stack
                if (existing.CanStack)
                {
                    existing.OnStackAdded(_unit);
                }
                else
                {
                    // Refresh duration instead
                    existing.RefreshDuration();
                    Debug.Log($"[StatusEffectManager] Refreshed {effect.Name} on {_unit.GetUnitName()}");
                }
            }
            else
            {
                // Add new effect
                _activeEffects.Add(effect);
                effect.OnApplied(_unit);
            }
        }

        /// <summary>
        /// Remove a status effect by type
        /// </summary>
        /// <param name="type">Type of effect to remove</param>
        public void RemoveEffect(StatusEffectType type)
        {
            StatusEffect effect = GetEffect(type);
            if (effect != null)
            {
                effect.OnRemoved(_unit);
                _activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Remove a specific status effect instance
        /// </summary>
        /// <param name="effect">Effect to remove</param>
        public void RemoveEffect(StatusEffect effect)
        {
            if (effect != null && _activeEffects.Contains(effect))
            {
                effect.OnRemoved(_unit);
                _activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Clear all status effects
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (StatusEffect effect in _activeEffects)
            {
                effect?.OnRemoved(_unit);
            }
            _activeEffects.Clear();
        }

        /// <summary>
        /// Remove all expired effects
        /// </summary>
        private void RemoveExpiredEffects()
        {
            List<StatusEffect> expired = new List<StatusEffect>();
            
            foreach (StatusEffect effect in _activeEffects)
            {
                if (!effect.IsActive)
                {
                    expired.Add(effect);
                }
            }

            foreach (StatusEffect effect in expired)
            {
                RemoveEffect(effect);
            }
        }
        #endregion

        #region Queries
        /// <summary>
        /// Get a status effect by type
        /// </summary>
        /// <param name="type">Type of effect</param>
        /// <returns>Effect or null if not found</returns>
        public StatusEffect GetEffect(StatusEffectType type)
        {
            foreach (StatusEffect effect in _activeEffects)
            {
                if (effect.Type == type)
                    return effect;
            }
            return null;
        }

        /// <summary>
        /// Check if unit has a specific status effect
        /// </summary>
        /// <param name="type">Type of effect</param>
        /// <returns>True if effect is active</returns>
        public bool HasEffect(StatusEffectType type)
        {
            return GetEffect(type) != null;
        }

        /// <summary>
        /// Get all effects of a specific type
        /// </summary>
        /// <param name="type">Type of effect</param>
        /// <returns>List of matching effects</returns>
        public List<StatusEffect> GetEffectsOfType(StatusEffectType type)
        {
            List<StatusEffect> effects = new List<StatusEffect>();
            foreach (StatusEffect effect in _activeEffects)
            {
                if (effect.Type == type)
                    effects.Add(effect);
            }
            return effects;
        }

        /// <summary>
        /// Check if unit is stunned
        /// </summary>
        public bool IsStunned()
        {
            return HasEffect(StatusEffectType.Stunned);
        }

        /// <summary>
        /// Check if unit is rooted
        /// </summary>
        public bool IsRooted()
        {
            return HasEffect(StatusEffectType.Rooted);
        }

        /// <summary>
        /// Check if unit is shaken
        /// </summary>
        public bool IsShaken()
        {
            return HasEffect(StatusEffectType.Shaken);
        }

        /// <summary>
        /// Get total penalty to dice pools (from Shaken)
        /// </summary>
        public int GetDicePenalty()
        {
            int penalty = 0;
            if (HasEffect(StatusEffectType.Shaken))
            {
                penalty += 1; // -1 die from Shaken
            }
            return penalty;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Called at start of unit's turn
        /// </summary>
        public void OnTurnStart()
        {
            foreach (StatusEffect effect in _activeEffects)
            {
                effect.OnTurnStart(_unit);
            }
        }

        /// <summary>
        /// Called at end of unit's turn
        /// </summary>
        public void OnTurnEnd()
        {
            foreach (StatusEffect effect in _activeEffects)
            {
                effect.OnTurnEnd(_unit);
            }

            // Remove expired effects
            RemoveExpiredEffects();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get summary of all active effects
        /// </summary>
        public string GetEffectsSummary()
        {
            if (_activeEffects.Count == 0)
                return "No status effects";

            string summary = "Status Effects:\n";
            foreach (StatusEffect effect in _activeEffects)
            {
                summary += $"  - {effect.GetDisplayString()}\n";
            }
            return summary;
        }

        /// <summary>
        /// Get list of effect names for UI
        /// </summary>
        public List<string> GetEffectNames()
        {
            List<string> names = new List<string>();
            foreach (StatusEffect effect in _activeEffects)
            {
                names.Add(effect.GetDisplayString());
            }
            return names;
        }
        #endregion

        #region Quick Apply Methods
        /// <summary>
        /// Apply bleed effect
        /// </summary>
        /// <param name="duration">Duration in turns</param>
        /// <param name="damagePerTurn">Damage per turn</param>
        public void ApplyBleed(int duration = 3, int damagePerTurn = 1)
        {
            AddEffect(new BleedEffect(duration, damagePerTurn));
        }

        /// <summary>
        /// Apply rooted effect
        /// </summary>
        /// <param name="duration">Duration in turns</param>
        public void ApplyRooted(int duration = 1)
        {
            AddEffect(new RootedEffect(duration));
        }

        /// <summary>
        /// Apply shaken effect (from morale)
        /// </summary>
        public void ApplyShaken()
        {
            AddEffect(new ShakenEffect(-1)); // Permanent until rallied
        }

        /// <summary>
        /// Remove shaken effect (rally)
        /// </summary>
        public void RemoveShaken()
        {
            RemoveEffect(StatusEffectType.Shaken);
        }

        /// <summary>
        /// Apply guard effect
        /// </summary>
        /// <param name="duration">Duration in turns</param>
        public void ApplyGuard(int duration = 1)
        {
            AddEffect(new GuardEffect(duration));
        }

        /// <summary>
        /// Apply stunned effect
        /// </summary>
        /// <param name="duration">Duration in turns</param>
        public void ApplyStunned(int duration = 1)
        {
            AddEffect(new StunnedEffect(duration));
        }

        /// <summary>
        /// Apply poisoned effect
        /// </summary>
        /// <param name="duration">Duration in turns</param>
        /// <param name="damagePerTurn">Damage per turn</param>
        public void ApplyPoisoned(int duration = 3, int damagePerTurn = 1)
        {
            AddEffect(new PoisonedEffect(duration, damagePerTurn));
        }
        #endregion
    }
}