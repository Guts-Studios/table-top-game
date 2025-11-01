using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;
using Warslammer.Battlefield;

namespace Warslammer.Units
{
    /// <summary>
    /// Commander aura system that provides passive buffs to nearby allied units
    /// </summary>
    public class CommanderAura : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Aura Configuration")]
        [Tooltip("Radius of the aura in inches")]
        [SerializeField]
        private float _auraRadius = 6f;

        [Tooltip("Only affect units of the same faction")]
        [SerializeField]
        private bool _sameFactionsOnly = true;

        [Header("Stat Bonuses")]
        [Tooltip("Attack bonus provided to units in aura")]
        [SerializeField]
        private int _attackBonus = 1;

        [Tooltip("Defense bonus provided to units in aura")]
        [SerializeField]
        private int _defenseBonus = 1;

        [Tooltip("Armor bonus provided to units in aura")]
        [SerializeField]
        private int _armorBonus = 0;

        [Tooltip("Movement bonus provided to units in aura (in inches)")]
        [SerializeField]
        private float _movementBonus = 0f;

        [Header("Special Effects")]
        [Tooltip("Units in aura can reroll failed morale checks")]
        [SerializeField]
        private bool _providesRerollMorale = true;

        [Tooltip("Units in aura ignore first point of damage")]
        [SerializeField]
        private bool _providesIgnoreFirstDamage = false;

        [Tooltip("Units in aura gain +1 action point")]
        [SerializeField]
        private bool _providesExtraAction = false;

        [Header("Visual Feedback")]
        [Tooltip("Color of the aura visualization")]
        [SerializeField]
        private Color _auraColor = new Color(1f, 0.8f, 0f, 0.2f); // Gold, semi-transparent

        [Tooltip("Show aura visualization in play mode")]
        [SerializeField]
        private bool _showAuraVisualization = true;

        [Header("Update Settings")]
        [Tooltip("How often to update affected units (in seconds)")]
        [SerializeField]
        private float _updateInterval = 0.5f;
        #endregion

        #region Properties
        /// <summary>
        /// Aura radius in inches
        /// </summary>
        public float AuraRadius => _auraRadius;

        /// <summary>
        /// Attack bonus provided by aura
        /// </summary>
        public int AttackBonus => _attackBonus;

        /// <summary>
        /// Defense bonus provided by aura
        /// </summary>
        public int DefenseBonus => _defenseBonus;

        /// <summary>
        /// Armor bonus provided by aura
        /// </summary>
        public int ArmorBonus => _armorBonus;

        /// <summary>
        /// Movement bonus provided by aura
        /// </summary>
        public float MovementBonus => _movementBonus;

        /// <summary>
        /// Number of units currently in aura
        /// </summary>
        public int UnitsInAura => _unitsInAura.Count;
        #endregion

