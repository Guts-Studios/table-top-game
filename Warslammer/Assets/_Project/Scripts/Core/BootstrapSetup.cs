using UnityEngine;

namespace Warslammer.Core
{
    /// <summary>
    /// Helper script to set up the Bootstrap scene with required managers
    /// Attach this to an empty GameObject in the Bootstrap scene
    /// </summary>
    public class BootstrapSetup : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [Tooltip("Automatically create GameManager on Awake if it doesn't exist")]
        public bool autoCreateGameManager = true;

        private void Awake()
        {
            if (autoCreateGameManager)
            {
                SetupGameManager();
            }
        }

        /// <summary>
        /// Create and configure the GameManager with all required components
        /// </summary>
        [ContextMenu("Setup Game Manager")]
        public void SetupGameManager()
        {
            // Check if GameManager already exists
            GameManager existingManager = FindFirstObjectByType<GameManager>();
            if (existingManager != null)
            {
                Debug.Log("[BootstrapSetup] GameManager already exists in scene");
                return;
            }

            // Create GameManager GameObject
            GameObject managerObject = new GameObject("GameManager");
            
            // Add core manager components
            GameManager gameManager = managerObject.AddComponent<GameManager>();
            TurnManager turnManager = managerObject.AddComponent<TurnManager>();
            PhaseManager phaseManager = managerObject.AddComponent<PhaseManager>();
            SaveManager saveManager = managerObject.AddComponent<SaveManager>();

            // Wire up references
            gameManager.turnManager = turnManager;
            gameManager.phaseManager = phaseManager;
            gameManager.saveManager = saveManager;

            Debug.Log("[BootstrapSetup] GameManager created and configured successfully!");
            Debug.Log("[BootstrapSetup] Core systems initialized:");
            Debug.Log("  - GameManager");
            Debug.Log("  - TurnManager");
            Debug.Log("  - PhaseManager");
            Debug.Log("  - SaveManager");
        }

        /// <summary>
        /// Test the game systems initialization
        /// </summary>
        [ContextMenu("Test System Initialization")]
        public void TestSystemInitialization()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogError("[BootstrapSetup] GameManager not found!");
                return;
            }

            Debug.Log("[BootstrapSetup] System Test Results:");
            Debug.Log($"  - GameManager: {(gm != null ? "✓" : "✗")}");
            Debug.Log($"  - TurnManager: {(gm.turnManager != null ? "✓" : "✗")}");
            Debug.Log($"  - PhaseManager: {(gm.phaseManager != null ? "✓" : "✗")}");
            Debug.Log($"  - SaveManager: {(gm.saveManager != null ? "✓" : "✗")}");
            Debug.Log($"  - Current State: {gm.CurrentState}");
        }
    }
}