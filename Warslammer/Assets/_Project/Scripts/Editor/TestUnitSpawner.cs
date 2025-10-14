using UnityEngine;
using UnityEditor;
using Warslammer.Data;
using Warslammer.Units;
using Warslammer.Core;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to spawn test units in the active scene
    /// </summary>
    public static class TestUnitSpawner
    {
        [MenuItem("Warslammer/Testing/Spawn Test Units in Scene")]
        public static void SpawnTestUnits()
        {
            // Load the unit data assets
            UnitData swordsman = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/TestSwordsman.asset");
            UnitData archer = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/TestArcher.asset");
            UnitData knight = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/TestKnight.asset");
            UnitData hero = AssetDatabase.LoadAssetAtPath<UnitData>("Assets/_Project/Data/Units/TestHero.asset");

            if (swordsman == null || archer == null || knight == null || hero == null)
            {
                Debug.LogError("[TestUnitSpawner] Could not load test unit data assets!");
                return;
            }

            // Create parent object for test units
            GameObject testUnitsParent = new GameObject("TestUnits");
            Undo.RegisterCreatedObjectUndo(testUnitsParent, "Create Test Units");

            // Spawn units in a grid pattern
            SpawnUnit(swordsman, new Vector3(-10, 0, 10), testUnitsParent.transform, "TestSwordsman_1");
            SpawnUnit(swordsman, new Vector3(-5, 0, 10), testUnitsParent.transform, "TestSwordsman_2");

            SpawnUnit(archer, new Vector3(5, 0, 10), testUnitsParent.transform, "TestArcher_1");
            SpawnUnit(archer, new Vector3(10, 0, 10), testUnitsParent.transform, "TestArcher_2");

            SpawnUnit(knight, new Vector3(-10, 0, -10), testUnitsParent.transform, "TestKnight_1");
            SpawnUnit(knight, new Vector3(-5, 0, -10), testUnitsParent.transform, "TestKnight_2");

            SpawnUnit(hero, new Vector3(0, 0, 0), testUnitsParent.transform, "TestHero_1");

            Debug.Log("[TestUnitSpawner] Created 7 test units in the scene");
            Selection.activeGameObject = testUnitsParent;
        }

        private static void SpawnUnit(UnitData unitData, Vector3 position, Transform parent, string name)
        {
            // Create the GameObject
            GameObject unitGO = new GameObject(name);
            unitGO.transform.SetParent(parent);
            unitGO.transform.position = position;
            unitGO.tag = "Unit"; // Make sure this tag exists

            // Add required components
            Unit unit = unitGO.AddComponent<Unit>();
            UnitStats stats = unitGO.AddComponent<UnitStats>();
            UnitMovement movement = unitGO.AddComponent<UnitMovement>();
            UnitVisuals visuals = unitGO.AddComponent<UnitVisuals>();

            // Add collider for selection and physics
            CapsuleCollider collider = unitGO.AddComponent<CapsuleCollider>();
            collider.radius = GetColliderRadius(unitData.baseSize);
            collider.height = 2f;
            collider.center = new Vector3(0, 1, 0);

            // Add visual representation
            GameObject visualChild = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visualChild.name = "Visual";
            visualChild.transform.SetParent(unitGO.transform);
            visualChild.transform.localPosition = Vector3.up;
            visualChild.transform.localScale = new Vector3(
                GetColliderRadius(unitData.baseSize) * 2,
                1f,
                GetColliderRadius(unitData.baseSize) * 2
            );

            // Assign different colors based on unit type
            Renderer renderer = visualChild.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetUnitColor(unitData.unitType);
            renderer.material = mat;

            // Remove the extra collider from the primitive
            Object.DestroyImmediate(visualChild.GetComponent<Collider>());

            // Initialize the unit with its data
            SerializedObject serializedUnit = new SerializedObject(unit);
            serializedUnit.FindProperty("unitData").objectReferenceValue = unitData;
            serializedUnit.FindProperty("currentHealth").intValue = unitData.maxHealth;
            serializedUnit.ApplyModifiedProperties();

            // Set the layer to "Units" if it exists, otherwise use Default
            int unitsLayer = LayerMask.NameToLayer("Units");
            if (unitsLayer != -1)
            {
                unitGO.layer = unitsLayer;
            }

            Undo.RegisterCreatedObjectUndo(unitGO, $"Create {name}");
        }

        private static float GetColliderRadius(BaseSize baseSize)
        {
            switch (baseSize)
            {
                case BaseSize.Small_25mm:
                    return 0.5f;
                case BaseSize.Medium_40mm:
                    return 0.8f;
                case BaseSize.Large_60mm:
                    return 1.2f;
                case BaseSize.Huge_80mm:
                    return 1.6f;
                case BaseSize.Gargantuan_100mm:
                    return 2.0f;
                default:
                    return 0.5f;
            }
        }

        private static Color GetUnitColor(UnitType unitType)
        {
            switch (unitType)
            {
                case UnitType.Infantry:
                    return new Color(0.3f, 0.6f, 0.3f); // Green
                case UnitType.Cavalry:
                    return new Color(0.6f, 0.4f, 0.2f); // Brown
                case UnitType.Monster:
                    return new Color(0.6f, 0.2f, 0.2f); // Red
                case UnitType.Hero:
                    return new Color(0.8f, 0.7f, 0.2f); // Gold
                case UnitType.Artillery:
                    return new Color(0.4f, 0.4f, 0.4f); // Gray
                case UnitType.Vehicle:
                    return new Color(0.3f, 0.3f, 0.6f); // Blue
                default:
                    return Color.white;
            }
        }
    }
}
