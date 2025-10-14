using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Warslammer.Core;
using Warslammer.Units;
using Warslammer.Utilities;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Manages all units on the battlefield, spawn/despawn, position validation
    /// Central authority for battlefield state
    /// </summary>
    public class BattlefieldManager : MonoBehaviour
    {
        #region Singleton
        private static BattlefieldManager _instance;
        
        /// <summary>
        /// Global access point for the BattlefieldManager
        /// </summary>
        public static BattlefieldManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattlefieldManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Fired when a unit is spawned on the battlefield
        /// </summary>
        public UnityEvent<Unit> OnUnitSpawned = new UnityEvent<Unit>();
        
        /// <summary>
        /// Fired when a unit is removed from the battlefield
        /// </summary>
        public UnityEvent<Unit> OnUnitDespawned = new UnityEvent<Unit>();
        
        /// <summary>
        /// Fired when a unit moves
        /// </summary>
        public UnityEvent<Unit, Vector3, Vector3> OnUnitMoved = new UnityEvent<Unit, Vector3, Vector3>();
        #endregion

        #region Properties
        [Header("Battlefield Configuration")]
        [SerializeField]
        [Tooltip("Battlefield width in inches")]
        private float _battlefieldWidthInches = 48f;
        
        /// <summary>
        /// Battlefield width in inches
        /// </summary>
        public float BattlefieldWidthInches => _battlefieldWidthInches;

        [SerializeField]
        [Tooltip("Battlefield depth in inches")]
        private float _battlefieldDepthInches = 48f;
        
        /// <summary>
        /// Battlefield depth in inches
        /// </summary>
        public float BattlefieldDepthInches => _battlefieldDepthInches;

        [SerializeField]
        [Tooltip("Center point of the battlefield")]
        private Vector3 _battlefieldCenter = Vector3.zero;
        
        /// <summary>
        /// Center point of the battlefield
        /// </summary>
        public Vector3 BattlefieldCenter => _battlefieldCenter;

        /// <summary>
        /// Minimum bounds of the battlefield in Unity units
        /// </summary>
        public Vector3 MinBounds => _battlefieldCenter - new Vector3(
            MeasurementUtility.InchesToUnityUnits(_battlefieldWidthInches / 2f),
            0,
            MeasurementUtility.InchesToUnityUnits(_battlefieldDepthInches / 2f)
        );

        /// <summary>
        /// Maximum bounds of the battlefield in Unity units
        /// </summary>
        public Vector3 MaxBounds => _battlefieldCenter + new Vector3(
            MeasurementUtility.InchesToUnityUnits(_battlefieldWidthInches / 2f),
            0,
            MeasurementUtility.InchesToUnityUnits(_battlefieldDepthInches / 2f)
        );

        [Header("Units")]
        [SerializeField]
        [Tooltip("List of all units currently on the battlefield")]
        private List<Unit> _activeUnits = new List<Unit>();
        
        /// <summary>
        /// List of all units currently on the battlefield
        /// </summary>
        public List<Unit> ActiveUnits => _activeUnits;

        [Header("References")]
        [SerializeField]
        [Tooltip("Reference to the terrain manager")]
        private TerrainManager _terrainManager;
        
        /// <summary>
        /// Reference to the terrain manager
        /// </summary>
        public TerrainManager TerrainManager => _terrainManager;

        [SerializeField]
        [Tooltip("Reference to the deployment zone manager")]
        private DeploymentZoneManager _deploymentZoneManager;
        
        /// <summary>
        /// Reference to the deployment zone manager
        /// </summary>
        public DeploymentZoneManager DeploymentZoneManager => _deploymentZoneManager;

        [SerializeField]
        [Tooltip("Reference to the line of sight manager")]
        private LineOfSightManager _lineOfSightManager;
        
        /// <summary>
        /// Reference to the line of sight manager
        /// </summary>
        public LineOfSightManager LineOfSightManager => _lineOfSightManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            
            _instance = this;

            // Find or create manager components
            if (_terrainManager == null)
                _terrainManager = GetComponent<TerrainManager>();
            
            if (_deploymentZoneManager == null)
                _deploymentZoneManager = GetComponent<DeploymentZoneManager>();
            
            if (_lineOfSightManager == null)
                _lineOfSightManager = GetComponent<LineOfSightManager>();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnDrawGizmos()
        {
            // Draw battlefield bounds
            Gizmos.color = Color.yellow;
            Vector3 min = MinBounds;
            Vector3 max = MaxBounds;
            
            // Draw ground rectangle
            Gizmos.DrawLine(new Vector3(min.x, 0, min.z), new Vector3(max.x, 0, min.z));
            Gizmos.DrawLine(new Vector3(max.x, 0, min.z), new Vector3(max.x, 0, max.z));
            Gizmos.DrawLine(new Vector3(max.x, 0, max.z), new Vector3(min.x, 0, max.z));
            Gizmos.DrawLine(new Vector3(min.x, 0, max.z), new Vector3(min.x, 0, min.z));
        }
        #endregion

        #region Unit Management
        /// <summary>
        /// Spawn a unit on the battlefield
        /// </summary>
        /// <param name="unit">Unit to spawn</param>
        /// <param name="position">Position to spawn at</param>
        /// <returns>True if spawn was successful</returns>
        public bool SpawnUnit(Unit unit, Vector3 position)
        {
            if (unit == null)
            {
                Debug.LogError("[BattlefieldManager] Cannot spawn null unit");
                return false;
            }

            // Validate position
            if (!IsPositionValid(position, unit.GetBaseRadius()))
            {
                Debug.LogWarning($"[BattlefieldManager] Cannot spawn {unit.name} at {position} - position is invalid");
                return false;
            }

            // Add to active units
            if (!_activeUnits.Contains(unit))
            {
                _activeUnits.Add(unit);
            }

            // Set position
            unit.transform.position = position;

            Debug.Log($"[BattlefieldManager] Spawned {unit.name} at {position}");
            OnUnitSpawned?.Invoke(unit);

            return true;
        }

        /// <summary>
        /// Remove a unit from the battlefield
        /// </summary>
        /// <param name="unit">Unit to remove</param>
        public void DespawnUnit(Unit unit)
        {
            if (unit == null)
                return;

            if (_activeUnits.Contains(unit))
            {
                _activeUnits.Remove(unit);
                Debug.Log($"[BattlefieldManager] Despawned {unit.name}");
                OnUnitDespawned?.Invoke(unit);
            }
        }

        /// <summary>
        /// Get all units belonging to a specific player
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <returns>List of units for that player</returns>
        public List<Unit> GetUnitsForPlayer(int playerIndex)
        {
            List<Unit> playerUnits = new List<Unit>();
            
            foreach (Unit unit in _activeUnits)
            {
                if (unit != null && unit.OwnerPlayerIndex == playerIndex)
                {
                    playerUnits.Add(unit);
                }
            }

            return playerUnits;
        }

        /// <summary>
        /// Get all enemy units for a specific player
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <returns>List of enemy units</returns>
        public List<Unit> GetEnemyUnits(int playerIndex)
        {
            List<Unit> enemyUnits = new List<Unit>();
            
            foreach (Unit unit in _activeUnits)
            {
                if (unit != null && unit.OwnerPlayerIndex != playerIndex)
                {
                    enemyUnits.Add(unit);
                }
            }

            return enemyUnits;
        }

        /// <summary>
        /// Clear all units from the battlefield
        /// </summary>
        public void ClearAllUnits()
        {
            List<Unit> unitsCopy = new List<Unit>(_activeUnits);
            foreach (Unit unit in unitsCopy)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }
            _activeUnits.Clear();
        }
        #endregion

        #region Position Validation
        /// <summary>
        /// Check if a position is valid on the battlefield
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="unitRadius">Radius of the unit</param>
        /// <param name="ignoreUnit">Unit to ignore in collision checks</param>
        /// <returns>True if position is valid</returns>
        public bool IsPositionValid(Vector3 position, float unitRadius, Unit ignoreUnit = null)
        {
            // Check if within battlefield bounds
            if (!IsWithinBounds(position))
                return false;

            // Check for collision with other units
            List<Transform> unitsToCheck = new List<Transform>();
            foreach (Unit unit in _activeUnits)
            {
                if (unit != null && unit != ignoreUnit)
                {
                    unitsToCheck.Add(unit.transform);
                }
            }

            if (CollisionUtility.WouldOverlapUnits(position, unitRadius, unitsToCheck))
                return false;

            // TODO: Phase 3 - Check for impassable terrain

            return true;
        }

        /// <summary>
        /// Check if a position is within battlefield bounds
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if within bounds</returns>
        public bool IsWithinBounds(Vector3 position)
        {
            Vector3 min = MinBounds;
            Vector3 max = MaxBounds;

            return position.x >= min.x && position.x <= max.x &&
                   position.z >= min.z && position.z <= max.z;
        }

        /// <summary>
        /// Clamp a position to battlefield bounds
        /// </summary>
        /// <param name="position">Position to clamp</param>
        /// <returns>Clamped position</returns>
        public Vector3 ClampToBounds(Vector3 position)
        {
            return MeasurementUtility.ClampToBounds(position, MinBounds, MaxBounds);
        }
        #endregion

        #region Unit Queries
        /// <summary>
        /// Get the closest unit to a position
        /// </summary>
        /// <param name="position">Position to check from</param>
        /// <param name="ignoreUnit">Unit to ignore</param>
        /// <returns>Closest unit, or null if none found</returns>
        public Unit GetClosestUnit(Vector3 position, Unit ignoreUnit = null)
        {
            Unit closestUnit = null;
            float closestDistance = float.MaxValue;

            foreach (Unit unit in _activeUnits)
            {
                if (unit == null || unit == ignoreUnit)
                    continue;

                float distance = position.HorizontalDistance(unit.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;
                }
            }

            return closestUnit;
        }

        /// <summary>
        /// Get all units within a radius of a position
        /// </summary>
        /// <param name="position">Center position</param>
        /// <param name="radiusInches">Radius in inches</param>
        /// <returns>List of units within radius</returns>
        public List<Unit> GetUnitsInRadius(Vector3 position, float radiusInches)
        {
            List<Unit> unitsInRange = new List<Unit>();
            float radiusUnityUnits = MeasurementUtility.InchesToUnityUnits(radiusInches);

            foreach (Unit unit in _activeUnits)
            {
                if (unit == null)
                    continue;

                float distance = position.HorizontalDistance(unit.Position);
                if (distance <= radiusUnityUnits)
                {
                    unitsInRange.Add(unit);
                }
            }

            return unitsInRange;
        }

        /// <summary>
        /// Notify that a unit has moved (for tracking purposes)
        /// </summary>
        /// <param name="unit">Unit that moved</param>
        /// <param name="fromPosition">Starting position</param>
        /// <param name="toPosition">Ending position</param>
        public void NotifyUnitMoved(Unit unit, Vector3 fromPosition, Vector3 toPosition)
        {
            OnUnitMoved?.Invoke(unit, fromPosition, toPosition);
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set battlefield size
        /// </summary>
        /// <param name="widthInches">Width in inches</param>
        /// <param name="depthInches">Depth in inches</param>
        public void SetBattlefieldSize(float widthInches, float depthInches)
        {
            _battlefieldWidthInches = widthInches;
            _battlefieldDepthInches = depthInches;
            Debug.Log($"[BattlefieldManager] Battlefield size set to {widthInches}\" x {depthInches}\"");
        }

        /// <summary>
        /// Set battlefield center
        /// </summary>
        /// <param name="center">New center position</param>
        public void SetBattlefieldCenter(Vector3 center)
        {
            _battlefieldCenter = center;
        }
        #endregion
    }
}