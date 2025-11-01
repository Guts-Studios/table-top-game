using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Warslammer.Core;
using Warslammer.Data;
using Warslammer.Combat;
using Warslammer.Battlefield;

namespace Warslammer.Units
{
    /// <summary>
    /// Controls ability usage for a unit
    /// Handles cooldowns, targeting validation, and ability execution
    /// </summary>
    public class AbilityController : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Fired when an ability is used
        /// </summary>
        public UnityEvent<AbilityData, Unit> OnAbilityUsed = new UnityEvent<AbilityData, Unit>();

        /// <summary>
        /// Fired when an ability cast fails
        /// </summary>
        public UnityEvent<AbilityData, string> OnAbilityCastFailed = new UnityEvent<AbilityData, string>();
        #endregion

        #region Serialized Fields
        [Header("Abilities")]
        [Tooltip("List of abilities this unit can use")]
        [SerializeField]
        private List<AbilityData> _abilities = new List<AbilityData>();

        [Tooltip("Max number of abilities this unit can have")]
        [SerializeField]
        private int _maxAbilities = 4;

        [Header("Resources")]
        [Tooltip("Action points available this turn")]
        [SerializeField]
        private int _actionPoints = 1;

        [Tooltip("Max action points per turn")]
        [SerializeField]
        private int _maxActionPoints = 1;
        #endregion

        #region Properties
        /// <summary>
        /// List of abilities this unit has
        /// </summary>
        public List<AbilityData> Abilities => _abilities;

        /// <summary>
        /// Current action points
        /// </summary>
        public int ActionPoints => _actionPoints;

        /// <summary>
        /// Maximum action points
        /// </summary>
        public int MaxActionPoints => _maxActionPoints;
        #endregion

        #region Private Fields
        private Unit _unit;
        private StatusEffectManager _statusEffectManager;
        private Dictionary<AbilityData, int> _abilityCooldowns = new Dictionary<AbilityData, int>();
        private Dictionary<AbilityData, int> _abilityLastUsedTurn = new Dictionary<AbilityData, int>();
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _statusEffectManager = GetComponent<StatusEffectManager>();

