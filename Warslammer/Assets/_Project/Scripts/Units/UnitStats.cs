using UnityEngine;
using Warslammer.Core;
using Warslammer.Data;

namespace Warslammer.Units
{
    /// <summary>
    /// Runtime stats component for units
    /// Tracks current HP, modifiers, status effects, etc.
    /// </summary>
    public class UnitStats : MonoBehaviour
    {
        #region Properties
        [Header("Health")]
        [SerializeField]
        private int _currentHealth;
        
        /// <summary>
        /// Current health points
        /// </summary>
        public int CurrentHealth
        {
            get => _currentHealth;
            private set => _currentHealth = Mathf.Max(0, value);
        }

        [SerializeField]
        private int _maxHealth;
        
        /// <summary>
        /// Maximum health points
        /// </summary>
        public int MaxHealth
        {
            get => _maxHealth;
            private set => _maxHealth = Mathf.Max(1, value);
        }

        [Header("Combat Stats")]
        [SerializeField]
        private int _attack;
        
        /// <summary>
        /// Current attack value
        /// </summary>
        public int Attack => _attack + _attackModifier;

        [SerializeField]
        private int _defense;
        
        /// <summary>
        /// Current defense value
        /// </summary>
        public int Defense => _defense + _defenseModifier;

        [SerializeField]
        private int _armor;
        
        /// <summary>
        /// Current armor value
        /// </summary>
        public int Armor => _armor + _armorModifier;

        [Header("Movement")]
        [SerializeField]
        private float _movementSpeed;
        
        /// <summary>
        /// Movement speed in inches
        /// </summary>
        public float MovementSpeed => _movementSpeed + _movementModifier;

        [SerializeField]
        private float _remainingMovement;
        
        /// <summary>
        /// Remaining movement for this turn
        /// </summary>
        public float RemainingMovement
        {
            get => _remainingMovement;
            set => _remainingMovement = Mathf.Max(0, value);
        }

        [Header("Stat Modifiers")]
        [SerializeField]
        private int _attackModifier;
        
        [SerializeField]
        private int _defenseModifier;
        
        [SerializeField]
        private int _armorModifier;
        
        [SerializeField]
        private float _movementModifier;

        [Header("Status")]
        [SerializeField]
        private bool _isStunned;
        
        /// <summary>
        /// Is this unit stunned?
        /// </summary>
        public bool IsStunned
        {
            get => _isStunned;
            set => _isStunned = value;
        }

        [SerializeField]
        private bool _isPoisoned;
        
        /// <summary>
        /// Is this unit poisoned?
        /// </summary>
        public bool IsPoisoned
        {
            get => _isPoisoned;
            set => _isPoisoned = value;
        }

        /// <summary>
        /// Is this unit alive?
        /// </summary>
        public bool IsAlive => _currentHealth > 0;

        /// <summary>
        /// Health percentage (0.0 to 1.0)
        /// </summary>
        public float HealthPercentage => (float)_currentHealth / _maxHealth;

        private UnitData _baseData;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize stats from unit data
        /// </summary>
        /// <param name="unitData">Unit data to initialize from</param>
        public void Initialize(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogError("[UnitStats] Cannot initialize with null unit data!");
                return;
            }

            _baseData = unitData;

            // Initialize stats from data
            _maxHealth = unitData.maxHealth;
            _currentHealth = _maxHealth;
            _attack = unitData.attack;
            _defense = unitData.defense;
            _armor = unitData.armor;
            _movementSpeed = unitData.movementSpeed;
            _remainingMovement = _movementSpeed;

            // Reset modifiers
            ResetModifiers();

            Debug.Log($"[UnitStats] Initialized {unitData.unitName} - HP: {_currentHealth}/{_maxHealth}, Move: {_movementSpeed}\"");
        }

        /// <summary>
        /// Reset all stat modifiers
        /// </summary>
        public void ResetModifiers()
        {
            _attackModifier = 0;
            _defenseModifier = 0;
            _armorModifier = 0;
            _movementModifier = 0;
        }
        #endregion

        #region Health Management
        /// <summary>
        /// Take damage
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        /// <param name="damageSource">Source of damage</param>
        /// <returns>Actual damage taken</returns>
        public int TakeDamage(int damage, DamageSource damageSource)
        {
            if (!IsAlive)
                return 0;

            // Apply damage resistances/vulnerabilities (if base data exists)
            float damageMultiplier = 1f;
            
            if (_baseData != null)
            {
                if (_baseData.IsImmuneTo(damageSource))
                {
                    Debug.Log($"[UnitStats] {name} is immune to {damageSource} damage!");
                    return 0;
                }
                
                if (_baseData.IsResistantTo(damageSource))
                {
                    damageMultiplier = 0.5f;
                    Debug.Log($"[UnitStats] {name} resists {damageSource} damage!");
                }
                else if (_baseData.IsVulnerableTo(damageSource))
                {
                    damageMultiplier = 1.5f;
                    Debug.Log($"[UnitStats] {name} is vulnerable to {damageSource} damage!");
                }
            }

            // Calculate actual damage
            int modifiedDamage = Mathf.RoundToInt(damage * damageMultiplier);
            int finalDamage = Mathf.Max(1, modifiedDamage - Armor); // Armor reduces damage, minimum 1

            CurrentHealth -= finalDamage;

            Debug.Log($"[UnitStats] {name} took {finalDamage} damage ({CurrentHealth}/{MaxHealth} HP remaining)");

            return finalDamage;
        }

