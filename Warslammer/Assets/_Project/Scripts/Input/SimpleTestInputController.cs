using UnityEngine;
using UnityEngine.InputSystem;
using Warslammer.Units;
using Warslammer.Battlefield;

namespace Warslammer.Input
{
    /// <summary>
    /// Simple temporary input controller using new Input System for testing movement
    /// Handles unit selection, movement, and basic camera controls
    /// </summary>
    public class SimpleTestInputController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _panSpeed = 10f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 50f;

        [Header("Unit Selection")]
        [SerializeField] private LayerMask _unitLayerMask = -1; // All layers by default

        private Unit _selectedUnit;
        private bool _isDraggingUnit;
        private Vector3 _dragStartPosition;
        private MeasurementTool _measurementTool;

        private void Start()
        {
            // Get or find camera
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            // Find measurement tool
            _measurementTool = FindFirstObjectByType<MeasurementTool>();

            Debug.Log("[SimpleTestInputController] Initialized. Click units to select, drag to move.");
            Debug.Log("[SimpleTestInputController] WASD = Pan Camera, Mouse Wheel = Zoom");
        }

        private void Update()
        {
            HandleCameraControls();
            HandleUnitSelection();
            HandleUnitDragging();
        }

        private void HandleCameraControls()
        {
            if (_camera == null) return;

            // WASD for panning
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Vector3 moveDir = Vector3.zero;

                if (keyboard.wKey.isPressed) moveDir += Vector3.forward;
                if (keyboard.sKey.isPressed) moveDir += Vector3.back;
                if (keyboard.aKey.isPressed) moveDir += Vector3.left;
                if (keyboard.dKey.isPressed) moveDir += Vector3.right;

                if (moveDir != Vector3.zero)
                {
                    _camera.transform.position += moveDir.normalized * _panSpeed * Time.deltaTime;
                }
            }

            // Mouse wheel for zoom
            var mouse = Mouse.current;
            if (mouse != null)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    Vector3 pos = _camera.transform.position;
                    pos.y = Mathf.Clamp(pos.y - scroll * _zoomSpeed * Time.deltaTime, _minZoom, _maxZoom);
                    _camera.transform.position = pos;
                }
            }
        }

        private void HandleUnitSelection()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Left click to select unit
            if (mouse.leftButton.wasPressedThisFrame && !_isDraggingUnit)
            {
                Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _unitLayerMask))
                {
                    Unit clickedUnit = hit.collider.GetComponent<Unit>();

                    if (clickedUnit != null)
                    {
                        SelectUnit(clickedUnit);
                    }
                    else
                    {
                        DeselectUnit();
                    }
                }
                else
                {
                    DeselectUnit();
                }
            }
        }

        private void HandleUnitDragging()
        {
            var mouse = Mouse.current;
            if (mouse == null || _selectedUnit == null) return;

            // Start dragging
            if (mouse.leftButton.wasPressedThisFrame && _selectedUnit != null)
            {
                Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _unitLayerMask))
                {
                    Unit clickedUnit = hit.collider.GetComponent<Unit>();

                    if (clickedUnit == _selectedUnit)
                    {
                        _isDraggingUnit = true;
                        _dragStartPosition = _selectedUnit.transform.position;

                        Debug.Log($"[SimpleTestInputController] Started dragging {_selectedUnit.name}");
                    }
                }
            }

            // During drag - show measurement line
            if (_isDraggingUnit && mouse.leftButton.isPressed)
            {
                Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    Vector3 targetPosition = hit.point;
                    targetPosition.y = _dragStartPosition.y; // Keep same height

                    // Show measurement tool
                    if (_measurementTool != null)
                    {
                        _measurementTool.ShowMeasurement(_selectedUnit, targetPosition);
                    }

                    // Show ghost/preview of unit at target position (optional - for now just draw debug)
                    Debug.DrawLine(_dragStartPosition, targetPosition, Color.cyan);
                }
            }

            // Release - execute movement
            if (_isDraggingUnit && mouse.leftButton.wasReleasedThisFrame)
            {
                Ray ray = _camera.ScreenPointToRay(mouse.position.ReadValue());

                if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
                {
                    Vector3 targetPosition = hit.point;
                    targetPosition.y = _dragStartPosition.y;

                    TryMoveUnit(_selectedUnit, targetPosition);
                }

                _isDraggingUnit = false;

                // Hide measurement tool
                if (_measurementTool != null)
                {
                    _measurementTool.Hide();
                }

                Debug.Log($"[SimpleTestInputController] Stopped dragging {_selectedUnit.name}");
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (_selectedUnit == unit) return;

            // Deselect previous
            if (_selectedUnit != null)
            {
                _selectedUnit.OnDeselected();
            }

            // Select new
            _selectedUnit = unit;
            _selectedUnit.OnSelected();

            Debug.Log($"[SimpleTestInputController] Selected unit: {unit.name}");
        }

        private void DeselectUnit()
        {
            if (_selectedUnit == null) return;

            _selectedUnit.OnDeselected();

            Debug.Log($"[SimpleTestInputController] Deselected unit: {_selectedUnit.name}");
            _selectedUnit = null;
        }

        private void TryMoveUnit(Unit unit, Vector3 targetPosition)
        {
            if (unit == null) return;

            var movement = unit.GetComponent<UnitMovement>();
            if (movement == null)
            {
                Debug.LogWarning($"[SimpleTestInputController] Unit {unit.name} has no UnitMovement component!");
                return;
            }

            float distance = Vector3.Distance(unit.transform.position, targetPosition);

            // Check if within movement range
            if (distance > unit.RemainingMovement)
            {
                Debug.LogWarning($"[SimpleTestInputController] Target too far! Distance: {distance:F1}\", Remaining Movement: {unit.RemainingMovement:F1}\"");
                return;
            }

            // Attempt movement
            movement.MoveTo(targetPosition);
            Debug.Log($"[SimpleTestInputController] Unit {unit.name} moved to {targetPosition}. Remaining movement: {unit.RemainingMovement:F1}\"");
        }

        private void OnGUI()
        {
            // Simple on-screen instructions
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            string instructions = "CONTROLS:\n" +
                                "- Click unit to select\n" +
                                "- Click & drag to move selected unit\n" +
                                "- WASD to pan camera\n" +
                                "- Mouse wheel to zoom\n";

            if (_selectedUnit != null)
            {
                instructions += $"\nSelected: {_selectedUnit.name}\n" +
                              $"Remaining Movement: {_selectedUnit.RemainingMovement:F1}\"";
            }

            GUI.Label(new Rect(10, 10, 300, 200), instructions, style);
        }
    }
}
