using UnityEngine;

namespace Warslammer.Combat
{
    /// <summary>
    /// Enum for status effect types
    /// </summary>
    public enum StatusEffectType
    {
        Bleed,
        Rooted,
        Shaken,
        Guard,
        Stunned,
        Poisoned,
        Burning,
        Frozen
    }

    /// <summary>
    /// Base class for status effects applied to units
    /// Handles duration, stacking, and per-turn effects
    /// </summary>
    public abstract class StatusEffect
    {
        #region Properties
        /// <summary>
        /// Type of status effect
        /// </summary>
        public StatusEffectType Type { get; protected set; }

        /// <summary>
        /// Display name of the effect
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Description of what the effect does
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Remaining duration in turns (-1 for permanent)
        /// </summary>
        public int RemainingDuration { get; protected set; }

        /// <summary>
        /// Maximum duration
        /// </summary>
        public int MaxDuration { get; protected set; }

        /// <summary>
        /// Can this effect stack?
        /// </summary>
        public bool CanStack { get; protected set; }

        /// <summary>
        /// Current stack count
        /// </summary>
        public int StackCount { get; protected set; }

        /// <summary>
        /// Maximum stack count
        /// </summary>
        public int MaxStacks { get; protected set; }

        /// <summary>
        /// Source of the effect (for tracking)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Is this effect active?
        /// </summary>
        public bool IsActive => RemainingDuration != 0;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new status effect
        /// </summary>
        protected StatusEffect(StatusEffectType type, string name, string description, int duration, bool canStack = false, int maxStacks = 1)
        {
            Type = type;
            Name = name;
            Description = description;
            RemainingDuration = duration;
            MaxDuration = duration;
            CanStack = canStack;
            StackCount = 1;
            MaxStacks = maxStacks;
            Source = "Unknown";
        }
        #endregion

        #region Virtual Methods
        /// <summary>
        /// Called when effect is first applied
        /// </summary>
        public virtual void OnApplied(Units.Unit target)
        {
            Debug.Log($"[StatusEffect] {Name} applied to {target.GetUnitName()} for {RemainingDuration} turns");
        }

        /// <summary>
        /// Called at the start of each turn
        /// </summary>
        public virtual void OnTurnStart(Units.Unit target)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called at the end of each turn
        /// </summary>
        public virtual void OnTurnEnd(Units.Unit target)
        {
            // Decrement duration
            if (RemainingDuration > 0)
            {
                RemainingDuration--;
            }
        }

        /// <summary>
        /// Called when effect is removed
        /// </summary>
        public virtual void OnRemoved(Units.Unit target)
        {
            Debug.Log($"[StatusEffect] {Name} removed from {target.GetUnitName()}");
        }

        /// <summary>
        /// Called when effect stacks are added
        /// </summary>
        public virtual void OnStackAdded(Units.Unit target)
        {
            if (CanStack && StackCount < MaxStacks)
            {
                StackCount++;
                Debug.Log($"[StatusEffect] {Name} stacked to {StackCount} on {target.GetUnitName()}");
            }
        }

        /// <summary>
        /// Refresh the duration of this effect
        /// </summary>
        public virtual void RefreshDuration()
        {
            RemainingDuration = MaxDuration;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get display string for UI
        /// </summary>
        public virtual string GetDisplayString()
        {
            string display = Name;
            if (StackCount > 1)
                display += $" x{StackCount}";
            if (RemainingDuration > 0)
                display += $" ({RemainingDuration})";
            return display;
        }
        #endregion
    }

    #region Specific Status Effects
    /// <summary>
    /// Bleed effect - lose HP at end of turn
    /// </summary>
    public class BleedEffect : StatusEffect
    {
        private int _damagePerTurn;

        public BleedEffect(int duration = 3, int damagePerTurn = 1) 
            : base(StatusEffectType.Bleed, "Bleed", "Lose HP at the end of each turn", duration, true, 5)
        {
            _damagePerTurn = damagePerTurn;
        }

        public override void OnTurnEnd(Units.Unit target)
        {
            base.OnTurnEnd(target);
            
            int totalDamage = _damagePerTurn * StackCount;
            target.TakeDamage(totalDamage, Core.DamageSource.Physical);
            Debug.Log($"[BleedEffect] {target.GetUnitName()} takes {totalDamage} bleed damage");
        }
    }

    /// <summary>
    /// Rooted effect - cannot move
    /// </summary>
    public class RootedEffect : StatusEffect
    {
        public RootedEffect(int duration = 1) 
            : base(StatusEffectType.Rooted, "Rooted", "Cannot move", duration)
        {
        }

        public override void OnApplied(Units.Unit target)
        {
            base.OnApplied(target);
            // Set remaining movement to 0
            if (target.TryGetComponent<Units.UnitStats>(out var stats))
            {
                stats.SetRemainingMovement(0);
            }
        }
    }

    /// <summary>
    /// Shaken effect - penalty to all rolls (from morale failure)
    /// </summary>
    public class ShakenEffect : StatusEffect
    {
        public ShakenEffect(int duration = -1) 
            : base(StatusEffectType.Shaken, "Shaken", "-1 die on all rolls", duration)
        {
        }
    }

    /// <summary>
    /// Guard effect - bonus defense on next attack
    /// </summary>
    public class GuardEffect : StatusEffect
    {
        public GuardEffect(int duration = 1) 
            : base(StatusEffectType.Guard, "Guard", "+1 DEF die vs next attack", duration)
        {
        }
    }

    /// <summary>
    /// Stunned effect - cannot act
    /// </summary>
    public class StunnedEffect : StatusEffect
    {
        public StunnedEffect(int duration = 1) 
            : base(StatusEffectType.Stunned, "Stunned", "Cannot take actions", duration)
        {
        }

        public override void OnApplied(Units.Unit target)
        {
            base.OnApplied(target);
            if (target.TryGetComponent<Units.UnitStats>(out var stats))
            {
                stats.Stun();
            }
        }

        public override void OnRemoved(Units.Unit target)
        {
            base.OnRemoved(target);
            if (target.TryGetComponent<Units.UnitStats>(out var stats))
            {
                stats.RemoveStun();
            }
        }
    }

    /// <summary>
    /// Poisoned effect - take damage over time
    /// </summary>
    public class PoisonedEffect : StatusEffect
    {
        private int _damagePerTurn;

        public PoisonedEffect(int duration = 3, int damagePerTurn = 1) 
            : base(StatusEffectType.Poisoned, "Poisoned", "Take poison damage each turn", duration, true, 3)
        {
            _damagePerTurn = damagePerTurn;
        }

        public override void OnTurnStart(Units.Unit target)
        {
            base.OnTurnStart(target);
            
            int totalDamage = _damagePerTurn * StackCount;
            target.TakeDamage(totalDamage, Core.DamageSource.Poison);
            Debug.Log($"[PoisonedEffect] {target.GetUnitName()} takes {totalDamage} poison damage");
        }
    }
    #endregion
}