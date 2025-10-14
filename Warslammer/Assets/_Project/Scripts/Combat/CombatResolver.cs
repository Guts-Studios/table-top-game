using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Warslammer.Battlefield;
using Warslammer.Core;
using Warslammer.Units;

namespace Warslammer.Combat
{
    /// <summary>
    /// Main combat flow coordinator
    /// Handles: range check → LOS → dice rolls → damage resolution
    /// </summary>
    public class CombatResolver : MonoBehaviour
    {
        #region Singleton
        private static CombatResolver _instance;
        
        /// <summary>
        /// Global access point for the CombatResolver
        /// </summary>
        public static CombatResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CombatResolver>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CombatResolver");
                        _instance = go.AddComponent<CombatResolver>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when combat is initiated
        /// </summary>
        public UnityEvent<CombatResult> OnCombatStarted = new UnityEvent<CombatResult>();
        
        /// <summary>
        /// Fired when combat is resolved
        /// </summary>
        public UnityEvent<CombatResult> OnCombatResolved = new UnityEvent<CombatResult>();
        
        /// <summary>
        /// Fired when a unit is killed
        /// </summary>
        public UnityEvent<Unit> OnUnitKilled = new UnityEvent<Unit>();
        #endregion

        #region Properties
        [Header("References")]
        [SerializeField]
        [Tooltip("Dice roller for combat")]
        private DiceRoller _diceRoller;
        
        [SerializeField]
        [Tooltip("Damage calculator")]
        private DamageCalculator _damageCalculator;
        
        [SerializeField]
        [Tooltip("Range calculator")]
        private RangeCalculator _rangeCalculator;

        [Header("Combat Settings")]
        [SerializeField]
        [Tooltip("Minimum damage on successful hit")]
        private int _minimumDamage = 1;
        
        [SerializeField]
        [Tooltip("Apply flanking bonuses")]
        private bool _enableFlankingBonuses = true;
        
        [SerializeField]
        [Tooltip("Apply cover penalties")]
        private bool _enableCoverSystem = true;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;

            // Get or create required components
            if (_diceRoller == null)
                _diceRoller = DiceRoller.Instance;
            
            if (_damageCalculator == null)
                _damageCalculator = DamageCalculator.Instance;
            
            if (_rangeCalculator == null)
                _rangeCalculator = RangeCalculator.Instance;
        }
        #endregion

        #region Combat Resolution
        /// <summary>
        /// Resolve a complete attack from attacker to defender
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="defender">Defending unit</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>Combat result</returns>
        public CombatResult ResolveAttack(Unit attacker, Unit defender, WeaponData weapon)
        {
            if (attacker == null || defender == null || weapon == null)
            {
                Debug.LogError("[CombatResolver] Cannot resolve attack with null parameters!");
                return null;
            }

            Debug.Log($"[CombatResolver] Resolving attack: {attacker.GetUnitName()} -> {defender.GetUnitName()} with {weapon.weaponName}");

            // Create combat result
            CombatResult result = new CombatResult(attacker, defender, weapon);

            // 1. Check range
            float distance = _rangeCalculator.GetDistanceInches(attacker, defender);
            bool inRange = weapon.IsInRange(distance);
            result.SetAttackDistance(distance, weapon.rangeType == RangeType.Melee);

            if (!inRange)
            {
                Debug.LogWarning($"[CombatResolver] Target out of range! Distance: {distance}\", Max: {weapon.maxRangeInches}\"");
                OnCombatResolved?.Invoke(result);
                return result;
            }

            // 2. Check line of sight (for ranged attacks)
            if (weapon.rangeType == RangeType.Ranged)
            {
                LineOfSightManager losManager = BattlefieldManager.Instance?.LineOfSightManager;
                if (losManager != null && !losManager.HasLineOfSight(attacker, defender))
                {
                    Debug.LogWarning("[CombatResolver] No line of sight to target!");
                    OnCombatResolved?.Invoke(result);
                    return result;
                }
            }

            // 3. Build attacker dice pool
            ModifierStack attackerModifiers = BuildAttackerModifiers(attacker, defender, weapon, distance);
            DicePool attackPool = weapon.CreateAttackPool();
            attackerModifiers.ApplyToPool(attackPool);

            // 4. Build defender dice pool
            ModifierStack defenderModifiers = BuildDefenderModifiers(attacker, defender, weapon);
            DicePool defensePool = new DicePool(defender.Defense, 4, 0);
            defenderModifiers.ApplyToPool(defensePool);

            // 5. Roll attacker dice
            DiceRollResult attackRoll = _diceRoller.Roll(attackPool);
            result.SetAttackRoll(attackRoll);

            // 6. Roll defender dice (if attack has successes and not unblockable)
            DiceRollResult defenseRoll = null;
            if (attackRoll.Successes > 0 && !weapon.unblockable)
            {
                defenseRoll = _diceRoller.Roll(defensePool);
                result.SetDefenseRoll(defenseRoll);
            }
            else
            {
                defenseRoll = new DiceRollResult(4, 0); // Empty defense
                result.SetDefenseRoll(defenseRoll);
            }

            // 7. Calculate damage
            int defenseSuccesses = defenseRoll?.Successes ?? 0;
            int damageModifiers = attackerModifiers.TotalResultModifier;
            
            // Add bonus damage vs unit type
            damageModifiers += weapon.GetBonusDamageVsType(defender.GetUnitType());

            int finalDamage = _damageCalculator.CalculateDamage(
                weapon.baseDamage,
                attackRoll.Successes,
                defenseSuccesses,
                defender.Armor,
                damageModifiers
            );

            // Apply minimum damage if hit occurred
            if (attackRoll.Successes > defenseSuccesses && finalDamage < _minimumDamage)
            {
                finalDamage = _minimumDamage;
            }

            result.SetDamage(finalDamage);
            result.SetDamageSource(weapon.damageSource);
            result.SetModifierSummary(GetModifierSummary(attackerModifiers, defenderModifiers));

            // 8. Apply damage to defender
            if (finalDamage > 0)
            {
                int actualDamage = defender.TakeDamage(finalDamage, weapon.damageSource);
                result.SetDamage(actualDamage);

                // Check if defender died
                if (!defender.IsAlive)
                {
                    result.SetKilled(true);
                    OnUnitKilled?.Invoke(defender);
                }
            }

            // 9. Apply status effects
            ApplyStatusEffects(weapon, defender, attackRoll);

            // 10. Log and notify
            Debug.Log($"[CombatResolver] {result.GetSummary()}");
            OnCombatResolved?.Invoke(result);

            return result;
        }
        #endregion

