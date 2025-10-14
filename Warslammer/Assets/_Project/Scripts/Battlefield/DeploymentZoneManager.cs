using System.Collections.Generic;
using UnityEngine;
using Warslammer.Utilities;

namespace Warslammer.Battlefield
{
    /// <summary>
    /// Manages deployment zones for army placement at the start of battle
    /// Handles zone validation and visualization
    /// </summary>
    public class DeploymentZoneManager : MonoBehaviour
    {
        #region Deployment Zone
        /// <summary>
        /// Represents a deployment zone for a player
        /// </summary>
        [System.Serializable]
        public class DeploymentZone
        {
            public int playerIndex;
            public Vector3 center;
            public float widthInches;
            public float depthInches;
            public Color zoneColor = Color.blue;

            public DeploymentZone(int player, Vector3 centerPos, float width, float depth)
            {
                playerIndex = player;
                center = centerPos;
                widthInches = width;
                depthInches = depth;
            }

            /// <summary>
            /// Get minimum bounds of this zone
            /// </summary>
            public Vector3 MinBounds => center - new Vector3(
                MeasurementUtility.InchesToUnityUnits(widthInches / 2f),
                0,
                MeasurementUtility.InchesToUnityUnits(depthInches / 2f)
            );

            /// <summary>
            /// Get maximum bounds of this zone
            /// </summary>
            public Vector3 MaxBounds => center + new Vector3(
                MeasurementUtility.InchesToUnityUnits(widthInches / 2f),
                0,
                MeasurementUtility.InchesToUnityUnits(depthInches / 2f)
            );

            /// <summary>
            /// Check if a position is within this deployment zone
            /// </summary>
            public bool ContainsPosition(Vector3 position)
            {
                Vector3 min = MinBounds;
                Vector3 max = MaxBounds;

                return position.x >= min.x && position.x <= max.x &&
                       position.z >= min.z && position.z <= max.z;
            }
        }
        #endregion

        #region Properties
        [Header("Deployment Configuration")]
        [SerializeField]
        [Tooltip("Deployment zone depth in inches (from battlefield edge)")]
        private float _deploymentDepthInches = 12f;

        /// <summary>
        /// Deployment zone depth in inches
        /// </summary>
        public float DeploymentDepthInches => _deploymentDepthInches;

        [SerializeField]
        [Tooltip("List of deployment zones")]
        private List<DeploymentZone> _deploymentZones = new List<DeploymentZone>();

        /// <summary>
        /// List of deployment zones
        /// </summary>
        public List<DeploymentZone> DeploymentZones => _deploymentZones;

        [Header("Visualization")]
        [SerializeField]
        [Tooltip("Show deployment zones in scene view")]
        private bool _showZonesInEditor = true;

        [SerializeField]
        [Tooltip("Show deployment zones in game")]
        private bool _showZonesInGame = false;

        private BattlefieldManager _battlefieldManager;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _battlefieldManager = GetComponent<BattlefieldManager>();
        }

        private void Start()
        {
            // Create default deployment zones if none exist
            if (_deploymentZones.Count == 0)
            {
                CreateDefaultDeploymentZones();
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showZonesInEditor)
                return;

            // Draw deployment zones
            foreach (DeploymentZone zone in _deploymentZones)
            {
                DrawDeploymentZone(zone);
            }
        }
        #endregion

        #region Zone Management
        /// <summary>
        /// Create default deployment zones (one on each side)
        /// </summary>
        private void CreateDefaultDeploymentZones()
        {
            if (_battlefieldManager == null)
                return;

            _deploymentZones.Clear();

            float battlefieldWidth = _battlefieldManager.BattlefieldWidthInches;
            float battlefieldDepth = _battlefieldManager.BattlefieldDepthInches;
            Vector3 center = _battlefieldManager.BattlefieldCenter;

            // Player 1 zone (bottom of battlefield)
            float zoneDepthUnits = MeasurementUtility.InchesToUnityUnits(_deploymentDepthInches);
            Vector3 player1Center = center - new Vector3(0, 0, MeasurementUtility.InchesToUnityUnits(battlefieldDepth / 2f - _deploymentDepthInches / 2f));
            
            DeploymentZone player1Zone = new DeploymentZone(0, player1Center, battlefieldWidth, _deploymentDepthInches);
            player1Zone.zoneColor = new Color(0.2f, 0.4f, 1f, 0.3f); // Blue
            _deploymentZones.Add(player1Zone);

            // Player 2 zone (top of battlefield)
            Vector3 player2Center = center + new Vector3(0, 0, MeasurementUtility.InchesToUnityUnits(battlefieldDepth / 2f - _deploymentDepthInches / 2f));
            
            DeploymentZone player2Zone = new DeploymentZone(1, player2Center, battlefieldWidth, _deploymentDepthInches);
            player2Zone.zoneColor = new Color(1f, 0.2f, 0.2f, 0.3f); // Red
            _deploymentZones.Add(player2Zone);

            Debug.Log($"[DeploymentZoneManager] Created {_deploymentZones.Count} deployment zones");
        }

