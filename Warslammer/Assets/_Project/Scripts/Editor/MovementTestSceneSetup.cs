using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Warslammer.Core;
using Warslammer.Battlefield;
using Warslammer.Input;
using Warslammer.Units;
using Warslammer.Data;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to automatically set up the MovementTest scene.
    /// Menu: Warslammer > Setup > Complete MovementTest Scene
    /// </summary>
    public static class MovementTestSceneSetup
    {
        [MenuItem("Warslammer/Setup/Complete MovementTest Scene")]
        public static void SetupMovementTestScene()
        {
            // Verify we're in the correct scene
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != "MovementTest")
            {
                bool openScene = EditorUtility.DisplayDialog(
                    "Wrong Scene",
                    "This tool is designed for the MovementTest scene. Would you like to open it now?",
                    "Yes, Open MovementTest",
                    "Cancel"
                );

                if (openScene)
                {
                    activeScene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Testing/MovementTest.unity");
                }
                else
                {
                    return;
                }
            }

            Debug.Log("=== Starting MovementTest Scene Setup ===");

            // Step 1: Find or create GameManager
            GameObject gameManagerObj = SetupGameManager();

            // Step 2: Link references in existing GameObjects
            LinkBattlefieldReferences();
            LinkInputReferences();
            LinkCameraReferences();

            // Step 3: Create test units
            CreateTestUnits();

            // Mark scene as dirty so it saves
            EditorSceneManager.MarkSceneDirty(activeScene);

            Debug.Log("=== MovementTest Scene Setup Complete! ===");
            EditorUtility.DisplayDialog(
                "Setup Complete",
                "MovementTest scene has been set up successfully!\n\n" +
                "Next Steps:\n" +
                "1. Save the scene (Ctrl+S)\n" +
                "2. Press Play to test movement system\n" +
                "3. Click on units to select them\n" +
                "4. Drag selected units to move them",
                "OK"
            );
        }

        private static GameObject SetupGameManager()
        {
            Debug.Log("[1/4] Setting up GameManager...");

            GameObject gameManagerObj = GameObject.Find("GameManager");

            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("GameManager");
                Debug.Log("  - Created GameManager GameObject");
            }

            // Add components if they don't exist
            var gameManager = gameManagerObj.GetComponent<GameManager>();
            if (gameManager == null)
            {
                gameManager = gameManagerObj.AddComponent<GameManager>();
                Debug.Log("  - Added GameManager component");
            }

            var turnManager = gameManagerObj.GetComponent<TurnManager>();
            if (turnManager == null)
            {
                turnManager = gameManagerObj.AddComponent<TurnManager>();
                Debug.Log("  - Added TurnManager component");
            }

            var phaseManager = gameManagerObj.GetComponent<PhaseManager>();
            if (phaseManager == null)
            {
                phaseManager = gameManagerObj.AddComponent<PhaseManager>();
                Debug.Log("  - Added PhaseManager component");
            }

            // Link references using SerializedObject for proper Unity serialization
            SerializedObject so = new SerializedObject(gameManager);
            SerializedProperty turnManagerProp = so.FindProperty("turnManager");
            SerializedProperty phaseManagerProp = so.FindProperty("phaseManager");

            if (turnManagerProp != null)
            {
                turnManagerProp.objectReferenceValue = turnManager;
            }
            else
            {
                Debug.LogWarning("  - Could not find 'turnManager' property, using direct assignment");
                gameManager.turnManager = turnManager;
            }

            if (phaseManagerProp != null)
            {
                phaseManagerProp.objectReferenceValue = phaseManager;
            }
            else
            {
                Debug.LogWarning("  - Could not find 'phaseManager' property, using direct assignment");
                gameManager.phaseManager = phaseManager;
            }

            so.ApplyModifiedProperties();

            Debug.Log("  - Linked TurnManager and PhaseManager references");

            return gameManagerObj;
        }

        private static void LinkBattlefieldReferences()
        {
            Debug.Log("[2/4] Linking Battlefield component references...");

            GameObject battlefieldObj = GameObject.Find("Battlefield");
            if (battlefieldObj == null)
            {
                Debug.LogError("  - Battlefield GameObject not found!");
                return;
            }

            var battlefieldManager = battlefieldObj.GetComponent<BattlefieldManager>();
            var terrainManager = battlefieldObj.GetComponent<TerrainManager>();
            var deploymentZoneManager = battlefieldObj.GetComponent<DeploymentZoneManager>();
            var losManager = battlefieldObj.GetComponent<LineOfSightManager>();

            if (battlefieldManager != null)
            {
                SerializedObject so = new SerializedObject(battlefieldManager);
                so.FindProperty("_terrainManager").objectReferenceValue = terrainManager;
                so.FindProperty("_deploymentZoneManager").objectReferenceValue = deploymentZoneManager;
                so.FindProperty("_lineOfSightManager").objectReferenceValue = losManager;
                so.ApplyModifiedProperties();

                Debug.Log("  - Linked Battlefield manager references");
            }
        }

        private static void LinkInputReferences()
        {
            Debug.Log("[3/4] Setting up Input System...");

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("  - Main Camera not found!");
                return;
            }

            // Disable old Input System components (they use UnityEngine.Input which conflicts with new Input System)
            var inputManager = Object.FindFirstObjectByType<InputManager>();
            if (inputManager != null)
            {
                inputManager.enabled = false;
                Debug.Log("  - Disabled old InputManager");
            }

            var unitSelector = Object.FindFirstObjectByType<UnitSelector>();
            if (unitSelector != null)
            {
                unitSelector.enabled = false;
                Debug.Log("  - Disabled old UnitSelector");
            }

            var cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.enabled = false;
                Debug.Log("  - Disabled old CameraController");
            }

            // Add new simple test input controller
            GameObject inputControllerObj = GameObject.Find("TestInputController");
            if (inputControllerObj == null)
            {
                inputControllerObj = new GameObject("TestInputController");
                Debug.Log("  - Created TestInputController GameObject");
            }

            var testController = inputControllerObj.GetComponent<SimpleTestInputController>();
            if (testController == null)
            {
                testController = inputControllerObj.AddComponent<SimpleTestInputController>();
                Debug.Log("  - Added SimpleTestInputController component");
            }

            // Link camera reference
            SerializedObject so = new SerializedObject(testController);
            so.FindProperty("_camera").objectReferenceValue = mainCamera;
            so.ApplyModifiedProperties();

            Debug.Log("  - New Input System controller set up successfully!");
        }

        private static void LinkCameraReferences()
        {
            // This is handled in LinkInputReferences
        }

        private static void CreateTestUnits()
        {
            Debug.Log("[4/4] Creating test units...");

            // Check if units already exist
            Unit[] existingUnits = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            if (existingUnits.Length > 0)
            {
                bool recreate = EditorUtility.DisplayDialog(
                    "Units Already Exist",
                    $"Found {existingUnits.Length} existing unit(s). Do you want to delete and recreate them?",
                    "Yes, Recreate",
                    "No, Keep Existing"
                );

                if (!recreate)
                {
                    Debug.Log("  - Keeping existing units");
                    return;
                }

                // Delete existing units
                foreach (var unit in existingUnits)
                {
                    Object.DestroyImmediate(unit.gameObject);
                }
                Debug.Log("  - Deleted existing units");
            }

            // Create 4 test units (2 per player)
            CreateTestUnit("TestUnit_Player1_01", new Vector3(-5, 0, -3), 0, Color.blue);
            CreateTestUnit("TestUnit_Player1_02", new Vector3(-5, 0, 3), 0, Color.blue);
            CreateTestUnit("TestUnit_Player2_01", new Vector3(5, 0, -3), 1, Color.red);
            CreateTestUnit("TestUnit_Player2_02", new Vector3(5, 0, 3), 1, Color.red);

            Debug.Log("  - Created 4 test units (2 per player)");
        }

        private static void CreateTestUnit(string name, Vector3 position, int ownerIndex, Color color)
        {
            GameObject unitObj = new GameObject(name);
            unitObj.transform.position = position;

            // Add Unit component
            var unit = unitObj.AddComponent<Unit>();

            // Set owner using SerializedObject (in case it's private)
            SerializedObject so = new SerializedObject(unit);
            so.FindProperty("_ownerPlayerIndex").intValue = ownerIndex;
            so.ApplyModifiedProperties();

            // Add other components
            var stats = unitObj.AddComponent<UnitStats>();
            unitObj.AddComponent<UnitMovement>();
            unitObj.AddComponent<UnitVisuals>();

            // Initialize stats with default values (since we have no UnitData yet)
            SerializedObject statsSO = new SerializedObject(stats);
            statsSO.FindProperty("_maxHealth").intValue = 10;
            statsSO.FindProperty("_currentHealth").intValue = 10;
            statsSO.FindProperty("_attack").intValue = 3;
            statsSO.FindProperty("_defense").intValue = 2;
            statsSO.FindProperty("_armor").intValue = 1;
            statsSO.FindProperty("_movementSpeed").floatValue = 6f;
            statsSO.FindProperty("_remainingMovement").floatValue = 6f;
            statsSO.ApplyModifiedProperties();

            // Add collider for selection
            var collider = unitObj.AddComponent<CapsuleCollider>();
            collider.radius = 0.5f;
            collider.height = 2f;
            collider.center = new Vector3(0, 1, 0);

            // Create visual representation (cube child)
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObj.name = "Visual";
            visualObj.transform.SetParent(unitObj.transform);
            visualObj.transform.localPosition = new Vector3(0, 1, 0);
            visualObj.transform.localScale = new Vector3(0.8f, 1.8f, 0.8f);

            // Remove the cube's collider (unit already has one)
            Object.DestroyImmediate(visualObj.GetComponent<BoxCollider>());

            // Color the unit
            var renderer = visualObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }

            Debug.Log($"    - Created {name} at {position}");
        }

        [MenuItem("Warslammer/Setup/Create Unit Data Assets")]
        public static void CreateUnitDataAssets()
        {
            string folderPath = "Assets/_Project/Data/Units";

            // Create folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] folders = folderPath.Split('/');
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }

                Debug.Log($"Created folder: {folderPath}");
            }

            // Create basic UnitData asset
            UnitData infantryData = ScriptableObject.CreateInstance<UnitData>();

            string assetPath = $"{folderPath}/Infantry_Basic.asset";
            AssetDatabase.CreateAsset(infantryData, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created UnitData asset: {assetPath}");
            EditorUtility.DisplayDialog(
                "Unit Data Created",
                $"Created basic UnitData asset at:\n{assetPath}\n\n" +
                "You can now configure the stats in the Inspector and assign it to units.",
                "OK"
            );

            // Select the asset in the Project window
            Selection.activeObject = infantryData;
            EditorGUIUtility.PingObject(infantryData);
        }
    }
}
