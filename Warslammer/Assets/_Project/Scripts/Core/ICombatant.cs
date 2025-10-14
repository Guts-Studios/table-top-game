using Warslammer.Data;

namespace Warslammer.Core
{
    /// <summary>
    /// Interface for units that can engage in combat
    /// </summary>
    public interface ICombatant
    {
        /// <summary>
        /// Unit data for this combatant
        /// </summary>
        UnitData UnitData { get; }

        /// <summary>
        /// Current attack value
        /// </summary>
        int Attack { get; }

        /// <summary>
        /// Current defense value
        /// </summary>
        int Defense { get; }

        /// <summary>
        /// Current armor value
        /// </summary>
        int Armor { get; }

        /// <summary>
        /// Can this unit attack this turn?
        /// </summary>
        bool CanAttack { get; }

        /// <summary>
        /// Has this unit attacked this turn?
        /// </summary>
        bool HasAttacked { get; }

        /// <summary>
        /// Is this unit currently engaged in melee?
        /// </summary>
        bool IsEngaged { get; }

        /// <summary>
        /// Attack a target
        /// </summary>
        /// <param name="target">Target to attack</param>
        /// <returns>True if attack was successful</returns>
        bool Attack(ICombatant target);

        /// <summary>
        /// Check if this unit can attack a target
        /// </summary>
        /// <param name="target">Potential target</param>
        /// <returns>True if target is valid</returns>
        bool CanAttackTarget(ICombatant target);

        /// <summary>
        /// Get the distance to a target
        /// </summary>
        /// <param name="target">Target to measure distance to</param>
        /// <returns>Distance in inches</returns>
        float GetDistanceTo(ICombatant target);

        /// <summary>
        /// Reset combat state for a new turn
        /// </summary>
        void ResetCombat();
    }
}