using UnityEngine;

namespace Warslammer.Input
{
    /// <summary>
    /// Isometric camera controller
    /// Handles pan, zoom, and rotation
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region Properties
        [Header("Camera Settings")]
        [SerializeField]
        [Tooltip("Camera transform to control")]
        private Transform _cameraTransform;

        [Header("Pan Settings")]
        [SerializeField]
        [Tooltip("Pan speed")]
        private float _panSpeed = 10f;

        [SerializeField]
        [Tooltip("Edge scroll distance from screen edge")]
        private float _edgeScrollDistance = 20f;

        [SerializeField]
        [Tooltip("Enable edge scrolling")]
        private bool _enableEdgeScroll = true;

        [Header("Zoom Settings")]
        [SerializeField]
        [Tooltip("Zoom speed")]
        private float _zoomSpeed = 10f;

        [SerializeField]
        [Tooltip("Minimum zoom distance")]
        private float _minZoom = 5f;

        [SerializeField]
        [Tooltip("Maximum zoom distance")]
        private float _maxZoom = 50f;

        [Header("Rotation Settings")]
        [SerializeField]
        [Tooltip("Rotation speed (degrees per second)")]
        private float _rotationSpeed = 90f;

        [SerializeField]
        [Tooltip("Enable camera rotation")]
        private bool _enableRotation = true;

        [Header("Bounds")]
        [SerializeField]
        [Tooltip("Limit camera to bounds")]
        private bool _useBounds = true;

        [SerializeField]
        [Tooltip("Camera movement bounds (min)")]
        private Vector3 _boundsMin = new Vector3(-50, 0, -50);

        [SerializeField]
        [Tooltip("Camera movement bounds (max)")]
        private Vector3 _boundsMax = new Vector3(50, 0, 50);

        private InputManager _inputManager;
        private float _currentZoom;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_cameraTransform == null)
            {
                _cameraTransform = transform;
            }

            _currentZoom = _cameraTransform.position.y;
        }

        private void Start()
        {
            _inputManager = InputManager.Instance;

            if (_inputManager != null)
            {
                _inputManager.OnPanInput.AddListener(OnPanInput);
                _inputManager.OnZoomInput.AddListener(OnZoomInput);
                _inputManager.OnRotateInput.AddListener(OnRotateInput);
            }
        }

        private void Update()
        {
            if (_enableEdgeScroll)
            {
                HandleEdgeScroll();
            }
        }

        private void OnDestroy()
        {
            if (_inputManager != null)
            {
                _inputManager.OnPanInput.RemoveListener(OnPanInput);
                _inputManager.OnZoomInput.RemoveListener(OnZoomInput);
                _inputManager.OnRotateInput.RemoveListener(OnRotateInput);
            }
        }
        #endregion

        #region Pan
        /// <summary>
        /// Handle pan input from keyboard/touch
        /// </summary>
        private void OnPanInput(Vector2 panDirection)
        {
            Vector3 movement = new Vector3(panDirection.x, 0, panDirection.y);
            movement = _cameraTransform.TransformDirection(movement);
            movement.y = 0;

            PanCamera(movement * _panSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Handle edge scrolling
        /// </summary>
        private void HandleEdgeScroll()
        {
            Vector3 movement = Vector3.zero;
            Vector3 mousePos = UnityEngine.Input.mousePosition;

            if (mousePos.x < _edgeScrollDistance)
                movement.x = -1f;
            else if (mousePos.x > Screen.width - _edgeScrollDistance)
                movement.x = 1f;

            if (mousePos.y < _edgeScrollDistance)
                movement.z = -1f;
            else if (mousePos.y > Screen.height - _edgeScrollDistance)
                movement.z = 1f;

            if (movement.sqrMagnitude > 0.01f)
            {
                movement = _cameraTransform.TransformDirection(movement);
                movement.y = 0;
                PanCamera(movement.normalized * _panSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Pan camera by movement vector
        /// </summary>
        private void PanCamera(Vector3 movement)
        {
            Vector3 newPosition = _cameraTransform.position + movement;

            if (_useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, _boundsMin.x, _boundsMax.x);
                newPosition.z = Mathf.Clamp(newPosition.z, _boundsMin.z, _boundsMax.z);
            }

            _cameraTransform.position = newPosition;
        }
        #endregion

        #region Zoom
        /// <summary>
        /// Handle zoom input
        /// </summary>
        private void OnZoomInput(float zoomDelta)
        {
            _currentZoom -= zoomDelta * _zoomSpeed;
            _currentZoom = Mathf.Clamp(_currentZoom, _minZoom, _maxZoom);

            Vector3 newPosition = _cameraTransform.position;
            newPosition.y = _currentZoom;
            _cameraTransform.position = newPosition;
        }
        #endregion

        #region Rotation
        /// <summary>
        /// Handle rotation input
        /// </summary>
        private void OnRotateInput(float rotationDelta)
        {
            if (!_enableRotation)
                return;

            float rotation = rotationDelta * _rotationSpeed * Time.deltaTime;
            _cameraTransform.Rotate(Vector3.up, rotation, Space.World);
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set camera bounds
        /// </summary>
        public void SetBounds(Vector3 min, Vector3 max)
        {
            _boundsMin = min;
            _boundsMax = max;
            _useBounds = true;
        }

        /// <summary>
        /// Set pan speed
        /// </summary>
        public void SetPanSpeed(float speed)
        {
            _panSpeed = speed;
        }

        /// <summary>
        /// Set zoom limits
        /// </summary>
        public void SetZoomLimits(float min, float max)
        {
            _minZoom = min;
            _maxZoom = max;
            _currentZoom = Mathf.Clamp(_currentZoom, min, max);
        }

        /// <summary>
        /// Enable/disable edge scrolling
        /// </summary>
        public void SetEdgeScroll(bool enabled)
        {
            _enableEdgeScroll = enabled;
        }

        /// <summary>
        /// Focus camera on position
        /// </summary>
        public void FocusOnPosition(Vector3 position, float duration = 0.5f)
        {
            // TODO: Smooth camera movement to position
            Vector3 newPos = position;
            newPos.y = _currentZoom;
            _cameraTransform.position = newPos;
        }
        #endregion
    }
}