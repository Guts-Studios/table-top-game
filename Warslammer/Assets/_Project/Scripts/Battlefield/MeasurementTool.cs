using UnityEngine;
using Warslammer.Core;
using Warslammer.Units;
using Warslammer.Utilities;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Visual ruler showing distance from unit to target position
    /// Displays measurement marks and color-codes based on validity
    /// </summary>
    public class MeasurementTool : MonoBehaviour
    {
        #region Properties
        [Header("Measurement Configuration")]
        [SerializeField]
        [Tooltip("Line renderer for drawing measurement line")]
        private LineRenderer _lineRenderer;

        [SerializeField]
        [Tooltip("Color when move is valid (within range)")]
        private Color _validColor = Color.green;

        [SerializeField]
        [Tooltip("Color when move is invalid (too far)")]
        private Color _invalidColor = Color.red;

        [SerializeField]
        [Tooltip("Line width")]
        private float _lineWidth = 0.1f;

        [Header("Measurement Marks")]
        [SerializeField]
        [Tooltip("Prefab for measurement marks")]
        private GameObject _measurementMarkPrefab;

        [SerializeField]
        [Tooltip("Spacing between marks in inches")]
        private float _markSpacingInches = 3f;

        [SerializeField]
        [Tooltip("Parent transform for measurement marks")]
        private Transform _marksContainer;

        [Header("Text Display")]
        [SerializeField]
        [Tooltip("Text mesh for displaying distance")]
        private TextMesh _distanceText;

        [SerializeField]
        [Tooltip("Offset for distance text from target position")]
        private Vector3 _textOffset = new Vector3(0, 1f, 0);

        private Unit _currentUnit;
        private Vector3 _targetPosition;
        private bool _isActive;
        private ObjectPool<MeasurementMarker> _markPool;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Create line renderer if not assigned
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            // Configure line renderer
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.positionCount = 2;
            _lineRenderer.enabled = false;

            // Create marks container
            if (_marksContainer == null)
            {
                GameObject container = new GameObject("MeasurementMarks");
                container.transform.SetParent(transform);
                _marksContainer = container.transform;
            }

            // Create distance text if not assigned
            if (_distanceText == null)
            {
                GameObject textObj = new GameObject("DistanceText");
                textObj.transform.SetParent(transform);
                _distanceText = textObj.AddComponent<TextMesh>();
                _distanceText.fontSize = 24;
                _distanceText.anchor = TextAnchor.MiddleCenter;
                _distanceText.alignment = TextAlignment.Center;
            }

            _distanceText.gameObject.SetActive(false);

            // Initialize mark pool
            if (_measurementMarkPrefab != null)
            {
                // Ensure the prefab has a MeasurementMarker component
                MeasurementMarker markerPrefab = _measurementMarkPrefab.GetComponent<MeasurementMarker>();
                if (markerPrefab == null)
                {
                    markerPrefab = _measurementMarkPrefab.AddComponent<MeasurementMarker>();
                }
                
                _markPool = new ObjectPool<MeasurementMarker>(
                    markerPrefab,
                    _marksContainer,
                    20,
                    100
                );
            }
        }

        private void Update()
        {
            if (_isActive && _currentUnit != null)
            {
                // Keep text facing camera
                if (_distanceText != null && Camera.main != null)
                {
                    _distanceText.transform.LookAt(Camera.main.transform);
                    _distanceText.transform.Rotate(0, 180, 0);
                }
            }
        }
        #endregion

        #region Measurement Display
        /// <summary>
        /// Show measurement from unit to target position
        /// </summary>
        /// <param name="unit">Unit to measure from</param>
        /// <param name="targetPosition">Target position to measure to</param>
        public void ShowMeasurement(Unit unit, Vector3 targetPosition)
        {
            if (unit == null)
            {
                Hide();
                return;
            }

            _currentUnit = unit;
            _targetPosition = targetPosition;
            _isActive = true;

            // Calculate distance
            float distanceInches = MeasurementUtility.GetDistanceInches(unit.Position, targetPosition);
            bool isValid = distanceInches <= unit.RemainingMovement;

            // Update line renderer
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, unit.Position + Vector3.up * 0.1f);
            _lineRenderer.SetPosition(1, targetPosition + Vector3.up * 0.1f);
            
            // Set color based on validity
            Color lineColor = isValid ? _validColor : _invalidColor;
            _lineRenderer.startColor = lineColor;
            _lineRenderer.endColor = lineColor;

            // Update measurement marks
            UpdateMeasurementMarks(unit.Position, targetPosition);

            // Update distance text
            UpdateDistanceText(targetPosition, distanceInches, isValid);
        }

        /// <summary>
        /// Update measurement marks along the line
        /// </summary>
        /// <param name="startPosition">Start position</param>
        /// <param name="endPosition">End position</param>
        private void UpdateMeasurementMarks(Vector3 startPosition, Vector3 endPosition)
        {
            if (_markPool == null)
                return;

            // Return all marks to pool
            _markPool.ReturnAll();

            float distanceInches = MeasurementUtility.GetDistanceInches(startPosition, endPosition);
            int markCount = Mathf.FloorToInt(distanceInches / _markSpacingInches);

            if (markCount == 0)
                return;

            Vector3 direction = (endPosition - startPosition).normalized;
            float markSpacingUnits = MeasurementUtility.InchesToUnityUnits(_markSpacingInches);

            for (int i = 1; i <= markCount; i++)
            {
                float distance = i * markSpacingUnits;
                Vector3 markPosition = startPosition + direction * distance;
                markPosition.y = 0.15f; // Slightly above ground

                MeasurementMarker marker = _markPool.Get();
                marker.transform.position = markPosition;
                marker.transform.rotation = Quaternion.Euler(90, 0, 0); // Face up
            }
        }

        /// <summary>
        /// Update distance text display
        /// </summary>
        /// <param name="position">Position for text</param>
        /// <param name="distanceInches">Distance in inches</param>
        /// <param name="isValid">Is the distance valid?</param>
        private void UpdateDistanceText(Vector3 position, float distanceInches, bool isValid)
        {
            if (_distanceText == null)
                return;

            _distanceText.gameObject.SetActive(true);
            _distanceText.transform.position = position + _textOffset;

            // Format distance text
            string distanceStr = MeasurementUtility.FormatDistance(distanceInches, 1);
            
            if (_currentUnit != null)
            {
                string validityText = isValid ? "" : $" (Max: {MeasurementUtility.FormatDistance(_currentUnit.RemainingMovement, 1)})";
                _distanceText.text = $"{distanceStr}{validityText}";
            }
            else
            {
                _distanceText.text = distanceStr;
            }

            // Set text color
            _distanceText.color = isValid ? _validColor : _invalidColor;
        }

        /// <summary>
        /// Hide the measurement tool
        /// </summary>
        public void Hide()
        {
            _isActive = false;
            _currentUnit = null;

            if (_lineRenderer != null)
                _lineRenderer.enabled = false;

            if (_distanceText != null)
                _distanceText.gameObject.SetActive(false);

            if (_markPool != null)
                _markPool.ReturnAll();
        }

        /// <summary>
        /// Check if measurement tool is currently active
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Get the current distance being measured in inches
        /// </summary>
        public float GetCurrentDistance()
        {
            if (!_isActive || _currentUnit == null)
                return 0f;

            return MeasurementUtility.GetDistanceInches(_currentUnit.Position, _targetPosition);
        }

        /// <summary>
        /// Check if current measurement is valid
        /// </summary>
        public bool IsCurrentMeasurementValid()
        {
            if (!_isActive || _currentUnit == null)
                return false;

            float distance = GetCurrentDistance();
            return distance <= _currentUnit.RemainingMovement;
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set colors for valid/invalid measurements
        /// </summary>
        /// <param name="validColor">Color for valid range</param>
        /// <param name="invalidColor">Color for invalid range</param>
        public void SetColors(Color validColor, Color invalidColor)
        {
            _validColor = validColor;
            _invalidColor = invalidColor;
        }

        /// <summary>
        /// Set measurement mark spacing
        /// </summary>
        /// <param name="spacingInches">Spacing between marks in inches</param>
        public void SetMarkSpacing(float spacingInches)
        {
            _markSpacingInches = spacingInches;
        }

        /// <summary>
        /// Set line width
        /// </summary>
        /// <param name="width">Width in Unity units</param>
        public void SetLineWidth(float width)
        {
            _lineWidth = width;
            if (_lineRenderer != null)
            {
                _lineRenderer.startWidth = width;
                _lineRenderer.endWidth = width;
            }
        }
        #endregion
    }
}