        #region Modifiers
        /// <summary>
        /// Build attacker modifiers from abilities, terrain, etc.
        /// </summary>
        private ModifierStack BuildAttackerModifiers(Unit attacker, Unit defender, WeaponData weapon, float distance)
        {
            ModifierStack modifiers = new ModifierStack();

            // Flanking bonus (if enabled)
            if (_enableFlankingBonuses && IsFlanking(attacker, defender))
            {
                modifiers.AddDiceCountModifier("Flanking", 1);
                Debug.Log("[CombatResolver] Flanking bonus applied!");
            }

            // High ground bonus (TODO: Phase 3 - implement terrain height)
            // if (HasHighGround(attacker, defender))
            // {
            //     modifiers.AddResultModifier("High Ground", 1);
            // }

            // Long range penalty
            if (weapon.rangeType == RangeType.Ranged && _rangeCalculator.IsLongRange(distance, weapon))
            {
                modifiers.AddDiceModifier("Long Range", -1);
            }

            // TODO: Phase 4 - Add ability modifiers

            return modifiers;
        }

        /// <summary>
        /// Build defender modifiers (cover, terrain, abilities)
        /// </summary>
        private ModifierStack BuildDefenderModifiers(Unit attacker, Unit defender, WeaponData weapon)
        {
            ModifierStack modifiers = new ModifierStack();

            // Cover bonus (for ranged attacks)
            if (_enableCoverSystem && weapon.rangeType == RangeType.Ranged && !weapon.ignoresCover)
            {
                // TODO: Phase 3 - implement cover system
                // if (HasCover(defender, attacker.Position))
                // {
                //     modifiers.AddDiceCountModifier("Cover", 1);
                // }
            }

            // TODO: Phase 4 - Add defensive ability modifiers

            return modifiers;
        }

        /// <summary>
        /// Get summary of all modifiers applied
        /// </summary>
        private string GetModifierSummary(ModifierStack attackerMods, ModifierStack defenderMods)
        {
            string summary = "";
            
            if (attackerMods.HasModifiers())
            {
                summary += "Attacker: " + attackerMods.GetSummary();
            }
            
            if (defenderMods.HasModifiers())
            {
                if (summary.Length > 0) summary += " | ";
                summary += "Defender: " + defenderMods.GetSummary();
            }

            return summary;
        }
        #endregion

        #region Tactical Calculations
        /// <summary>
        /// Check if attacker is flanking defender
        /// </summary>
        private bool IsFlanking(Unit attacker, Unit defender)
        {
            // Simple flanking: check if attacker is behind defender
            // TODO: Phase 3 - improve flanking detection with facing
            
            Vector3 defenderForward = defender.transform.forward;
            Vector3 toAttacker = (attacker.Position - defender.Position).normalized;
            
            float dot = Vector3.Dot(defenderForward, toAttacker);
            
            // If dot < 0, attacker is behind defender
            return dot < -0.5f;
        }
        #endregion

        #region Status Effects
        /// <summary>
        /// Apply status effects from weapon hits
        /// </summary>
        private void ApplyStatusEffects(WeaponData weapon, Unit target, DiceRollResult attackRoll)
        {
            if (attackRoll.Successes <= 0)
                return;

            // Apply bleed
            if (weapon.canInflictBleed)
            {
                float roll = Random.value;
                if (roll <= weapon.bleedChance)
                {
                    // TODO: Phase 3 - implement status effect system
                    Debug.Log($"[CombatResolver] {target.GetUnitName()} is bleeding!");
                }
            }

            // Apply rooted
            if (weapon.canInflictRooted)
            {
                float roll = Random.value;
                if (roll <= weapon.rootedChance)
                {
                    // TODO: Phase 3 - implement status effect system
                    Debug.Log($"[CombatResolver] {target.GetUnitName()} is rooted!");
                }
            }
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set minimum damage value
        /// </summary>
        public void SetMinimumDamage(int minimum)
        {
            _minimumDamage = Mathf.Max(0, minimum);
        }

        /// <summary>
        /// Enable/disable flanking bonuses
        /// </summary>
        public void SetFlankingBonuses(bool enabled)
        {
            _enableFlankingBonuses = enabled;
        }

        /// <summary>
        /// Enable/disable cover system
        /// </summary>
        public void SetCoverSystem(bool enabled)
        {
            _enableCoverSystem = enabled;
        }
        #endregion
    }
}