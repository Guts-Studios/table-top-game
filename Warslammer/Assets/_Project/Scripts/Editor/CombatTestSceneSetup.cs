using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Warslammer.Core;
using Warslammer.Battlefield;
using Warslammer.Combat;
using Warslammer.Units;
using Warslammer.UI;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to automatically set up the CombatTest scene
    /// Menu: Warslammer > Setup > Complete CombatTest Scene
    /// </summary>
    public class CombatTestSceneSetup : UnityEditor.Editor
    {
        [MenuItem("Warslammer/Setup/Complete CombatTest Scene")]
        public static void SetupCombatTestScene()
        {
            Debug.Log("=== Setting up CombatTest Scene ===");

            // Check if we're in CombatTest scene
            if (!EditorSceneManager.GetActiveScene().name.Contains("CombatTest"))
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Scene Check",
                    "This doesn't appear to be the CombatTest scene. Continue anyway?",
                    "Yes",
                    "No"
                );

                if (!proceed)
                {
                    Debug.Log("Setup cancelled by user");
                    return;
                }
            }

            // Run setup steps
            SetupGameManager();
            SetupCombatManager();
            SetupBattlefieldManager();
            SetupCamera();
            SetupUI();
            CreateTestUnits();

            // Mark scene as dirty
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log("=== CombatTest Scene Setup Complete! ===");
            EditorUtility.DisplayDialog(
                "Setup Complete",
                "CombatTest scene has been configured!\n\n" +
                "- GameManager created\n" +
                "- Combat systems initialized\n" +
                "- Test units created\n" +
                "- UI created\n\n" +
                "Press Play and use SPACEBAR to trigger test attacks!",
                "OK"
            );
        }

        private static GameObject SetupGameManager()
        {
            Debug.Log("[1/6] Setting up GameManager...");

            GameObject gameManagerObj = GameObject.Find("GameManager");
            if (gameManagerObj == null)
            {
                gameManagerObj = new GameObject("GameManager");
                Debug.Log("  - Created GameManager GameObject");
            }

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

            var saveManager = gameManagerObj.GetComponent<SaveManager>();
            if (saveManager == null)
            {
                saveManager = gameManagerObj.AddComponent<SaveManager>();
                Debug.Log("  - Added SaveManager component");
            }

            // Link managers using SerializedObject
            SerializedObject so = new SerializedObject(gameManager);
            SerializedProperty turnManagerProp = so.FindProperty("turnManager");
            if (turnManagerProp != null)
            {
                turnManagerProp.objectReferenceValue = turnManager;
            }
            else
            {
                gameManager.turnManager = turnManager;
            }

            SerializedProperty phaseManagerProp = so.FindProperty("phaseManager");
            if (phaseManagerProp != null)
            {
                phaseManagerProp.objectReferenceValue = phaseManager;
            }
            else
            {
                gameManager.phaseManager = phaseManager;
            }

            SerializedProperty saveManagerProp = so.FindProperty("saveManager");
            if (saveManagerProp != null)
            {
                saveManagerProp.objectReferenceValue = saveManager;
            }
            else
            {
                gameManager.saveManager = saveManager;
            }

            so.ApplyModifiedProperties();

            Debug.Log("  - GameManager setup complete");
            return gameManagerObj;
        }

        private static GameObject SetupCombatManager()
        {
            Debug.Log("[2/6] Setting up CombatManager...");

            GameObject combatManagerObj = GameObject.Find("CombatManager");
            if (combatManagerObj == null)
            {
                combatManagerObj = new GameObject("CombatManager");
                Debug.Log("  - Created CombatManager GameObject");
            }

            // Add combat components
            if (combatManagerObj.GetComponent<DiceRoller>() == null)
            {
                combatManagerObj.AddComponent<DiceRoller>();
                Debug.Log("  - Added DiceRoller");
            }

            if (combatManagerObj.GetComponent<DamageCalculator>() == null)
            {
                combatManagerObj.AddComponent<DamageCalculator>();
                Debug.Log("  - Added DamageCalculator");
            }

            if (combatManagerObj.GetComponent<RangeCalculator>() == null)
            {
                combatManagerObj.AddComponent<RangeCalculator>();
                Debug.Log("  - Added RangeCalculator");
            }

            if (combatManagerObj.GetComponent<CombatResolver>() == null)
            {
                combatManagerObj.AddComponent<CombatResolver>();
                Debug.Log("  - Added CombatResolver");
            }

            if (combatManagerObj.GetComponent<MoraleSystem>() == null)
            {
                combatManagerObj.AddComponent<MoraleSystem>();
                Debug.Log("  - Added MoraleSystem");
            }

            Debug.Log("  - CombatManager setup complete");
            return combatManagerObj;
        }

        private static GameObject SetupBattlefieldManager()
        {
            Debug.Log("[3/6] Setting up BattlefieldManager...");

            GameObject battlefieldObj = GameObject.Find("Battlefield");
            if (battlefieldObj == null)
            {
                battlefieldObj = new GameObject("Battlefield");
                Debug.Log("  - Created Battlefield GameObject");
            }

            if (battlefieldObj.GetComponent<BattlefieldManager>() == null)
            {
                battlefieldObj.AddComponent<BattlefieldManager>();
                Debug.Log("  - Added BattlefieldManager");
            }

            if (battlefieldObj.GetComponent<LineOfSightManager>() == null)
            {
                battlefieldObj.AddComponent<LineOfSightManager>();
                Debug.Log("  - Added LineOfSightManager");
            }

            if (battlefieldObj.GetComponent<TerrainManager>() == null)
            {
                battlefieldObj.AddComponent<TerrainManager>();
                Debug.Log("  - Added TerrainManager");
            }

            if (battlefieldObj.GetComponent<DeploymentZoneManager>() == null)
            {
                battlefieldObj.AddComponent<DeploymentZoneManager>();
                Debug.Log("  - Added DeploymentZoneManager");
            }

            // Ensure ground plane exists
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(5, 1, 5);
                Debug.Log("  - Created Ground plane");
            }

            Debug.Log("  - BattlefieldManager setup complete");
            return battlefieldObj;
        }

        private static Camera SetupCamera()
        {
            Debug.Log("[4/6] Setting up Camera...");

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                Debug.Log("  - Created Main Camera");
            }

            // Position camera for top-down view
            mainCamera.transform.position = new Vector3(0, 10, -5);
            mainCamera.transform.rotation = Quaternion.Euler(60, 0, 0);

            Debug.Log("  - Camera positioned for combat view");
            return mainCamera;
        }

        private static void SetupUI()
        {
            Debug.Log("[5/6] Setting up UI...");

            // Create Canvas
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("  - Created Canvas");
            }

            // Create Combat Log Panel
            GameObject combatLogPanel = new GameObject("CombatLogPanel");
            combatLogPanel.transform.SetParent(canvas.transform, false);

            var panelRect = combatLogPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.5f);
            panelRect.anchorMax = new Vector2(0.3f, 1);
            panelRect.offsetMin = new Vector2(10, 10);
            panelRect.offsetMax = new Vector2(-10, -10);

            var panelImage = combatLogPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            // Create Scroll View
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(combatLogPanel.transform, false);

            var scrollRect = scrollView.AddComponent<UnityEngine.UI.ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(5, 5);
            scrollRectTransform.offsetMax = new Vector2(-5, -5);

            // Create Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            var viewportMask = viewport.AddComponent<UnityEngine.UI.Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewport.AddComponent<UnityEngine.UI.Image>();

            // Create Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 300);

            var contentLayout = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            contentLayout.childForceExpandHeight = false;
            contentLayout.childControlHeight = true;

            // Create Log Text
            GameObject logText = new GameObject("LogText");
            logText.transform.SetParent(content.transform, false);
            var textComponent = logText.AddComponent<UnityEngine.UI.Text>();
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;
            textComponent.supportRichText = true;

            var textRect = logText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 1);

            // Link ScrollRect references
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;

            // Add CombatLog component
            var combatLog = scrollView.AddComponent<CombatLog>();
            SerializedObject so = new SerializedObject(combatLog);
            so.FindProperty("_logText").objectReferenceValue = textComponent;
            so.FindProperty("_scrollRect").objectReferenceValue = scrollRect;
            so.ApplyModifiedProperties();

            Debug.Log("  - Combat Log UI created");

            // Create Instructions Text
            GameObject instructionsObj = new GameObject("InstructionsText");
            instructionsObj.transform.SetParent(canvas.transform, false);

            var instructionsRect = instructionsObj.AddComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0.5f, 0);
            instructionsRect.anchorMax = new Vector2(0.5f, 0);
            instructionsRect.pivot = new Vector2(0.5f, 0);
            instructionsRect.anchoredPosition = new Vector2(0, 20);
            instructionsRect.sizeDelta = new Vector2(600, 60);

            var instructionsText = instructionsObj.AddComponent<UnityEngine.UI.Text>();
            instructionsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            instructionsText.fontSize = 16;
            instructionsText.color = Color.yellow;
            instructionsText.alignment = TextAnchor.MiddleCenter;
            instructionsText.text = "Press SPACEBAR to trigger test attack\nRed unit attacks Blue unit";

            Debug.Log("  - Instructions text created");
        }

        private static void CreateTestUnits()
        {
            Debug.Log("[6/6] Creating test units...");

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

            // Create both units first
            GameObject attackerObj = CreateCombatTestUnit("TestUnit_Attacker", new Vector3(0, 0.5f, -2), 0, Color.red, true);
            GameObject defenderObj = CreateCombatTestUnit("TestUnit_Defender", new Vector3(0, 0.5f, 2), 1, Color.blue, false);

            // Now link the defender reference to the attacker's CombatTestController
            var testController = attackerObj.GetComponent<CombatTestController>();
            var defenderUnit = defenderObj.GetComponent<Unit>();

            if (testController != null && defenderUnit != null)
            {
                SerializedObject controllerSO = new SerializedObject(testController);
                controllerSO.FindProperty("defender").objectReferenceValue = defenderUnit;
                controllerSO.ApplyModifiedProperties();
                Debug.Log("  - Linked defender to attacker's CombatTestController");
            }

            Debug.Log("  - Created 2 test units (Attacker and Defender)");
        }

        private static GameObject CreateCombatTestUnit(string name, Vector3 position, int ownerIndex, Color color, bool isAttacker)
        {
            GameObject unitObj = new GameObject(name);
            unitObj.transform.position = position;

            // Add Unit component
            var unit = unitObj.AddComponent<Unit>();

            // Set owner
            SerializedObject unitSO = new SerializedObject(unit);
            unitSO.FindProperty("_ownerPlayerIndex").intValue = ownerIndex;
            unitSO.ApplyModifiedProperties();

            // Add required components
            var stats = unitObj.AddComponent<UnitStats>();
            unitObj.AddComponent<UnitMovement>();
            unitObj.AddComponent<UnitVisuals>();
            var unitCombat = unitObj.AddComponent<UnitCombat>();
            unitObj.AddComponent<StatusEffectManager>();

            // Initialize stats
            SerializedObject statsSO = new SerializedObject(stats);
            statsSO.FindProperty("_maxHealth").intValue = 20;
            statsSO.FindProperty("_currentHealth").intValue = 20;
            statsSO.FindProperty("_attack").intValue = 3;
            statsSO.FindProperty("_defense").intValue = 2;
            statsSO.FindProperty("_armor").intValue = 1;
            statsSO.FindProperty("_movementSpeed").floatValue = 6f;
            statsSO.FindProperty("_remainingMovement").floatValue = 6f;
            statsSO.ApplyModifiedProperties();

            // Add collider
            var collider = unitObj.AddComponent<CapsuleCollider>();
            collider.radius = 0.5f;
            collider.height = 2f;
            collider.center = new Vector3(0, 1, 0);

            // Create visual cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Visual";
            cube.transform.SetParent(unitObj.transform);
            cube.transform.localPosition = new Vector3(0, 1, 0);
            cube.transform.localScale = new Vector3(0.5f, 1f, 0.5f);

            // Remove collider from visual (unit already has one)
            Object.DestroyImmediate(cube.GetComponent<Collider>());

            // Set color
            var renderer = cube.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;

            // Try to find and assign weapons (they may not exist yet)
            if (isAttacker)
            {
                WeaponData sword = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/_Project/Data/Weapons/Weapon_Sword.asset");
                if (sword != null)
                {
                    SerializedObject combatSO = new SerializedObject(unitCombat);
                    combatSO.FindProperty("_primaryWeapon").objectReferenceValue = sword;
                    combatSO.ApplyModifiedProperties();
                    Debug.Log($"  - Assigned Sword to {name}");
                }
                else
                {
                    Debug.LogWarning("  - Weapon_Sword.asset not found. Create it using Create > Warslammer > Combat > Weapon Data");
                }
            }
            else
            {
                WeaponData bow = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/_Project/Data/Weapons/Weapon_Bow.asset");
                if (bow != null)
                {
                    SerializedObject combatSO = new SerializedObject(unitCombat);
                    combatSO.FindProperty("_primaryWeapon").objectReferenceValue = bow;
                    combatSO.ApplyModifiedProperties();
                    Debug.Log($"  - Assigned Bow to {name}");
                }
                else
                {
                    Debug.LogWarning("  - Weapon_Bow.asset not found. Create it using Create > Warslammer > Combat > Weapon Data");
                }
            }

            // Add CombatTestController to attacker for spacebar testing
            if (isAttacker)
            {
                unitObj.AddComponent<CombatTestController>();
            }

            return unitObj;
        }
    }

    /// <summary>
    /// Simple test controller for combat testing
    /// Attach to attacker unit, assign defender, press Space to attack
    /// </summary>
    public class CombatTestController : MonoBehaviour
    {
        public Unit defender;

        private Unit _attacker;
        private UnitCombat _combat;

        private void Awake()
        {
            _attacker = GetComponent<Unit>();
            _combat = GetComponent<UnitCombat>();
        }

        private void Update()
        {
            // Press Space to attack (using new Input System)
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                PerformTestAttack();
            }
        }

        private void PerformTestAttack()
        {
            if (_attacker == null || defender == null)
            {
                Debug.LogError("[CombatTest] Assign defender in Inspector!");
                return;
            }

            if (_combat == null)
            {
                Debug.LogError("[CombatTest] No UnitCombat component!");
                return;
            }

            Debug.Log($"[CombatTest] {_attacker.name} attacking {defender.name}...");

            CombatResult result = _combat.Attack(defender);
            if (result != null)
            {
                Debug.Log($"[CombatTest] {result.GetCombatLogString()}");
            }
            else
            {
                Debug.LogWarning("[CombatTest] Attack returned null result!");
            }
        }
    }
}
