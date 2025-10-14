using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Warslammer.Battlefield;
using Warslammer.Utilities;

namespace Warslammer.Units
{
    /// <summary>
    /// Handles unit movement, rotation, and drag-and-drop
    /// Manages smooth movement animation and validation
    /// </summary>
    public class UnitMovement : MonoBehaviour
    {
        #region Events
        /// <summary>
        /// Fired when movement starts
        /// </summary>
        public UnityEvent<Vector3> OnMovementStart = new UnityEvent<Vector3>();
        
        /// <summary>
        /// Fired when movement completes
        /// </summary>
        public UnityEvent<Vector3> OnMovementComplete = new UnityEvent<Vector3>();
        
        /// <summary>
        /// Fired when movement is cancelled
        /// </summary>
        public UnityEvent OnMovementCancelled = new UnityEvent();
        #endregion

        #region Properties
        [Header("Movement Settings")]
        [SerializeField]
        [Tooltip("Movement speed for animation (Unity units per second)")]
        private float _movementAnimationSpeed = 5f;

        [SerializeField]
        [Tooltip("Rotation speed (degrees per second)")]
        private float _rotationSpeed = 180f;

        [Header("Ghost Preview")]
        [SerializeField]
        [Tooltip("Material for ghost sprite preview")]
        private Material _ghostMaterial;

        [SerializeField]
        [Tooltip("Ghost sprite renderer")]
        private SpriteRenderer _ghostSprite;

        [SerializeField]
        [Tooltip("Show ghost preview when dragging")]
        private bool _showGhostPreview = true;

        [Header("State")]
        [SerializeField]
        private bool _isMoving;
        
        /// <summary>
        /// Is this unit currently moving?
        /// </summary>
        public bool IsMoving => _isMoving;

        [SerializeField]
        private bool _isDragging;
        
        /// <summary>
        /// Is this unit being dragged?
        /// </summary>
        public bool IsDragging => _isDragging;

        private Unit _unit;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private Coroutine _moveCoroutine;
        private MovementValidator _movementValidator;
        private MeasurementTool _measurementTool;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _unit = GetComponent<Unit>();
            
            // Create ghost sprite if needed
            if (_showGhostPreview && _ghostSprite == null)
            {
                CreateGhostSprite();
            }

            if (_ghostSprite != null)
            {
                _ghostSprite.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Get references from battlefield manager
            if (BattlefieldManager.Instance != null)
            {
                _movementValidator = BattlefieldManager.Instance.GetComponent<MovementValidator>();
                _measurementTool = BattlefieldManager.Instance.GetComponent<MeasurementTool>();
            }
        }
        #endregion

        #region Ghost Preview
        /// <summary>
        /// Create ghost sprite for movement preview
        /// </summary>
        private void CreateGhostSprite()
        {
            GameObject ghostObj = new GameObject("GhostPreview");
            ghostObj.transform.SetParent(transform.parent);
            _ghostSprite = ghostObj.AddComponent<SpriteRenderer>();
            
            // Copy sprite from unit's visuals
            UnitVisuals visuals = GetComponent<UnitVisuals>();
            if (visuals != null)
            {
                _ghostSprite.sprite = visuals.GetCurrentSprite();
            }

            // Apply ghost material/transparency
            Color ghostColor = Color.white;
            ghostColor.a = 0.5f;
            _ghostSprite.color = ghostColor;
            _ghostSprite.sortingOrder = -1;

            ghostObj.SetActive(false);
        }

        /// <summary>
        /// Show ghost preview at position
        /// </summary>
        /// <param name="position">Position to show ghost at</param>
        /// <param name="isValid">Is the position valid?</param>
        private void ShowGhost(Vector3 position, bool isValid)
        {
            if (_ghostSprite == null)
                return;

            _ghostSprite.gameObject.SetActive(true);
            _ghostSprite.transform.position = position;

            // Color code based on validity
            Color ghostColor = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
            _ghostSprite.color = ghostColor;
        }