            // Initialize cooldown tracking
            foreach (var ability in _abilities)
            {
                if (ability != null)
                {
                    _abilityCooldowns[ability] = 0;
                    _abilityLastUsedTurn[ability] = -999; // Large negative number = ready to use
                }
            }
        }
        #endregion

        #region Ability Management
        /// <summary>
        /// Add an ability to this unit
        /// </summary>
        public bool AddAbility(AbilityData ability)
        {
            if (ability == null)
            {
                Debug.LogWarning("[AbilityController] Cannot add null ability!");
                return false;
            }

            if (_abilities.Count >= _maxAbilities)
            {
                Debug.LogWarning($"[AbilityController] {_unit.GetUnitName()} already has max abilities ({_maxAbilities})!");
                return false;
            }

            if (_abilities.Contains(ability))
            {
                Debug.LogWarning($"[AbilityController] {_unit.GetUnitName()} already has {ability.abilityName}!");
                return false;
            }

            _abilities.Add(ability);
            _abilityCooldowns[ability] = 0;
            _abilityLastUsedTurn[ability] = -999;

            Debug.Log($"[AbilityController] {_unit.GetUnitName()} learned {ability.abilityName}!");
            return true;
        }

        /// <summary>
        /// Remove an ability from this unit
        /// </summary>
        public bool RemoveAbility(AbilityData ability)
        {
            if (!_abilities.Contains(ability))
                return false;

            _abilities.Remove(ability);
            _abilityCooldowns.Remove(ability);
            _abilityLastUsedTurn.Remove(ability);

            Debug.Log($"[AbilityController] {_unit.GetUnitName()} forgot {ability.abilityName}");
            return true;
        }

        /// <summary>
        /// Check if unit has a specific ability
        /// </summary>
        public bool HasAbility(AbilityData ability)
        {
            return _abilities.Contains(ability);
        }

        /// <summary>
        /// Get ability by ID
        /// </summary>
        public AbilityData GetAbilityByID(string abilityID)
        {
            return _abilities.Find(a => a.abilityID == abilityID);
        }
        #endregion

        #region Ability Execution
        /// <summary>
        /// Use an ability on a target
        /// </summary>
        public bool UseAbility(AbilityData ability, Unit target = null)
        {
            // Validate ability
            if (!CanUseAbility(ability, target, out string failureReason))
            {
                Debug.LogWarning($"[AbilityController] Cannot use {ability.abilityName}: {failureReason}");
                OnAbilityCastFailed?.Invoke(ability, failureReason);
                return false;
            }

            // Pay costs
            _actionPoints -= ability.actionPointCost;
            if (ability.healthCost > 0)
            {
                _unit.TakeDamage(ability.healthCost, DamageSource.Physical); // Health cost as physical damage
            }

            // Execute ability based on type
            bool success = ExecuteAbility(ability, target);

            if (success)
            {
                // Update cooldown
                int currentTurn = TurnManager.Instance != null ? TurnManager.Instance.CurrentTurn : 0;
                _abilityLastUsedTurn[ability] = currentTurn;
                _abilityCooldowns[ability] = ability.cooldownTurns;

                // Fire event
                OnAbilityUsed?.Invoke(ability, target);

                Debug.Log($"[AbilityController] {_unit.GetUnitName()} used {ability.abilityName}" +
                         (target != null ? $" on {target.GetUnitName()}" : ""));
            }

            return success;
        }

        /// <summary>
        /// Check if an ability can be used
        /// </summary>
        public bool CanUseAbility(AbilityData ability, Unit target, out string failureReason)
        {
            failureReason = "";

            // Check if unit has this ability
            if (!HasAbility(ability))
            {
                failureReason = "Unit does not have this ability";
                return false;
            }

            // Check if unit is alive
            if (!_unit.IsAlive)
            {
                failureReason = "Unit is dead";
                return false;
            }

            // Check if stunned
            if (_statusEffectManager != null && _statusEffectManager.IsStunned())
            {
                failureReason = "Unit is stunned";
                return false;
            }

            // Check action points
            if (_actionPoints < ability.actionPointCost)
            {
                failureReason = $"Not enough action points ({_actionPoints}/{ability.actionPointCost})";
                return false;
            }

            // Check health cost
            if (ability.healthCost > 0 && _unit.CurrentHealth <= ability.healthCost)
            {
                failureReason = "Not enough health to pay cost";
                return false;
            }

            // Check cooldown
            if (IsAbilityOnCooldown(ability))
            {
                failureReason = $"Ability on cooldown ({GetAbilityCooldown(ability)} turns remaining)";
                return false;
            }

            // Check if engaged in melee (for abilities that can't be used in melee)
            if (!ability.usableInMelee && _unit.IsEngaged)
            {
                failureReason = "Cannot use ability while engaged in melee";
                return false;
            }

            // Validate target for targeted abilities
            if (ability.rangeType != RangeType.Self && ability.rangeType != RangeType.Aura)
            {
                if (target == null)
                {
                    failureReason = "No target specified";
                    return false;
                }

                if (!target.IsAlive)
                {
                    failureReason = "Target is dead";
                    return false;
                }

                // Check ally/enemy targeting
                bool isAlly = target.OwnerPlayerIndex == _unit.OwnerPlayerIndex;
                bool isEnemy = !isAlly;

                if (isAlly && !ability.canTargetAllies)
                {
                    failureReason = "Cannot target allies";
                    return false;
                }

                if (isEnemy && !ability.canTargetEnemies)
                {
                    failureReason = "Cannot target enemies";
                    return false;
                }

                // Check range
                float distance = Vector3.Distance(_unit.Position, target.Position);
                float distanceInches = distance; // Assuming 1 Unity unit = 1 inch
                if (distanceInches > ability.range)
                {
                    failureReason = $"Target out of range ({distanceInches:F1}\" > {ability.range}\")";
                    return false;
                }

                // Check line of sight
                if (ability.requiresLineOfSight)
                {
                    var losManager = BattlefieldManager.Instance?.LineOfSightManager;
                    if (losManager != null && !losManager.HasLineOfSight(_unit.Position, target.Position))
                    {
                        failureReason = "No line of sight to target";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Execute ability effects
        /// </summary>
        private bool ExecuteAbility(AbilityData ability, Unit target)
        {
            switch (ability.rangeType)
            {
                case RangeType.Self:
                    return ApplyAbilityEffects(ability, _unit);

                case RangeType.Aura:
                    return ExecuteAuraAbility(ability);

                case RangeType.Melee:
                case RangeType.Ranged:
                    if (target != null)
                        return ApplyAbilityEffects(ability, target);
                    break;
            }

            return false;
        }

        /// <summary>
        /// Apply ability effects to a single target
        /// </summary>
        private bool ApplyAbilityEffects(AbilityData ability, Unit target)
        {
            // Damage
            if (ability.damage > 0)
            {
                int damageDealt = target.TakeDamage(ability.damage, ability.damageType);
                Debug.Log($"[AbilityController] {ability.abilityName} dealt {damageDealt} damage to {target.GetUnitName()}");
            }

            // Healing
            if (ability.healing > 0)
            {
                int healingDone = target.Heal(ability.healing);
                Debug.Log($"[AbilityController] {ability.abilityName} healed {target.GetUnitName()} for {healingDone} HP");
            }

            // Status effects
            var targetStatusManager = target.GetComponent<StatusEffectManager>();
            if (targetStatusManager != null)
            {
                if (ability.appliesStun)
                {
                    targetStatusManager.ApplyStunned(ability.duration);
                    Debug.Log($"[AbilityController] {target.GetUnitName()} is stunned for {ability.duration} turns");
                }

                if (ability.appliesPoison)
                {
                    targetStatusManager.ApplyPoisoned(ability.duration);
                    Debug.Log($"[AbilityController] {target.GetUnitName()} is poisoned for {ability.duration} turns");
                }
            }

            // Stat modifiers (if duration > 0, these would be temporary)
            if (ability.duration > 0)
            {
                var targetStats = target.GetComponent<UnitStats>();
                if (targetStats != null)
                {
                    if (ability.attackModifier != 0)
                        targetStats.AddAttackModifier(ability.attackModifier);
                    if (ability.defenseModifier != 0)
                        targetStats.AddDefenseModifier(ability.defenseModifier);
                    if (ability.armorModifier != 0)
                        targetStats.AddArmorModifier(ability.armorModifier);
                    if (ability.movementModifier != 0f)
                        targetStats.AddMovementModifier(ability.movementModifier);

                    Debug.Log($"[AbilityController] Applied stat modifiers to {target.GetUnitName()} for {ability.duration} turns");
                }
            }

            // Knockback
            if (ability.appliesKnockback && ability.knockbackDistance > 0f)
            {
                Vector3 knockbackDirection = (target.Position - _unit.Position).normalized;
                Vector3 knockbackPosition = target.Position + (knockbackDirection * ability.knockbackDistance);
                target.MoveTo(knockbackPosition);
                Debug.Log($"[AbilityController] {target.GetUnitName()} knocked back {ability.knockbackDistance}\"");
            }

            // Spawn VFX
            if (ability.vfxPrefab != null)
            {
                GameObject.Instantiate(ability.vfxPrefab, target.Position, Quaternion.identity);
            }

            // Play sound
            if (ability.soundEffect != null)
            {
                // TODO: Integrate with audio system when implemented
                Debug.Log($"[AbilityController] Would play sound: {ability.soundEffect.name}");
            }

            return true;
        }

        /// <summary>
        /// Execute an aura ability (affects all units in AOE)
        /// </summary>
        private bool ExecuteAuraAbility(AbilityData ability)
        {
            int affectedCount = 0;

            // Find all units in range using GetUnitsInRadius
            var battlefield = BattlefieldManager.Instance;
            if (battlefield == null)
                return false;

            var allUnits = battlefield.GetUnitsInRadius(_unit.Position, ability.areaOfEffect);
            foreach (var unit in allUnits)
            {
                if (unit == null || !unit.IsAlive)
                    continue;

                // GetUnitsInRadius already filtered by distance, just check targeting
                bool isAlly = unit.OwnerPlayerIndex == _unit.OwnerPlayerIndex;
                bool isEnemy = !isAlly;

                // Check targeting rules
                if ((isAlly && ability.canTargetAllies) || (isEnemy && ability.canTargetEnemies))
                {
                    ApplyAbilityEffects(ability, unit);
                    affectedCount++;
                }
            }

            Debug.Log($"[AbilityController] {ability.abilityName} affected {affectedCount} units");
            return affectedCount > 0;
        }
        #endregion

        #region Cooldown Management
        /// <summary>
        /// Check if an ability is on cooldown
        /// </summary>
        public bool IsAbilityOnCooldown(AbilityData ability)
        {
            if (!_abilityCooldowns.ContainsKey(ability))
                return false;

            return _abilityCooldowns[ability] > 0;
        }

        /// <summary>
        /// Get remaining cooldown for an ability
        /// </summary>
        public int GetAbilityCooldown(AbilityData ability)
        {
            if (!_abilityCooldowns.ContainsKey(ability))
                return 0;

            return _abilityCooldowns[ability];
        }

        /// <summary>
        /// Reduce all cooldowns by 1
        /// </summary>
        public void TickCooldowns()
        {
            List<AbilityData> keys = new List<AbilityData>(_abilityCooldowns.Keys);
            foreach (var ability in keys)
            {
                if (_abilityCooldowns[ability] > 0)
                {
                    _abilityCooldowns[ability]--;
                }
            }
        }
        #endregion

        #region Action Point Management
        /// <summary>
        /// Reset action points for new turn
        /// </summary>
        public void ResetActionPoints()
        {
            _actionPoints = _maxActionPoints;
        }

        /// <summary>
        /// Set max action points
        /// </summary>
        public void SetMaxActionPoints(int max)
        {
            _maxActionPoints = Mathf.Max(0, max);
            _actionPoints = Mathf.Min(_actionPoints, _maxActionPoints);
        }

        /// <summary>
        /// Add action points
        /// </summary>
        public void AddActionPoints(int amount)
        {
            _actionPoints = Mathf.Min(_actionPoints + amount, _maxActionPoints);
        }

        /// <summary>
        /// Consume action points
        /// </summary>
        public bool ConsumeActionPoints(int amount)
        {
            if (_actionPoints < amount)
                return false;

            _actionPoints -= amount;
            return true;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Called at start of turn
        /// </summary>
        public void OnTurnStart()
        {
            ResetActionPoints();
            TickCooldowns();
        }

        /// <summary>
        /// Called at end of turn
        /// </summary>
        public void OnTurnEnd()
        {
            // Could add per-turn ability effects here
        }
        #endregion
    }
}
