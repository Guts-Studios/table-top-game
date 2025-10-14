using UnityEngine;
using UnityEngine.Events;
using Warslammer.Core;
using Warslammer.Data;
using Warslammer.Utilities;

namespace Warslammer.Units
{
    /// <summary>
    /// Main MonoBehaviour for units
    /// Implements all core interfaces (IDamageable, ISelectable, IMovable, ICombatant)
    /// Central coordination point for all unit systems
    /// </summary>
    [RequireComponent(typeof(UnitStats))]
    [RequireComponent(typeof(UnitMovement))]
    [RequireComponent(typeof(UnitVisuals))]
    public class Unit : MonoBehaviour, IDamageable, ISelectable, IMovable, ICombatant
    {
        #region Events
        /// <summary>
        /// Fired when unit is selected
        /// </summary>
        public UnityEvent OnUnitSelected = new UnityEvent();
        
        /// <summary>
        /// Fired when unit is deselected
        /// </summary>
        public UnityEvent OnUnitDeselected = new UnityEvent();
        
        /// <summary>
        /// Fired when unit takes damage
        /// </summary>
        public UnityEvent<int> OnUnitDamaged = new UnityEvent<int>();
        
        /// <summary>
        /// Fired when unit dies
        /// </summary>
        public UnityEvent OnUnitDeath = new UnityEvent();
        
        /// <summary>
        /// Fired when unit attacks
        /// </summary>
        public UnityEvent<ICombatant> OnUnitAttack = new UnityEvent<ICombatant>();
        #endregion

        #region Properties
        [Header("Unit Configuration")]
        [SerializeField]
        [Tooltip("Unit data ScriptableObject")]
        private UnitData _unitData;
        
        /// <summary>
        /// Unit data for this unit
        /// </summary>
        public UnitData UnitData => _unitData;

        [SerializeField]
        [Tooltip("Player index who owns this unit")]
        private int _ownerPlayerIndex = 0;
        
        /// <summary>
        /// Player index who owns this unit
        /// </summary>
        public int OwnerPlayerIndex
        {
            get => _ownerPlayerIndex;
            set => _ownerPlayerIndex = value;
        }

        [Header("Base Configuration")]
        [SerializeField]
        [Tooltip("Collider for unit base (used for selection and collision)")]
        private Collider _baseCollider;

        // Component references
        private UnitStats _stats;
        private UnitMovement _movement;
        private UnitVisuals _visuals;

        // State
        private bool _isSelected;
        private bool _hasAttackedThisTurn;

        /// <summary>
        /// Radius of unit's base
        /// </summary>
        private float _baseRadius;
        #endregion

        #region ISelectable Implementation
        /// <summary>
        /// Is this unit currently selected?
        /// </summary>
        public bool IsSelected => _isSelected;

        /// <summary>
        /// Can this unit be selected?
        /// </summary>
        public bool CanBeSelected => true; // TODO: Add conditions

        /// <summary>
        /// Transform of the unit
        /// </summary>
        public Transform Transform => transform;

        /// <summary>
        /// Called when unit is selected
        /// </summary>
        public void OnSelected()
        {
            _isSelected = true;
            _visuals?.ShowSelection();
            OnUnitSelected?.Invoke();
            Debug.Log($"[Unit] {name} selected");
        }

        /// <summary>
        /// Called when unit is deselected
        /// </summary>
        public void OnDeselected()
        {
            _isSelected = false;
            _visuals?.HideSelection();
            OnUnitDeselected?.Invoke();
            Debug.Log($"[Unit] {name} deselected");
        }

        /// <summary>
        /// Called when unit is hovered over
        /// </summary>
        public void OnHoverEnter()
        {
            _visuals?.ShowHover();
        }

        /// <summary>
        /// Called when hover leaves unit
        /// </summary>
        public void OnHoverExit()
        {
            _visuals?.HideHover();
        }
        #endregion

        #region IDamageable Implementation
        /// <summary>
        /// Current health points
        /// </summary>
        public int CurrentHealth => _stats?.CurrentHealth ?? 0;

        /// <summary>
        /// Maximum health points
        /// </summary>
        public int MaxHealth => _stats?.MaxHealth ?? 1;

