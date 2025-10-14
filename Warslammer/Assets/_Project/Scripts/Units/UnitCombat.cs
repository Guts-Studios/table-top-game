using UnityEngine;
using UnityEngine.Events;
using Warslammer.Combat;
using Warslammer.Core;

namespace Warslammer.Units
{
    /// <summary>
    /// Handles unit combat actions and weapon management
    /// Component for Units - manages attacking and ability usage
    /// </summary>
    [RequireComponent(typeof(Unit))]
    public class UnitCombat : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Fired when unit performs an attack
        /// </summary>
        public UnityEvent<CombatResult> OnAttackPerformed = new UnityEvent<CombatResult>();
        
        /// <summary>
        /// Fired when unit is attacked
        /// </summary>
        public UnityEvent<CombatResult> OnAttackReceived = new UnityEvent<CombatResult>();
        #endregion

        #region Properties
        [Header("Weapons")]
        [SerializeField]
        [Tooltip("Primary weapon for this unit")]
        private WeaponData _primaryWeapon;
        
        /// <summary>
        /// Primary weapon
        /// </summary>
        public WeaponData PrimaryWeapon
        {
            get => _primaryWeapon;
            set => _primaryWeapon = value;
        }

        [SerializeField]
        [Tooltip("Secondary weapon (if any)")]
        private WeaponData _secondaryWeapon;
        
        /// <summary>
        /// Secondary weapon
        /// </summary>
        public WeaponData SecondaryWeapon
        {
            get => _secondaryWeapon;
            set => _secondaryWeapon = value;
        }

        [SerializeField]
        [Tooltip("Currently equipped weapon")]
        private WeaponData _equippedWeapon;
        
        /// <summary>
        /// Currently equipped weapon
        /// </summary>
        public WeaponData EquippedWeapon => _equippedWeapon;

        [Header("Combat State")]
        [SerializeField]
        [Tooltip("Has this unit attacked this turn?")]
        private bool _hasAttackedThisTurn;
        
        /// <summary>
        /// Has attacked this turn?
        /// </summary>
        public bool HasAttackedThisTurn => _hasAttackedThisTurn;

        [SerializeField]
        [Tooltip("Can this unit attack this turn?")]
        private bool _canAttackThisTurn = true;
        
        /// <summary>
        /// Can attack this turn?
        /// </summary>
        public bool CanAttackThisTurn => _canAttackThisTurn && !_hasAttackedThisTurn;

        private Unit _unit;
        private CombatResolver _combatResolver;
        private StatusEffectManager _statusEffectManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _unit = GetComponent<Unit>();
            _combatResolver = CombatResolver.Instance;
            _statusEffectManager = GetComponent<StatusEffectManager>();