        /// <summary>
        /// Hide ghost preview
        /// </summary>
        private void HideGhost()
        {
            if (_ghostSprite != null)
            {
                _ghostSprite.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Drag and Drop
        /// <summary>
        /// Start dragging the unit
        /// </summary>
        public void StartDrag()
        {
            if (_isMoving || !_unit.CanMove)
                return;

            _isDragging = true;
            _startPosition = transform.position;

            Debug.Log($"[UnitMovement] Started dragging {name}");
        }

        /// <summary>
        /// Update drag position (called each frame while dragging)
        /// </summary>
        /// <param name="worldPosition">Current mouse/touch world position</param>
        public void UpdateDrag(Vector3 worldPosition)
        {
            if (!_isDragging)
                return;

            // Flatten Y position
            worldPosition.y = transform.position.y;

            // Show measurement tool
            if (_measurementTool != null)
            {
                _measurementTool.ShowMeasurement(_unit, worldPosition);
            }

            // Validate position
            bool isValid = false;
            if (_movementValidator != null)
            {
                var result = _movementValidator.ValidateMove(_unit, worldPosition);
                isValid = result.isValid;
            }

            // Show ghost preview
            if (_showGhostPreview)
            {
                ShowGhost(worldPosition, isValid);
            }
        }

        /// <summary>
        /// End dragging and attempt to move to position
        /// </summary>
        /// <param name="worldPosition">Final position</param>
        public void EndDrag(Vector3 worldPosition)
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            HideGhost();

            // Hide measurement tool
            if (_measurementTool != null)
            {
                _measurementTool.Hide();
            }

            // Flatten Y position
            worldPosition.y = transform.position.y;

            // Validate move
            if (_movementValidator != null)
            {
                var result = _movementValidator.ValidateMove(_unit, worldPosition);
                
                if (result.isValid)
                {
                    MoveTo(worldPosition);
                }
                else
                {
                    Debug.LogWarning($"[UnitMovement] Invalid move: {result.reason}");
                    
                    // Snap back to start position
                    SnapToPosition(_startPosition);
                    OnMovementCancelled?.Invoke();
                }
            }
            else
            {
                // No validator, just move
                MoveTo(worldPosition);
            }
        }

        /// <summary>
        /// Cancel dragging
        /// </summary>
        public void CancelDrag()
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            HideGhost();

            if (_measurementTool != null)
            {
                _measurementTool.Hide();
            }

            SnapToPosition(_startPosition);
            OnMovementCancelled?.Invoke();
        }
        #endregion

        #region Movement
        /// <summary>
        /// Move unit to target position
        /// </summary>
        /// <param name="targetPosition">Destination</param>
        public void MoveTo(Vector3 targetPosition)
        {
            if (_isMoving)
            {
                Debug.LogWarning($"[UnitMovement] {name} is already moving!");
                return;
            }

            _startPosition = transform.position;
            _targetPosition = targetPosition;

            // Start movement coroutine
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
            }

            _moveCoroutine = StartCoroutine(MoveCoroutine());
        }

        /// <summary>
        /// Movement animation coroutine
        /// </summary>
        private IEnumerator MoveCoroutine()
        {
            _isMoving = true;
            OnMovementStart?.Invoke(_targetPosition);

            float distance = Vector3.Distance(_startPosition, _targetPosition);
            float duration = distance / _movementAnimationSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Smooth movement
                transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);

                yield return null;
            }

            // Ensure we're exactly at target
            transform.position = _targetPosition;

            // Update unit stats
            float movementCost = MeasurementUtility.GetDistanceInches(_startPosition, _targetPosition);
            _unit.ConsumeMovement(movementCost);

            // Notify battlefield manager
            if (BattlefieldManager.Instance != null)
            {
                BattlefieldManager.Instance.NotifyUnitMoved(_unit, _startPosition, _targetPosition);
            }

            _isMoving = false;
            OnMovementComplete?.Invoke(_targetPosition);

            Debug.Log($"[UnitMovement] {name} moved {MeasurementUtility.FormatDistance(movementCost)}");
        }

        /// <summary>
        /// Instantly snap to a position (no animation)
        /// </summary>
        /// <param name="position">Position to snap to</param>
        public void SnapToPosition(Vector3 position)
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            transform.position = position;
            _isMoving = false;
        }

        /// <summary>
        /// Stop current movement
        /// </summary>
        public void StopMovement()
        {
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }

            _isMoving = false;
        }
        #endregion

        #region Rotation
        /// <summary>
        /// Rotate unit to face a direction
        /// </summary>
        /// <param name="direction">Direction to face</param>
        public void FaceDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.001f)
                return;

            float targetAngle = MeasurementUtility.GetAngleBetween(transform.position, transform.position + direction);
            StartCoroutine(RotateCoroutine(targetAngle));
        }

        /// <summary>
        /// Rotate unit to face a position
        /// </summary>
        /// <param name="position">Position to face</param>
        public void FacePosition(Vector3 position)
        {
            Vector3 direction = (position - transform.position).Flatten();
            FaceDirection(direction);
        }

        /// <summary>
        /// Rotation animation coroutine
        /// </summary>
        private IEnumerator RotateCoroutine(float targetAngle)
        {
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

            float duration = Quaternion.Angle(startRotation, targetRotation) / _rotationSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            transform.rotation = targetRotation;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set movement animation speed
        /// </summary>
        /// <param name="speed">Speed in Unity units per second</param>
        public void SetMovementSpeed(float speed)
        {
            _movementAnimationSpeed = speed;
        }

        /// <summary>
        /// Set rotation speed
        /// </summary>
        /// <param name="speed">Speed in degrees per second</param>
        public void SetRotationSpeed(float speed)
        {
            _rotationSpeed = speed;
        }

        /// <summary>
        /// Enable/disable ghost preview
        /// </summary>
        /// <param name="enabled">Show ghost?</param>
        public void SetGhostPreview(bool enabled)
        {
            _showGhostPreview = enabled;
        }
        #endregion
    }
}