        /// <summary>
        /// Is this unit alive?
        /// </summary>
        public bool IsAlive => _stats != null && _stats.IsAlive;

        /// <summary>
        /// Health percentage (0.0 to 1.0)
        /// </summary>
        public float HealthPercentage => _stats?.HealthPercentage ?? 0f;

        /// <summary>
        /// Take damage
        /// </summary>
        public int TakeDamage(int damage, DamageSource damageSource)
        {
            if (_stats == null)
                return 0;

            int actualDamage = _stats.TakeDamage(damage, damageSource);

            // Visual feedback
            _visuals?.FlashDamage();
            _visuals?.Shake();

            OnUnitDamaged?.Invoke(actualDamage);

            // Check for death
            if (!IsAlive)
            {
                OnDeath();
            }

            return actualDamage;
        }

        /// <summary>
        /// Heal unit
        /// </summary>
        public int Heal(int healAmount)
        {
            return _stats?.Heal(healAmount) ?? 0;
        }

        /// <summary>
        /// Called when unit dies
        /// </summary>
        public void OnDeath()
        {
            Debug.Log($"[Unit] {name} has died!");
            OnUnitDeath?.Invoke();
            
            // TODO: Phase 3 - Death animation, loot drops, etc.
            // For now, just destroy the unit
            Destroy(gameObject, 1f);
        }
        #endregion

        #region IMovable Implementation
        /// <summary>
        /// Current position
        /// </summary>
        public Vector3 Position => transform.position;

        /// <summary>
        /// Movement speed in inches
        /// </summary>
        public float MovementSpeed => _stats?.MovementSpeed ?? 0f;

        /// <summary>
        /// Remaining movement for this turn
        /// </summary>
        public float RemainingMovement => _stats?.RemainingMovement ?? 0f;

