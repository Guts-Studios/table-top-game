using UnityEngine;
using UnityEngine.Events;
using Warslammer.Core;

namespace Warslammer.Input
{
    /// <summary>
    /// Central input handler for mouse and touch input
    /// Manages input events and delegates to appropriate systems
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region Singleton
        private static InputManager _instance;
        
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<InputManager>();
                }
                return _instance;
            }
        }
        #endregion

        #region Events
        public UnityEvent<Vector3> OnPointerDown = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> OnPointerMove = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> OnPointerUp = new UnityEvent<Vector3>();
        public UnityEvent<Vector2> OnPanInput = new UnityEvent<Vector2>();
        public UnityEvent<float> OnZoomInput = new UnityEvent<float>();
        public UnityEvent<float> OnRotateInput = new UnityEvent<float>();
        #endregion

        #region Properties
        [Header("Input Settings")]
        [SerializeField]
        [Tooltip("Layer mask for raycast targets")]
        private LayerMask _raycastLayers = -1;

        [SerializeField]
        [Tooltip("Camera for raycasting")]
        private Camera _mainCamera;

        [Header("Touch Settings")]
        [SerializeField]
        [Tooltip("Enable touch input")]
        private bool _enableTouch = true;

        [SerializeField]
        [Tooltip("Touch drag threshold")]
        private float _touchDragThreshold = 10f;

        private bool _isPointerDown;
        private Vector3 _pointerDownPosition;
        private Vector3 _lastPointerPosition;
        private UnitSelector _unitSelector;
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

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            _unitSelector = GetComponent<UnitSelector>();
        }

        private void Update()
        {
            HandleMouseInput();
            
            if (_enableTouch)
            {
                HandleTouchInput();
            }

            HandleKeyboardInput();
        }
        #endregion

        #region Mouse Input
        private void HandleMouseInput()
        {
            // Mouse button down
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _isPointerDown = true;
                _pointerDownPosition = UnityEngine.Input.mousePosition;
                _lastPointerPosition = _pointerDownPosition;

                Vector3 worldPos = GetWorldPosition(UnityEngine.Input.mousePosition);
                OnPointerDown?.Invoke(worldPos);

                // Delegate to unit selector
                _unitSelector?.OnPointerDown(worldPos);
            }

            // Mouse move
            if (UnityEngine.Input.GetMouseButton(0) && _isPointerDown)
            {
                Vector3 currentPos = UnityEngine.Input.mousePosition;
                if (Vector3.Distance(currentPos, _lastPointerPosition) > 0.1f)
                {
                    Vector3 worldPos = GetWorldPosition(currentPos);
                    OnPointerMove?.Invoke(worldPos);

                    _unitSelector?.OnPointerMove(worldPos);
                    _lastPointerPosition = currentPos;
                }
            }

            // Mouse button up
            if (UnityEngine.Input.GetMouseButtonUp(0) && _isPointerDown)
            {
                _isPointerDown = false;
                Vector3 worldPos = GetWorldPosition(UnityEngine.Input.mousePosition);
                OnPointerUp?.Invoke(worldPos);

                _unitSelector?.OnPointerUp(worldPos);
            }

            // Mouse wheel for zoom
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                OnZoomInput?.Invoke(scroll);
            }
        }
        #endregion

        #region Touch Input
        private void HandleTouchInput()
        {
            if (UnityEngine.Input.touchCount == 0)
                return;

            Touch touch = UnityEngine.Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _isPointerDown = true;
                    _pointerDownPosition = touch.position;
                    Vector3 worldPos = GetWorldPosition(touch.position);
                    OnPointerDown?.Invoke(worldPos);
                    _unitSelector?.OnPointerDown(worldPos);
                    break;

                case TouchPhase.Moved:
                    if (_isPointerDown)
                    {
                        // Check if drag exceeds threshold
                        float dragDistance = Vector2.Distance(touch.position, _pointerDownPosition);
                        if (dragDistance > _touchDragThreshold)
                        {
                            Vector3 currentWorldPos = GetWorldPosition(touch.position);
                            OnPointerMove?.Invoke(currentWorldPos);
                            _unitSelector?.OnPointerMove(currentWorldPos);
                        }
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if (_isPointerDown)
                    {
                        _isPointerDown = false;
                        Vector3 endWorldPos = GetWorldPosition(touch.position);
                        OnPointerUp?.Invoke(endWorldPos);
                        _unitSelector?.OnPointerUp(endWorldPos);
                    }
                    break;
            }

            // Two-finger gestures for zoom/rotate
            if (UnityEngine.Input.touchCount == 2)
            {
                HandleTwoFingerGestures();
            }
        }

        private void HandleTwoFingerGestures()
        {
            Touch touch0 = UnityEngine.Input.GetTouch(0);
            Touch touch1 = UnityEngine.Input.GetTouch(1);

            // Pinch zoom
            Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

            float prevMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
            float currentMagnitude = (touch0.position - touch1.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;

            OnZoomInput?.Invoke(difference * 0.01f);

            // Two-finger rotation (optional)
            // TODO: Implement rotation gesture
        }
        #endregion

        #region Keyboard Input
        private void HandleKeyboardInput()
        {
            // WASD camera panning
            Vector2 panInput = Vector2.zero;

            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
                panInput.y += 1f;
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
                panInput.y -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                panInput.x -= 1f;
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
                panInput.x += 1f;

            if (panInput.sqrMagnitude > 0.01f)
            {
                OnPanInput?.Invoke(panInput.normalized);
            }

            // Q/E for rotation
            float rotateInput = 0f;
            if (UnityEngine.Input.GetKey(KeyCode.Q))
                rotateInput = -1f;
            if (UnityEngine.Input.GetKey(KeyCode.E))
                rotateInput = 1f;

            if (Mathf.Abs(rotateInput) > 0.01f)
            {
                OnRotateInput?.Invoke(rotateInput);
            }
        }
        #endregion

        #region Raycasting
        public Vector3 GetWorldPosition(Vector3 screenPosition)
        {
            if (_mainCamera == null)
                return Vector3.zero;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        public T RaycastForComponent<T>(Vector3 screenPosition) where T : Component
        {
            if (_mainCamera == null)
                return null;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, _raycastLayers))
            {
                return hit.collider.GetComponent<T>();
            }

            return null;
        }
        #endregion
    }
}