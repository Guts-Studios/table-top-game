using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;
using Warslammer.Data;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Manages terrain objects and their effects on the battlefield
    /// Handles terrain placement, queries, and movement modifiers
    /// </summary>
    public class TerrainManager : MonoBehaviour
    {
        #region Terrain Object
        /// <summary>
        /// Represents a single terrain feature on the battlefield
        /// </summary>
        [System.Serializable]
        public class TerrainObject
        {
            public string terrainName;
            public TerrainType terrainType;
            public Transform transform;
            public Collider terrainCollider;
            public TerrainTypeData terrainData;
            
            public TerrainObject(string name, TerrainType type, Transform trans, Collider collider, TerrainTypeData data)
            {
                terrainName = name;
                terrainType = type;
                transform = trans;
                terrainCollider = collider;
                terrainData = data;
            }
        }
        #endregion

        #region Properties
        [Header("Terrain Objects")]
        [SerializeField]
        [Tooltip("List of all terrain objects on the battlefield")]
        private List<TerrainObject> _terrainObjects = new List<TerrainObject>();

        /// <summary>
        /// List of all terrain objects on the battlefield
        /// </summary>
        public List<TerrainObject> TerrainObjects => _terrainObjects;

        [Header("Terrain Layer")]
        [SerializeField]
        [Tooltip("Layer used for terrain objects")]
        private LayerMask _terrainLayer;

        /// <summary>
        /// Layer used for terrain objects
        /// </summary>
        public LayerMask TerrainLayer => _terrainLayer;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Find all existing terrain objects in the scene
            FindTerrainObjects();
        }

        private void OnDrawGizmos()
        {
            // Visualize terrain objects
            foreach (TerrainObject terrainObj in _terrainObjects)
            {
                if (terrainObj.transform == null)
                    continue;

                // Color code by terrain type
                Gizmos.color = GetTerrainColor(terrainObj.terrainType);
                
                if (terrainObj.terrainCollider != null)
                {
                    Gizmos.DrawWireCube(terrainObj.terrainCollider.bounds.center, terrainObj.terrainCollider.bounds.size);
                }
            }
        }
        #endregion

        #region Terrain Management
        /// <summary>
        /// Find all terrain objects in the scene
        /// </summary>
        private void FindTerrainObjects()
        {
            _terrainObjects.Clear();

            // Try to find all GameObjects tagged as "Terrain"
            try
            {
                GameObject[] terrainGOs = GameObject.FindGameObjectsWithTag("Terrain");

                foreach (GameObject terrainGO in terrainGOs)
                {
                    RegisterTerrainObject(terrainGO.transform);
                }

                Debug.Log($"[TerrainManager] Found {_terrainObjects.Count} terrain objects");
            }
            catch (UnityException)
            {
                // "Terrain" tag doesn't exist - this is OK for a test scene without terrain
                Debug.LogWarning("[TerrainManager] 'Terrain' tag not defined. No terrain objects will be loaded. Add the tag in Project Settings > Tags and Layers if you want terrain features.");
            }
        }

        /// <summary>
        /// Register a terrain object
        /// </summary>
        /// <param name="terrainTransform">Transform of the terrain object</param>
        public void RegisterTerrainObject(Transform terrainTransform)
        {
            if (terrainTransform == null)
                return;

            // Get collider
            Collider terrainCollider = terrainTransform.GetComponent<Collider>();
            if (terrainCollider == null)
            {
                Debug.LogWarning($"[TerrainManager] Terrain object {terrainTransform.name} has no collider!");
                return;
            }

            // Try to get terrain data from a component
            TerrainTypeData terrainData = null;
            // TODO: Create a TerrainObject component to hold terrain data reference
            
            // Default to Open terrain if no data
            TerrainType terrainType = TerrainType.Open;
            
            // Check name for terrain type hints
            string name = terrainTransform.name.ToLower();
            if (name.Contains("forest") || name.Contains("tree"))
                terrainType = TerrainType.Forest;
            else if (name.Contains("hill") || name.Contains("elevation"))
                terrainType = TerrainType.Hill;
            else if (name.Contains("building") || name.Contains("structure"))
                terrainType = TerrainType.Building;
            else if (name.Contains("water") || name.Contains("river"))
                terrainType = TerrainType.Water;
            else if (name.Contains("obstacle") || name.Contains("rock"))
                terrainType = TerrainType.Obstacle;
            else if (name.Contains("impassable") || name.Contains("wall"))
                terrainType = TerrainType.Impassable;

            TerrainObject terrainObj = new TerrainObject(
                terrainTransform.name,
                terrainType,
                terrainTransform,
                terrainCollider,
                terrainData
            );

            _terrainObjects.Add(terrainObj);
            Debug.Log($"[TerrainManager] Registered terrain object: {terrainTransform.name} ({terrainType})");
        }

        /// <summary>
        /// Remove a terrain object from management
        /// </summary>
        /// <param name="terrainTransform">Transform of the terrain object to remove</param>
        public void UnregisterTerrainObject(Transform terrainTransform)
        {
            _terrainObjects.RemoveAll(t => t.transform == terrainTransform);
        }

        /// <summary>
        /// Clear all terrain objects
        /// </summary>
        public void ClearAllTerrain()
        {
            _terrainObjects.Clear();
        }
        #endregion

        #region Terrain Queries
        /// <summary>
        /// Get terrain type at a specific position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Terrain type at position (Open if none found)</returns>
        public TerrainType GetTerrainTypeAtPosition(Vector3 position)
        {
            // Raycast downward to check for terrain
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 20f, _terrainLayer))
            {
                // Find terrain object at this position
                foreach (TerrainObject terrainObj in _terrainObjects)
                {
                    if (terrainObj.transform == hit.transform)
                    {
                        return terrainObj.terrainType;
                    }
                }
            }

            return TerrainType.Open;
        }

        /// <summary>
        /// Check if a position is on impassable terrain
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>True if position is impassable</returns>
        public bool IsPositionImpassable(Vector3 position)
        {
            TerrainType terrainType = GetTerrainTypeAtPosition(position);
            return terrainType == TerrainType.Impassable;
        }

        /// <summary>
        /// Get movement modifier for terrain at position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Movement cost multiplier (1.0 = normal, 2.0 = double cost, etc.)</returns>
        public float GetMovementModifier(Vector3 position)
        {
            TerrainType terrainType = GetTerrainTypeAtPosition(position);
            
            // TODO: Phase 3 - Load from TerrainTypeData ScriptableObjects
            return terrainType switch
            {
                TerrainType.Open => 1.0f,
                TerrainType.Forest => 1.5f,
                TerrainType.Hill => 1.5f,
                TerrainType.Building => 1.0f,
                TerrainType.Water => 2.0f,
                TerrainType.Obstacle => 1.5f,
                TerrainType.Impassable => float.MaxValue,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Get cover bonus for terrain at position
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <returns>Cover bonus value</returns>
        public int GetCoverBonus(Vector3 position)
        {
            TerrainType terrainType = GetTerrainTypeAtPosition(position);
            
            // TODO: Phase 3 - Implement proper cover system
            return terrainType switch
            {
                TerrainType.Forest => 1,
                TerrainType.Building => 2,
                TerrainType.Obstacle => 1,
                _ => 0
            };
        }

        /// <summary>
        /// Check if terrain blocks line of sight between two positions
        /// </summary>
        /// <param name="from">Start position</param>
        /// <param name="to">End position</param>
        /// <returns>True if terrain blocks LOS</returns>
        public bool TerrainBlocksLineOfSight(Vector3 from, Vector3 to)
        {
            // Raycast between positions
            Vector3 direction = to - from;
            float distance = direction.magnitude;
            
            RaycastHit hit;
            if (Physics.Raycast(from, direction.normalized, out hit, distance, _terrainLayer))
            {
                // Check if hit terrain is blocking type
                foreach (TerrainObject terrainObj in _terrainObjects)
                {
                    if (terrainObj.transform == hit.transform)
                    {
                        // Buildings and impassable terrain block LOS
                        return terrainObj.terrainType == TerrainType.Building || 
                               terrainObj.terrainType == TerrainType.Impassable;
                    }
                }
            }

            return false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get a color for visualizing terrain type
        /// </summary>
        /// <param name="terrainType">Terrain type</param>
        /// <returns>Color for this terrain type</returns>
        private Color GetTerrainColor(TerrainType terrainType)
        {
            return terrainType switch
            {
                TerrainType.Open => Color.green,
                TerrainType.Forest => new Color(0.1f, 0.5f, 0.1f),
                TerrainType.Hill => new Color(0.6f, 0.4f, 0.2f),
                TerrainType.Building => Color.gray,
                TerrainType.Water => Color.blue,
                TerrainType.Obstacle => Color.yellow,
                TerrainType.Impassable => Color.red,
                _ => Color.white
            };
        }

        /// <summary>
        /// Get all terrain objects of a specific type
        /// </summary>
        /// <param name="terrainType">Type to filter by</param>
        /// <returns>List of terrain objects of this type</returns>
        public List<TerrainObject> GetTerrainObjectsByType(TerrainType terrainType)
        {
            List<TerrainObject> filtered = new List<TerrainObject>();
            
            foreach (TerrainObject terrainObj in _terrainObjects)
            {
                if (terrainObj.terrainType == terrainType)
                {
                    filtered.Add(terrainObj);
                }
            }

            return filtered;
        }
        #endregion
    }
}