        /// <summary>
        /// Can this unit currently move?
        /// </summary>
        public bool CanMove
        {
            get
            {
                if (_stats == null || _movement == null)
                    return false;

                // Can't move if dead, stunned, or already moving
                if (!IsAlive || _stats.IsStunned || _movement.IsMoving)
                    return false;

                // Can't move if no movement remaining
                if (RemainingMovement <= 0f)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Has this unit moved this turn?
        /// </summary>
        public bool HasMoved => RemainingMovement < MovementSpeed;

        /// <summary>
        /// Move to target position
        /// </summary>
        public void MoveTo(Vector3 targetPosition)
        {
            _movement?.MoveTo(targetPosition);
        }

        /// <summary>
        /// Check if position is reachable
        /// </summary>
        public bool IsPositionReachable(Vector3 position)
        {
            float distance = MeasurementUtility.GetDistanceInches(Position, position);
            return distance <= RemainingMovement;
        }

        /// <summary>
        /// Get movement cost to reach position
        /// </summary>
        public float GetMovementCost(Vector3 position)
        {
            return MeasurementUtility.GetDistanceInches(Position, position);
        }

        /// <summary>
        /// Reset movement for new turn
        /// </summary>
        public void ResetMovement()
        {
            _stats?.ResetMovement();
        }

        /// <summary>
        /// Consume movement
        /// </summary>
        public bool ConsumeMovement(float movementCost)
        {
            return _stats?.ConsumeMovement(movementCost) ?? false;
        }
        #endregion

        #region ICombatant Implementation
        /// <summary>
        /// Current attack value
        /// </summary>
        public int Attack => _stats?.Attack ?? 0;

        /// <summary>
        /// Current defense value
        /// </summary>
        public int Defense => _stats?.Defense ?? 0;

        /// <summary>
        /// Current armor value
        /// </summary>
        public int Armor => _stats?.Armor ?? 0;

        /// <summary>
        /// Can this unit attack this turn?
        /// </summary>
        public bool CanAttack
        {
            get
            {
                if (!IsAlive)
                    return false;

                // TODO: Phase 3 - Check action phase, stunned status, etc.
                return !_hasAttackedThisTurn;
            }
        }

        /// <summary>
        /// Has this unit attacked this turn?
        /// </summary>
        public bool HasAttacked => _hasAttackedThisTurn;

        /// <summary>
        /// Is this unit engaged in melee?
        /// </summary>
        public bool IsEngaged
        {
            get
            {
                // TODO: Phase 3 - Check for nearby enemy units
                return false;
            }
        }

        /// <summary>
        /// Attack a target
        /// </summary>
        public bool Attack(ICombatant target)
        {
            if (!CanAttack || target == null)
                return false;

            // TODO: Phase 3 - Implement full combat system
            Debug.Log($"[Unit] {name} attacks {target}!");
            
            _hasAttackedThisTurn = true;
            OnUnitAttack?.Invoke(target);

            return true;
        }

        /// <summary>
        /// Check if can attack target
        /// </summary>
        public bool CanAttackTarget(ICombatant target)
        {
            if (!CanAttack || target == null)
                return false;

            // TODO: Phase 3 - Check range, line of sight, etc.
            return true;
        }

        /// <summary>
        /// Get distance to target in inches
        /// </summary>
        public float GetDistanceTo(ICombatant target)
        {
            if (target?.UnitData == null)
                return float.MaxValue;

            // Assuming target is a Unit
            if (target is Unit targetUnit)
            {
                return MeasurementUtility.GetDistanceInches(Position, targetUnit.Position);
            }

            return float.MaxValue;
        }

        /// <summary>
        /// Reset combat state for new turn
        /// </summary>
        public void ResetCombat()
        {
            _hasAttackedThisTurn = false;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Get component references
            _stats = GetComponent<UnitStats>();
            _movement = GetComponent<UnitMovement>();
            _visuals = GetComponent<UnitVisuals>();

            // Get or create base collider
            if (_baseCollider == null)
            {
                _baseCollider = GetComponent<Collider>();
                if (_baseCollider == null)
                {
                    // Create a default collider
                    CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
                    capsule.radius = 0.5f;
                    capsule.height = 1f;
                    _baseCollider = capsule;
                }
            }
        }

        private void Start()
        {
            // Initialize if we have unit data
            if (_unitData != null)
            {
                Initialize(_unitData);
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize unit from unit data
        /// </summary>
        public void Initialize(UnitData unitData)
        {
            if (unitData == null)
            {
                Debug.LogError($"[Unit] Cannot initialize {name} with null unit data!");
                return;
            }

            _unitData = unitData;

            // Initialize stats
            _stats?.Initialize(unitData);

            // Initialize visuals
            _visuals?.Initialize(unitData);

            // Calculate base radius
            _baseRadius = MeasurementUtility.GetBaseRadius(unitData.baseSize);

            // Update collider size
            UpdateColliderSize();

            Debug.Log($"[Unit] Initialized {unitData.unitName}");
        }

        /// <summary>
        /// Update collider to match base size
        /// </summary>
        private void UpdateColliderSize()
        {
            if (_baseCollider == null)
                return;

            if (_baseCollider is SphereCollider sphere)
            {
                sphere.radius = _baseRadius;
            }
            else if (_baseCollider is CapsuleCollider capsule)
            {
                capsule.radius = _baseRadius;
            }
            else if (_baseCollider is BoxCollider box)
            {
                box.size = new Vector3(_baseRadius * 2f, box.size.y, _baseRadius * 2f);
            }
        }
        #endregion

        #region Base Management
        /// <summary>
        /// Get the radius of this unit's base
        /// </summary>
        public float GetBaseRadius()
        {
            return _baseRadius;
        }

        /// <summary>
        /// Get the base collider
        /// </summary>
        public Collider GetBaseCollider()
        {
            return _baseCollider;
        }
        #endregion

        #region Turn Management
        /// <summary>
        /// Called at start of turn
        /// </summary>
        public void OnTurnStart()
        {
            _stats?.OnTurnStart();
            ResetMovement();
            ResetCombat();

            Debug.Log($"[Unit] {name} turn started");
        }

        /// <summary>
        /// Called at end of turn
        /// </summary>
        public void OnTurnEnd()
        {
            _stats?.OnTurnEnd();

            Debug.Log($"[Unit] {name} turn ended");
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get unit name
        /// </summary>
        public string GetUnitName()
        {
            return _unitData != null ? _unitData.unitName : name;
        }

        /// <summary>
        /// Get unit type
        /// </summary>
        public UnitType GetUnitType()
        {
            return _unitData != null ? _unitData.unitType : UnitType.Infantry;
        }
        #endregion
    }
}