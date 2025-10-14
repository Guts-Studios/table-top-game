namespace Warslammer.Core
{
    /// <summary>
    /// Interface for objects that can take damage
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Current health points
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Maximum health points
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// Is this unit currently alive?
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// Apply damage to this object
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        /// <param name="damageSource">Source/type of damage</param>
        /// <returns>Actual damage taken after resistances</returns>
        int TakeDamage(int damage, DamageSource damageSource);

        /// <summary>
        /// Heal this object
        /// </summary>
        /// <param name="healAmount">Amount to heal</param>
        /// <returns>Actual amount healed</returns>
        int Heal(int healAmount);

        /// <summary>
        /// Called when this object's health reaches zero
        /// </summary>
        void OnDeath();
    }
}