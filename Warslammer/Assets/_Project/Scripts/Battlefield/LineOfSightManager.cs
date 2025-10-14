using System.Collections.Generic;
using UnityEngine;
using Warslammer.Units;
using Warslammer.Utilities;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Calculates line of sight between units using 2D raycasts
    /// Handles LOS blocking terrain and unit occlusion
    /// </summary>
    public class LineOfSightManager : MonoBehaviour
    {
        #region Properties
        [Header("Line of Sight Configuration")]
        [SerializeField]
        [Tooltip("Layer mask for LOS blocking objects")]
        private LayerMask _losBlockingLayers;

        /// <summary>
        /// Layer mask for LOS blocking objects
        /// </summary>
        public LayerMask LosBlockingLayers => _losBlockingLayers;

        [SerializeField]
        [Tooltip("Height offset for LOS checks (from unit position)")]
        private float _losHeightOffset = 0.5f;

        /// <summary>
        /// Height offset for LOS checks
        /// </summary>
        public float LosHeightOffset => _losHeightOffset;

        [Header("Visualization")]
        [SerializeField]
        [Tooltip("Draw LOS rays in scene view")]
        private bool _debugDrawRays = false;

        private TerrainManager _terrainManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _terrainManager = GetComponent<TerrainManager>();
        }
        #endregion

        #region Line of Sight Checks
        /// <summary>
        /// Check if unit A has line of sight to unit B
        /// </summary>
        /// <param name="fromUnit">Observing unit</param>
        /// <param name="toUnit">Target unit</param>
        /// <returns>True if LOS is clear</returns>
        public bool HasLineOfSight(Unit fromUnit, Unit toUnit)
        {
            if (fromUnit == null || toUnit == null)
                return false;

            Vector3 fromPosition = fromUnit.Position + Vector3.up * _losHeightOffset;
            Vector3 toPosition = toUnit.Position + Vector3.up * _losHeightOffset;

            return HasLineOfSight(fromPosition, toPosition);
        }

        /// <summary>
        /// Check if there is line of sight between two positions
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Target position</param>
        /// <returns>True if LOS is clear</returns>
        public bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            // Perform raycast
            RaycastHit hit;
            if (Physics.Raycast(from, direction.normalized, out hit, distance, _losBlockingLayers))
            {
                // Something is blocking LOS
                if (_debugDrawRays)
                {
                    Debug.DrawLine(from, hit.point, Color.red, 1f);
                }
                return false;
            }

            // Check if terrain blocks LOS
            if (_terrainManager != null && _terrainManager.TerrainBlocksLineOfSight(from, to))
            {
                if (_debugDrawRays)
                {
                    Debug.DrawLine(from, to, Color.yellow, 1f);
                }
                return false;
            }

            if (_debugDrawRays)
            {
                Debug.DrawLine(from, to, Color.green, 1f);
            }

            return true;
        }

        /// <summary>
        /// Get all units that have line of sight to a target unit
        /// </summary>
        /// <param name="targetUnit">Target unit</param>
        /// <param name="potentialObservers">List of units to check</param>
        /// <returns>List of units that can see the target</returns>
        public List<Unit> GetUnitsWithLineOfSight(Unit targetUnit, List<Unit> potentialObservers)
        {
            List<Unit> observers = new List<Unit>();

            if (targetUnit == null)
                return observers;

            foreach (Unit observer in potentialObservers)
            {
                if (observer == null || observer == targetUnit)
                    continue;

                if (HasLineOfSight(observer, targetUnit))
                {
                    observers.Add(observer);
                }
            }

            return observers;
        }

        /// <summary>
        /// Check if a position is visible from a unit
        /// </summary>
        /// <param name="unit">Observing unit</param>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is visible</returns>
        public bool IsPositionVisible(Unit unit, Vector3 position)
        {
            if (unit == null)
                return false;

            Vector3 fromPosition = unit.Position + Vector3.up * _losHeightOffset;
            Vector3 toPosition = position + Vector3.up * _losHeightOffset;

            return HasLineOfSight(fromPosition, toPosition);
        }

        /// <summary>
        /// Get percentage of unit that is visible (for partial cover)
        /// </summary>
        /// <param name="fromUnit">Observing unit</param>
        /// <param name="toUnit">Target unit</param>
        /// <returns>Visibility percentage (0.0 to 1.0)</returns>
        public float GetVisibilityPercentage(Unit fromUnit, Unit toUnit)
        {
            if (fromUnit == null || toUnit == null)
                return 0f;

            // Simple implementation - check multiple height points
            int visiblePoints = 0;
            int totalPoints = 5;

            for (int i = 0; i < totalPoints; i++)
            {
                float heightOffset = (i / (float)(totalPoints - 1)) * 2f * _losHeightOffset;
                Vector3 fromPos = fromUnit.Position + Vector3.up * _losHeightOffset;
                Vector3 toPos = toUnit.Position + Vector3.up * heightOffset;

                if (HasLineOfSight(fromPos, toPos))
                {
                    visiblePoints++;
                }
            }

            return visiblePoints / (float)totalPoints;
        }
        #endregion

        #region Area Queries
        /// <summary>
        /// Check if a position has cover from a specific direction
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="fromDirection">Direction to check cover from</param>
        /// <param name="checkDistance">Distance to check for cover</param>
        /// <returns>True if position has cover</returns>
        public bool HasCoverFromDirection(Vector3 position, Vector3 fromDirection, float checkDistance = 1f)
        {
            Vector3 checkPosition = position + Vector3.up * _losHeightOffset;
            Vector3 sourcePosition = checkPosition - fromDirection.normalized * checkDistance;

            return !HasLineOfSight(sourcePosition, checkPosition);
        }

        /// <summary>
        /// Get all positions within range that have LOS to a target
        /// </summary>
        /// <param name="targetPosition">Target position</param>
        /// <param name="searchPositions">Positions to check</param>
        /// <returns>List of positions with clear LOS</returns>
        public List<Vector3> GetPositionsWithLineOfSight(Vector3 targetPosition, List<Vector3> searchPositions)
        {
            List<Vector3> validPositions = new List<Vector3>();

            Vector3 targetPos = targetPosition + Vector3.up * _losHeightOffset;

            foreach (Vector3 position in searchPositions)
            {
                Vector3 fromPos = position + Vector3.up * _losHeightOffset;
                
                if (HasLineOfSight(fromPos, targetPos))
                {
                    validPositions.Add(position);
                }
            }

            return validPositions;
        }
        #endregion

        #region 2D Circle Checks
        /// <summary>
        /// Check if a 2D circle blocks line of sight
        /// Used for unit-to-unit LOS blocking
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="circleCenter">Center of blocking circle</param>
        /// <param name="circleRadius">Radius of blocking circle</param>
        /// <returns>True if circle blocks LOS</returns>
        public bool CircleBlocksLineOfSight(Vector3 from, Vector3 to, Vector3 circleCenter, float circleRadius)
        {
            return CollisionUtility.RayIntersectsCircle(from, to, circleCenter, circleRadius);
        }

        /// <summary>
        /// Check if any units block LOS between two positions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <param name="unitsToCheck">Units that might block LOS</param>
        /// <param name="ignoreUnits">Units to ignore (e.g., shooter and target)</param>
        /// <returns>True if a unit blocks LOS</returns>
        public bool AnyUnitBlocksLineOfSight(Vector3 from, Vector3 to, List<Unit> unitsToCheck, List<Unit> ignoreUnits = null)
        {
            foreach (Unit unit in unitsToCheck)
            {
                if (unit == null)
                    continue;

                // Skip ignored units
                if (ignoreUnits != null && ignoreUnits.Contains(unit))
                    continue;

                float unitRadius = unit.GetBaseRadius();
                
                if (CircleBlocksLineOfSight(from, to, unit.Position, unitRadius))
                    return true;
            }

            return false;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set LOS height offset
        /// </summary>
        /// <param name="offset">Height offset in Unity units</param>
        public void SetLosHeightOffset(float offset)
        {
            _losHeightOffset = offset;
        }

        /// <summary>
        /// Set LOS blocking layers
        /// </summary>
        /// <param name="layers">Layer mask for blocking objects</param>
        public void SetLosBlockingLayers(LayerMask layers)
        {
            _losBlockingLayers = layers;
        }

        /// <summary>
        /// Toggle debug ray visualization
        /// </summary>
        /// <param name="debug">Enable debug rays?</param>
        public void SetDebugDrawRays(bool debug)
        {
            _debugDrawRays = debug;
        }
        #endregion
    }
}