        /// <summary>
        /// Add a custom deployment zone
        /// </summary>
        /// <param name="zone">Deployment zone to add</param>
        public void AddDeploymentZone(DeploymentZone zone)
        {
            if (zone == null)
                return;

            _deploymentZones.Add(zone);
        }

        /// <summary>
        /// Remove a deployment zone
        /// </summary>
        /// <param name="playerIndex">Player index of zone to remove</param>
        public void RemoveDeploymentZone(int playerIndex)
        {
            _deploymentZones.RemoveAll(z => z.playerIndex == playerIndex);
        }

        /// <summary>
        /// Clear all deployment zones
        /// </summary>
        public void ClearDeploymentZones()
        {
            _deploymentZones.Clear();
        }
        #endregion

        #region Zone Queries
        /// <summary>
        /// Get deployment zone for a specific player
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <returns>Deployment zone for player, or null if not found</returns>
        public DeploymentZone GetDeploymentZone(int playerIndex)
        {
            foreach (DeploymentZone zone in _deploymentZones)
            {
                if (zone.playerIndex == playerIndex)
                    return zone;
            }

            return null;
        }

        /// <summary>
        /// Check if a position is valid for deployment by a specific player
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="playerIndex">Player index</param>
        /// <returns>True if position is valid for deployment</returns>
        public bool IsValidDeploymentPosition(Vector3 position, int playerIndex)
        {
            DeploymentZone zone = GetDeploymentZone(playerIndex);
            
            if (zone == null)
            {
                Debug.LogWarning($"[DeploymentZoneManager] No deployment zone found for player {playerIndex}");
                return false;
            }

            return zone.ContainsPosition(position);
        }

        /// <summary>
        /// Get a random position within a player's deployment zone
        /// </summary>
        /// <param name="playerIndex">Player index</param>
        /// <returns>Random position in zone</returns>
        public Vector3 GetRandomPositionInZone(int playerIndex)
        {
            DeploymentZone zone = GetDeploymentZone(playerIndex);
            
            if (zone == null)
            {
                Debug.LogWarning($"[DeploymentZoneManager] No deployment zone found for player {playerIndex}");
                return Vector3.zero;
            }

            Vector3 min = zone.MinBounds;
            Vector3 max = zone.MaxBounds;

            return new Vector3(
                Random.Range(min.x, max.x),
                0,
                Random.Range(min.z, max.z)
            );
        }

        /// <summary>
        /// Clamp a position to a player's deployment zone
        /// </summary>
        /// <param name="position">Position to clamp</param>
        /// <param name="playerIndex">Player index</param>
        /// <returns>Clamped position</returns>
        public Vector3 ClampToDeploymentZone(Vector3 position, int playerIndex)
        {
            DeploymentZone zone = GetDeploymentZone(playerIndex);
            
            if (zone == null)
                return position;

            Vector3 min = zone.MinBounds;
            Vector3 max = zone.MaxBounds;

            return new Vector3(
                Mathf.Clamp(position.x, min.x, max.x),
                position.y,
                Mathf.Clamp(position.z, min.z, max.z)
            );
        }
        #endregion

        #region Visualization
        /// <summary>
        /// Draw a deployment zone in the scene
        /// </summary>
        /// <param name="zone">Zone to draw</param>
        private void DrawDeploymentZone(DeploymentZone zone)
        {
            if (zone == null)
                return;

            Gizmos.color = zone.zoneColor;

            Vector3 min = zone.MinBounds;
            Vector3 max = zone.MaxBounds;

            // Draw filled rectangle
            Vector3[] corners = new Vector3[]
            {
                new Vector3(min.x, 0.1f, min.z),
                new Vector3(max.x, 0.1f, min.z),
                new Vector3(max.x, 0.1f, max.z),
                new Vector3(min.x, 0.1f, max.z)
            };

            // Draw filled quad
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }

            // Draw grid pattern
            Gizmos.color = zone.zoneColor.WithAlpha(0.5f);
            float gridSpacing = MeasurementUtility.InchesToUnityUnits(6f); // 6" grid

            // Draw horizontal lines
            for (float z = min.z; z <= max.z; z += gridSpacing)
            {
                Gizmos.DrawLine(new Vector3(min.x, 0.1f, z), new Vector3(max.x, 0.1f, z));
            }

            // Draw vertical lines
            for (float x = min.x; x <= max.x; x += gridSpacing)
            {
                Gizmos.DrawLine(new Vector3(x, 0.1f, min.z), new Vector3(x, 0.1f, max.z));
            }
        }

        /// <summary>
        /// Toggle deployment zone visualization in game
        /// </summary>
        /// <param name="show">Show zones?</param>
        public void SetZoneVisualization(bool show)
        {
            _showZonesInGame = show;
            // TODO: Phase 3 - Implement runtime visualization with mesh renderer
        }
        #endregion

        #region Configuration
        /// <summary>
        /// Set deployment zone depth
        /// </summary>
        /// <param name="depthInches">Depth in inches</param>
        public void SetDeploymentDepth(float depthInches)
        {
            _deploymentDepthInches = depthInches;
            CreateDefaultDeploymentZones();
        }
        #endregion
    }
}