        /// <summary>
        /// Heal unit
        /// </summary>
        /// <param name="healAmount">Amount to heal</param>
        /// <returns>Actual amount healed</returns>
        public int Heal(int healAmount)
        {
            if (!IsAlive)
                return 0;

            int previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + healAmount);
            int actualHealed = CurrentHealth - previousHealth;

            Debug.Log($"[UnitStats] {name} healed for {actualHealed} HP ({CurrentHealth}/{MaxHealth})");

            return actualHealed;
        }

        /// <summary>
        /// Set current health directly
        /// </summary>
        /// <param name="health">New health value</param>
        public void SetHealth(int health)
        {
            CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        }

        /// <summary>
        /// Set max health
        /// </summary>
        /// <param name="maxHealth">New max health</param>
        /// <param name="healToMax">Heal to new max?</param>
        public void SetMaxHealth(int maxHealth, bool healToMax = false)
        {
            MaxHealth = maxHealth;
            
            if (healToMax)
            {
                CurrentHealth = MaxHealth;
            }
            else
            {
                CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
            }
        }
        #endregion

        #region Movement Management
        /// <summary>
        /// Consume movement
        /// </summary>
        /// <param name="movementCost">Movement cost in inches</param>
        /// <returns>True if movement was consumed</returns>
        public bool ConsumeMovement(float movementCost)
        {
            if (RemainingMovement < movementCost)
                return false;

            RemainingMovement -= movementCost;
            return true;
        }

        /// <summary>
        /// Reset movement for new turn
        /// </summary>
        public void ResetMovement()
        {
            RemainingMovement = MovementSpeed;
        }

        /// <summary>
        /// Set remaining movement
        /// </summary>
        /// <param name="movement">New remaining movement</param>
        public void SetRemainingMovement(float movement)
        {
            RemainingMovement = movement;
        }
        #endregion

        #region Stat Modifiers
        /// <summary>
        /// Add attack modifier
        /// </summary>
        /// <param name="modifier">Modifier value</param>
        public void AddAttackModifier(int modifier)
        {
            _attackModifier += modifier;
        }

        /// <summary>
        /// Add defense modifier
        /// </summary>
        /// <param name="modifier">Modifier value</param>
        public void AddDefenseModifier(int modifier)
        {
            _defenseModifier += modifier;
        }

        /// <summary>
        /// Add armor modifier
        /// </summary>
        /// <param name="modifier">Modifier value</param>
        public void AddArmorModifier(int modifier)
        {
            _armorModifier += modifier;
        }

        /// <summary>
        /// Add movement modifier
        /// </summary>
        /// <param name="modifier">Modifier value in inches</param>
        public void AddMovementModifier(float modifier)
        {
            _movementModifier += modifier;
        }
        #endregion

        #region Status Effects
        /// <summary>
        /// Apply stun effect
        /// </summary>
        public void Stun()
        {
            IsStunned = true;
            Debug.Log($"[UnitStats] {name} is stunned!");
        }

        /// <summary>
        /// Remove stun effect
        /// </summary>
        public void RemoveStun()
        {
            IsStunned = false;
        }

        /// <summary>
        /// Apply poison effect
        /// </summary>
        public void Poison()
        {
            IsPoisoned = true;
            Debug.Log($"[UnitStats] {name} is poisoned!");
        }

        /// <summary>
        /// Remove poison effect
        /// </summary>
        public void RemovePoison()
        {
            IsPoisoned = false;
        }

        /// <summary>
        /// Clear all status effects
        /// </summary>
        public void ClearStatusEffects()
        {
            IsStunned = false;
            IsPoisoned = false;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Reset stats for new turn
        /// </summary>
        public void OnTurnStart()
        {
            ResetMovement();
            
            // Apply poison damage
            if (IsPoisoned && IsAlive)
            {
                TakeDamage(1, DamageSource.Poison);
            }
        }

        /// <summary>
        /// Called at end of turn
        /// </summary>
        public void OnTurnEnd()
        {
            // TODO: Phase 3 - Apply regeneration, cooldown updates, etc.
        }
        #endregion
    }
}