            // Default to primary weapon
            if (_primaryWeapon != null)
            {
                _equippedWeapon = _primaryWeapon;
            }
        }
        #endregion

        #region Combat Actions
        /// <summary>
        /// Attack a target unit with equipped weapon
        /// </summary>
        /// <param name="target">Target unit</param>
        /// <returns>Combat result</returns>
        public CombatResult Attack(Unit target)
        {
            return Attack(target, _equippedWeapon);
        }

        /// <summary>
        /// Attack a target unit with a specific weapon
        /// </summary>
        /// <param name="target">Target unit</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>Combat result</returns>
        public CombatResult Attack(Unit target, WeaponData weapon)
        {
            if (!CanAttack(target, weapon))
            {
                Debug.LogWarning($"[UnitCombat] {_unit.GetUnitName()} cannot attack {target?.GetUnitName()}!");
                return null;
            }

            // Resolve attack
            CombatResult result = _combatResolver.ResolveAttack(_unit, target, weapon);

            if (result != null)
            {
                _hasAttackedThisTurn = true;
                OnAttackPerformed?.Invoke(result);

                // Check morale if significant damage was dealt
                if (result.DamageDealt > 0)
                {
                    MoraleSystem morale = MoraleSystem.Instance;
                    if (morale != null)
                    {
                        morale.CheckMoraleAfterDamage(target, result.DamageDealt);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Check if can attack a target
        /// </summary>
        /// <param name="target">Target unit</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>True if can attack</returns>
        public bool CanAttack(Unit target, WeaponData weapon = null)
        {
            // Use equipped weapon if not specified
            if (weapon == null)
                weapon = _equippedWeapon;

            // Basic checks
            if (!CanAttackThisTurn || target == null || weapon == null)
                return false;

            // Check if unit is alive
            if (!_unit.IsAlive || !target.IsAlive)
                return false;

            // Check if stunned
            if (_statusEffectManager != null && _statusEffectManager.IsStunned())
                return false;

            // Check range
            RangeCalculator rangeCalc = RangeCalculator.Instance;
            if (rangeCalc != null && !rangeCalc.IsInRange(_unit, target, weapon))
                return false;

            return true;
        }

        /// <summary>
        /// Get all valid targets for current weapon
        /// </summary>
        /// <param name="potentialTargets">List of potential targets</param>
        /// <returns>List of valid targets</returns>
        public System.Collections.Generic.List<Unit> GetValidTargets(System.Collections.Generic.List<Unit> potentialTargets)
        {
            return GetValidTargets(potentialTargets, _equippedWeapon);
        }

        /// <summary>
        /// Get all valid targets for a specific weapon
        /// </summary>
        /// <param name="potentialTargets">List of potential targets</param>
        /// <param name="weapon">Weapon to check</param>
        /// <returns>List of valid targets</returns>
        public System.Collections.Generic.List<Unit> GetValidTargets(System.Collections.Generic.List<Unit> potentialTargets, WeaponData weapon)
        {
            System.Collections.Generic.List<Unit> validTargets = new System.Collections.Generic.List<Unit>();

            if (potentialTargets == null || weapon == null)
                return validTargets;

            foreach (Unit target in potentialTargets)
            {
                if (CanAttack(target, weapon))
                {
                    validTargets.Add(target);
                }
            }

            return validTargets;
        }
        #endregion

        #region Weapon Management
        /// <summary>
        /// Equip primary weapon
        /// </summary>
        public void EquipPrimaryWeapon()
        {
            if (_primaryWeapon != null)
            {
                _equippedWeapon = _primaryWeapon;
                Debug.Log($"[UnitCombat] {_unit.GetUnitName()} equipped {_primaryWeapon.weaponName}");
            }
        }

        /// <summary>
        /// Equip secondary weapon
        /// </summary>
        public void EquipSecondaryWeapon()
        {
            if (_secondaryWeapon != null)
            {
                _equippedWeapon = _secondaryWeapon;
                Debug.Log($"[UnitCombat] {_unit.GetUnitName()} equipped {_secondaryWeapon.weaponName}");
            }
        }

        /// <summary>
        /// Set primary weapon
        /// </summary>
        public void SetPrimaryWeapon(WeaponData weapon)
        {
            _primaryWeapon = weapon;
            if (_equippedWeapon == null)
            {
                _equippedWeapon = weapon;
            }
        }

        /// <summary>
        /// Set secondary weapon
        /// </summary>
        public void SetSecondaryWeapon(WeaponData weapon)
        {
            _secondaryWeapon = weapon;
        }

        /// <summary>
        /// Check if unit has a secondary weapon
        /// </summary>
        public bool HasSecondaryWeapon()
        {
            return _secondaryWeapon != null;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Reset combat state for new turn
        /// </summary>
        public void OnTurnStart()
        {
            _hasAttackedThisTurn = false;
            _canAttackThisTurn = true;
        }

        /// <summary>
        /// Called at end of turn
        /// </summary>
        public void OnTurnEnd()
        {
            // Nothing to do for now
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get combat range for current weapon
        /// </summary>
        public float GetWeaponRange()
        {
            return _equippedWeapon?.maxRangeInches ?? 0f;
        }

        /// <summary>
        /// Is current weapon melee?
        /// </summary>
        public bool IsWeaponMelee()
        {
            return _equippedWeapon?.rangeType == RangeType.Melee;
        }

        /// <summary>
        /// Is current weapon ranged?
        /// </summary>
        public bool IsWeaponRanged()
        {
            return _equippedWeapon?.rangeType == RangeType.Ranged;
        }

        /// <summary>
        /// Get weapon summary
        /// </summary>
        public string GetWeaponSummary()
        {
            if (_equippedWeapon == null)
                return "No weapon equipped";

            return _equippedWeapon.GetSummary();
        }
        #endregion
    }
}