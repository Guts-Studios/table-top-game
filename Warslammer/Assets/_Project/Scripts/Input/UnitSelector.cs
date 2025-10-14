using UnityEngine;
using Warslammer.Units;

namespace Warslammer.Input
{
    /// <summary>
    /// Handles unit selection via raycasts
    /// Manages drag-and-drop for unit movement
    /// </summary>
    public class UnitSelector : MonoBehaviour
    {
        #region Properties
        [Header("Selection Settings")]
        [SerializeField]
        [Tooltip("Layer mask for selectable units")]
        private LayerMask _unitLayer;

        [SerializeField]
        [Tooltip("Minimum drag distance to start movement")]
        private float _dragThreshold = 0.5f;

        private Unit _selectedUnit;
        private Unit _hoveredUnit;
        private bool _isDragging;
        private Vector3 _dragStartPosition;
        private Camera _mainCamera;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _mainCamera = Camera.main;
        }
        #endregion

        #region Selection
        /// <summary>
        /// Handle pointer down event
        /// </summary>
        public void OnPointerDown(Vector3 worldPosition)
        {
            // Try to select a unit
            Unit clickedUnit = GetUnitAtPosition(worldPosition);

            if (clickedUnit != null)
            {
                SelectUnit(clickedUnit);
                _dragStartPosition = worldPosition;
            }
            else
            {
                // Clicked empty space - deselect
                DeselectCurrentUnit();
            }
        }

        /// <summary>
        /// Handle pointer move event
        /// </summary>
        public void OnPointerMove(Vector3 worldPosition)
        {
            // Check for hover
            Unit hoveredUnit = GetUnitAtPosition(worldPosition);
            
            if (hoveredUnit != _hoveredUnit)
            {
                if (_hoveredUnit != null && _hoveredUnit != _selectedUnit)
                {
                    _hoveredUnit.OnHoverExit();
                }

                _hoveredUnit = hoveredUnit;

                if (_hoveredUnit != null && _hoveredUnit != _selectedUnit)
                {
                    _hoveredUnit.OnHoverEnter();
                }
            }

            // Handle dragging selected unit
            if (_selectedUnit != null && !_isDragging)
            {
                float dragDistance = Vector3.Distance(worldPosition, _dragStartPosition);
                
                if (dragDistance > _dragThreshold)
                {
                    StartDraggingUnit(worldPosition);
                }
            }

            if (_isDragging && _selectedUnit != null)
            {
                UpdateDraggingUnit(worldPosition);
            }
        }

        /// <summary>
        /// Handle pointer up event
        /// </summary>
        public void OnPointerUp(Vector3 worldPosition)
        {
            if (_isDragging && _selectedUnit != null)
            {
                EndDraggingUnit(worldPosition);
            }

            _isDragging = false;
        }

        /// <summary>
        /// Get unit at world position via raycast
        /// </summary>
        private Unit GetUnitAtPosition(Vector3 worldPosition)
        {
            if (_mainCamera == null)
                return null;

            // Raycast from camera to world position
            Ray ray = _mainCamera.ScreenPointToRay(_mainCamera.WorldToScreenPoint(worldPosition));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, _unitLayer))
            {
                return hit.collider.GetComponent<Unit>();
            }

            return null;
        }

        /// <summary>
        /// Select a unit
        /// </summary>
        private void SelectUnit(Unit unit)
        {
            if (unit == _selectedUnit)
                return;

            // Deselect current unit
            DeselectCurrentUnit();

            // Select new unit
            _selectedUnit = unit;
            
            if (_selectedUnit != null && _selectedUnit.CanBeSelected)
            {
                _selectedUnit.OnSelected();
            }
        }

        /// <summary>
        /// Deselect current unit
        /// </summary>
        private void DeselectCurrentUnit()
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.OnDeselected();
                _selectedUnit = null;
            }
        }

        /// <summary>
        /// Get currently selected unit
        /// </summary>
        public Unit GetSelectedUnit()
        {
            return _selectedUnit;
        }
        #endregion

        #region Dragging
        /// <summary>
        /// Start dragging the selected unit
        /// </summary>
        private void StartDraggingUnit(Vector3 worldPosition)
        {
            if (_selectedUnit == null || !_selectedUnit.CanMove)
                return;

            _isDragging = true;

            UnitMovement movement = _selectedUnit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.StartDrag();
            }

            Debug.Log($"[UnitSelector] Started dragging {_selectedUnit.name}");
        }

        /// <summary>
        /// Update unit drag position
        /// </summary>
        private void UpdateDraggingUnit(Vector3 worldPosition)
        {
            if (_selectedUnit == null)
                return;

            UnitMovement movement = _selectedUnit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.UpdateDrag(worldPosition);
            }
        }

        /// <summary>
        /// End dragging and attempt to move unit
        /// </summary>
        private void EndDraggingUnit(Vector3 worldPosition)
        {
            if (_selectedUnit == null)
                return;

            UnitMovement movement = _selectedUnit.GetComponent<UnitMovement>();
            if (movement != null)
            {
                movement.EndDrag(worldPosition);
            }

            Debug.Log($"[UnitSelector] Ended dragging {_selectedUnit.name}");
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set unit layer mask
        /// </summary>
        public void SetUnitLayer(LayerMask layer)
        {
            _unitLayer = layer;
        }

        /// <summary>
        /// Set drag threshold
        /// </summary>
        public void SetDragThreshold(float threshold)
        {
            _dragThreshold = threshold;
        }
        #endregion
    }
}