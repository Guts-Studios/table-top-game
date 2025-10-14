using UnityEngine;
using Warslammer.Core;
using Warslammer.Units;
using Warslammer.Utilities;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Validates if a move is legal (within range, no overlap)
    /// Provides feedback on why moves are invalid
    /// </summary>
    public class MovementValidator : MonoBehaviour
    {
        #region Validation Result
        /// <summary>
        /// Result of a movement validation check
        /// </summary>
        public struct ValidationResult
        {
            public bool isValid;
            public string reason;
            public Vector3 suggestedPosition;

            public ValidationResult(bool valid, string message = "", Vector3 suggested = default)
            {
                isValid = valid;
                reason = message;
                suggestedPosition = suggested;
            }
        }
        #endregion

        #region Properties
        [Header("Validation Settings")]
        [SerializeField]
        [Tooltip("Check for unit collisions")]
        private bool _checkCollisions = true;

        [SerializeField]
        [Tooltip("Check battlefield bounds")]
        private bool _checkBounds = true;

        [SerializeField]
        [Tooltip("Check movement range")]
        private bool _checkRange = true;

        [SerializeField]
        [Tooltip("Check for impassable terrain")]
        private bool _checkTerrain = false; // TODO: Phase 3

        [SerializeField]
        [Tooltip("Try to find alternative valid position if move is invalid")]
        private bool _suggestAlternatives = true;

        private BattlefieldManager _battlefieldManager;
        private TerrainManager _terrainManager;
        private TurnManager _turnManager;
        private PhaseManager _phaseManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _battlefieldManager = GetComponent<BattlefieldManager>();
            _terrainManager = GetComponent<TerrainManager>();
        }

        private void Start()
        {
            _turnManager = TurnManager.Instance;
            _phaseManager = PhaseManager.Instance;
        }
        #endregion

        #region Movement Validation
        /// <summary>
        /// Validate if a unit can move to a target position
        /// </summary>
        /// <param name="unit">Unit attempting to move</param>
        /// <param name="targetPosition">Desired position</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateMove(Unit unit, Vector3 targetPosition)
        {
            if (unit == null)
            {
                return new ValidationResult(false, "Unit is null");
            }

            // Check if it's the correct phase
            if (_phaseManager != null && !_phaseManager.IsInPhase(GamePhase.Movement))
            {
                return new ValidationResult(false, "Can only move during Movement phase");
            }

            // Check if it's the unit's owner's turn
            if (_turnManager != null && unit.OwnerPlayerIndex != _turnManager.ActivePlayerIndex)
            {
                return new ValidationResult(false, "Not your turn");
            }

            // Check if unit can move
            if (!unit.CanMove)
            {
                return new ValidationResult(false, "Unit cannot move (already moved or stunned)");
            }

            // Check movement range
            if (_checkRange)
            {
                float distance = MeasurementUtility.GetDistanceInches(unit.Position, targetPosition);
                if (distance > unit.RemainingMovement)
                {
                    return new ValidationResult(false, 
                        $"Target is too far ({MeasurementUtility.FormatDistance(distance)} > {MeasurementUtility.FormatDistance(unit.RemainingMovement)})");
                }
            }

            // Check battlefield bounds
            if (_checkBounds && _battlefieldManager != null)
            {
                if (!_battlefieldManager.IsWithinBounds(targetPosition))
                {
                    Vector3 clamped = _battlefieldManager.ClampToBounds(targetPosition);
                    
                    if (_suggestAlternatives)
                    {
                        return new ValidationResult(false, "Position is outside battlefield", clamped);
                    }
                    
                    return new ValidationResult(false, "Position is outside battlefield");
                }
            }

            // Check collisions with other units
            if (_checkCollisions && _battlefieldManager != null)
            {
                float unitRadius = unit.GetBaseRadius();
                
                if (!_battlefieldManager.IsPositionValid(targetPosition, unitRadius, unit))
                {
                    if (_suggestAlternatives)
                    {
                        // Try to find nearby valid position
                        Vector3 suggested = FindNearbyValidPosition(unit, targetPosition);
                        if (suggested != targetPosition)
                        {
                            return new ValidationResult(false, "Position overlaps another unit", suggested);
                        }
                    }
                    
                    return new ValidationResult(false, "Position overlaps another unit");
                }
            }

            // Check terrain (Phase 3)
            if (_checkTerrain && _terrainManager != null)
            {
                if (_terrainManager.IsPositionImpassable(targetPosition))
                {
                    return new ValidationResult(false, "Position is on impassable terrain");
                }
            }

            // All checks passed
            return new ValidationResult(true, "Move is valid");
        }

        /// <summary>
        /// Quick check if a position is reachable (ignoring detailed checks)
        /// </summary>
        /// <param name="unit">Unit to check for</param>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is reachable</returns>
        public bool IsPositionReachable(Unit unit, Vector3 position)
        {
            if (unit == null)
                return false;

            float distance = MeasurementUtility.GetDistanceInches(unit.Position, position);
            return distance <= unit.RemainingMovement;
        }

        /// <summary>
        /// Get the movement cost to reach a position
        /// </summary>
        /// <param name="unit">Unit attempting to move</param>
        /// <param name="position">Target position</param>
        /// <returns>Movement cost in inches (float.MaxValue if impassable)</returns>
        public float GetMovementCost(Unit unit, Vector3 position)
        {
            if (unit == null)
                return float.MaxValue;

            float baseDistance = MeasurementUtility.GetDistanceInches(unit.Position, position);

            // Apply terrain modifier (Phase 3)
            if (_terrainManager != null)
            {
                float modifier = _terrainManager.GetMovementModifier(position);
                return baseDistance * modifier;
            }

            return baseDistance;
        }
        #endregion

        #region Path Validation
        /// <summary>
        /// Validate if a path between two positions is clear
        /// </summary>
        /// <param name="unit">Unit moving</param>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <returns>True if path is clear</returns>
        public bool IsPathClear(Unit unit, Vector3 from, Vector3 to)
        {
            // Simple implementation - check several points along the path
            int checkPoints = 5;
            
            for (int i = 0; i <= checkPoints; i++)
            {
                float t = i / (float)checkPoints;
                Vector3 checkPos = Vector3.Lerp(from, to, t);
                
                // Check if this position is valid
                if (_battlefieldManager != null)
                {
                    float unitRadius = unit.GetBaseRadius();
                    if (!_battlefieldManager.IsPositionValid(checkPos, unitRadius, unit))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Find a nearby valid position if the desired position is invalid
        /// </summary>
        /// <param name="unit">Unit attempting to move</param>
        /// <param name="desiredPosition">Desired position</param>
        /// <returns>Nearest valid position</returns>
        public Vector3 FindNearbyValidPosition(Unit unit, Vector3 desiredPosition)
        {
            if (unit == null || _battlefieldManager == null)
                return desiredPosition;

            float unitRadius = unit.GetBaseRadius();
            
            // Use CollisionUtility to find valid position
            var allUnits = _battlefieldManager.ActiveUnits;
            var unitTransforms = new System.Collections.Generic.List<Transform>();
            
            foreach (var u in allUnits)
            {
                if (u != null && u != unit)
                {
                    unitTransforms.Add(u.transform);
                }
            }

            Vector3 validPosition = CollisionUtility.FindNearestValidPosition(
                desiredPosition, 
                unitRadius, 
                unitTransforms, 
                unit.transform,
                MeasurementUtility.InchesToUnityUnits(3f) // Search within 3"
            );

            // Make sure it's still within range
            float distance = MeasurementUtility.GetDistanceInches(unit.Position, validPosition);
            if (distance > unit.RemainingMovement)
            {
                // Clamp to movement range
                Vector3 direction = (validPosition - unit.Position).normalized;
                float maxDistance = MeasurementUtility.InchesToUnityUnits(unit.RemainingMovement);
                validPosition = unit.Position + direction * maxDistance;
            }

            // Clamp to battlefield bounds
            if (_battlefieldManager != null)
            {
                validPosition = _battlefieldManager.ClampToBounds(validPosition);
            }

            return validPosition;
        }
        #endregion

        #region Deployment Validation
        /// <summary>
        /// Validate if a unit can be deployed at a position
        /// </summary>
        /// <param name="unit">Unit to deploy</param>
        /// <param name="position">Deployment position</param>
        /// <param name="playerIndex">Player deploying the unit</param>
        /// <returns>Validation result</returns>
        public ValidationResult ValidateDeployment(Unit unit, Vector3 position, int playerIndex)
        {
            if (unit == null)
            {
                return new ValidationResult(false, "Unit is null");
            }

            // Check deployment zone
            DeploymentZoneManager deploymentManager = GetComponent<DeploymentZoneManager>();
            if (deploymentManager != null)
            {
                if (!deploymentManager.IsValidDeploymentPosition(position, playerIndex))
                {
                    Vector3 clamped = deploymentManager.ClampToDeploymentZone(position, playerIndex);
                    return new ValidationResult(false, "Position is outside deployment zone", clamped);
                }
            }

            // Check collisions
            if (_battlefieldManager != null)
            {
                float unitRadius = unit.GetBaseRadius();
                if (!_battlefieldManager.IsPositionValid(position, unitRadius, unit))
                {
                    return new ValidationResult(false, "Position overlaps another unit");
                }
            }

            return new ValidationResult(true, "Deployment position is valid");
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Enable/disable specific validation checks
        /// </summary>
        public void SetValidationChecks(bool collisions, bool bounds, bool range, bool terrain)
        {
            _checkCollisions = collisions;
            _checkBounds = bounds;
            _checkRange = range;
            _checkTerrain = terrain;
        }

        /// <summary>
        /// Enable/disable suggestion of alternative positions
        /// </summary>
        public void SetSuggestAlternatives(bool suggest)
        {
            _suggestAlternatives = suggest;
        }
        #endregion
    }
}