        #region Private Fields
        private Unit _commanderUnit;
        private HashSet<Unit> _unitsInAura = new HashSet<Unit>();
        private HashSet<Unit> _previousUnitsInAura = new HashSet<Unit>();
        private float _updateTimer = 0f;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _commanderUnit = GetComponent<Unit>();
        }

        private void Start()
        {
            UpdateAffectedUnits();
        }

        private void Update()
        {
            // Update affected units periodically
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                UpdateAffectedUnits();
            }
        }

        private void OnDrawGizmos()
        {
            if (_showAuraVisualization && Application.isPlaying)
            {
                // Draw aura radius
                Gizmos.color = _auraColor;
                DrawCircle(transform.position, _auraRadius, 32);

                // Draw lines to affected units
                Gizmos.color = new Color(_auraColor.r, _auraColor.g, _auraColor.b, 0.5f);
                foreach (var unit in _unitsInAura)
                {
                    if (unit != null)
                    {
                        Gizmos.DrawLine(transform.position, unit.Position);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Always show aura when selected
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            DrawCircle(transform.position, _auraRadius, 32);
        }
        #endregion

        #region Aura Management
        /// <summary>
        /// Update which units are affected by the aura
        /// </summary>
        private void UpdateAffectedUnits()
        {
            // Skip if commander is dead
            if (_commanderUnit == null || !_commanderUnit.IsAlive)
            {
                RemoveAllBuffs();
                return;
            }

            // Store previous units
            _previousUnitsInAura.Clear();
            _previousUnitsInAura.UnionWith(_unitsInAura);

            // Clear current set
            _unitsInAura.Clear();

            // Find all units in range using GetUnitsInRadius
            var battlefield = BattlefieldManager.Instance;
            if (battlefield == null)
                return;

            var unitsInRange = battlefield.GetUnitsInRadius(_commanderUnit.Position, _auraRadius);
            foreach (var unit in unitsInRange)
            {
                if (unit == null || !unit.IsAlive)
                    continue;

                // Skip self
                if (unit == _commanderUnit)
                    continue;

                // Check faction
                if (_sameFactionsOnly && unit.OwnerPlayerIndex != _commanderUnit.OwnerPlayerIndex)
                    continue;

                // Unit is in range and passes all checks
                _unitsInAura.Add(unit);

                // If this is a new unit entering aura, apply buffs
                if (!_previousUnitsInAura.Contains(unit))
                {
                    ApplyAuraBuffs(unit);
                }
            }

            // Remove buffs from units that left the aura
            foreach (var unit in _previousUnitsInAura)
            {
                if (!_unitsInAura.Contains(unit))
                {
                    RemoveAuraBuffs(unit);
                }
            }
        }

        /// <summary>
        /// Apply aura buffs to a unit
        /// </summary>
        private void ApplyAuraBuffs(Unit unit)
        {
            var stats = unit.GetComponent<UnitStats>();
            if (stats == null)
                return;

            // Apply stat bonuses
            if (_attackBonus != 0)
                stats.AddAttackModifier(_attackBonus);

            if (_defenseBonus != 0)
                stats.AddDefenseModifier(_defenseBonus);

            if (_armorBonus != 0)
                stats.AddArmorModifier(_armorBonus);

            if (_movementBonus != 0f)
                stats.AddMovementModifier(_movementBonus);

            // Apply special effects
            var abilityController = unit.GetComponent<AbilityController>();
            if (abilityController != null && _providesExtraAction)
            {
                abilityController.AddActionPoints(1);
            }

            Debug.Log($"[CommanderAura] {unit.GetUnitName()} entered {_commanderUnit.GetUnitName()}'s aura" +
                     $" (ATK+{_attackBonus}, DEF+{_defenseBonus}, ARM+{_armorBonus}, MOV+{_movementBonus})");
        }

        /// <summary>
        /// Remove aura buffs from a unit
        /// </summary>
        private void RemoveAuraBuffs(Unit unit)
        {
            if (unit == null)
                return;

            var stats = unit.GetComponent<UnitStats>();
            if (stats == null)
                return;

            // Remove stat bonuses (apply negative of bonuses)
            if (_attackBonus != 0)
                stats.AddAttackModifier(-_attackBonus);

            if (_defenseBonus != 0)
                stats.AddDefenseModifier(-_defenseBonus);

            if (_armorBonus != 0)
                stats.AddArmorModifier(-_armorBonus);

            if (_movementBonus != 0f)
                stats.AddMovementModifier(-_movementBonus);

            Debug.Log($"[CommanderAura] {unit.GetUnitName()} left {_commanderUnit.GetUnitName()}'s aura");
        }

        /// <summary>
        /// Remove all buffs (when commander dies or is disabled)
        /// </summary>
        private void RemoveAllBuffs()
        {
            foreach (var unit in _unitsInAura)
            {
                RemoveAuraBuffs(unit);
            }

            _unitsInAura.Clear();
        }
        #endregion

        #region Special Effects
        /// <summary>
        /// Check if a unit is in the aura
        /// </summary>
        public bool IsUnitInAura(Unit unit)
        {
            return _unitsInAura.Contains(unit);
        }

        /// <summary>
        /// Check if unit can reroll morale
        /// </summary>
        public bool CanRerollMorale(Unit unit)
        {
            return _providesRerollMorale && IsUnitInAura(unit);
        }

        /// <summary>
        /// Check if unit has damage ignore effect
        /// </summary>
        public bool HasIgnoreFirstDamage(Unit unit)
        {
            return _providesIgnoreFirstDamage && IsUnitInAura(unit);
        }

        /// <summary>
        /// Get all units currently in aura
        /// </summary>
        public List<Unit> GetUnitsInAura()
        {
            return new List<Unit>(_unitsInAura);
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set aura radius
        /// </summary>
        public void SetAuraRadius(float radius)
        {
            _auraRadius = Mathf.Max(0f, radius);
            UpdateAffectedUnits();
        }

        /// <summary>
        /// Set attack bonus
        /// </summary>
        public void SetAttackBonus(int bonus)
        {
            // Remove old buffs
            RemoveAllBuffs();

            // Update bonus
            _attackBonus = bonus;

            // Reapply buffs
            UpdateAffectedUnits();
        }

        /// <summary>
        /// Set defense bonus
        /// </summary>
        public void SetDefenseBonus(int bonus)
        {
            RemoveAllBuffs();
            _defenseBonus = bonus;
            UpdateAffectedUnits();
        }

        /// <summary>
        /// Set armor bonus
        /// </summary>
        public void SetArmorBonus(int bonus)
        {
            RemoveAllBuffs();
            _armorBonus = bonus;
            UpdateAffectedUnits();
        }

        /// <summary>
        /// Set movement bonus
        /// </summary>
        public void SetMovementBonus(float bonus)
        {
            RemoveAllBuffs();
            _movementBonus = bonus;
            UpdateAffectedUnits();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Draw a circle gizmo
        /// </summary>
        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private void OnDisable()
        {
            // Remove all buffs when disabled
            RemoveAllBuffs();
        }

        private void OnDestroy()
        {
            // Remove all buffs when destroyed
            RemoveAllBuffs();
        }
        #endregion
    }
}
