using UnityEngine;
using UnityEditor;
using Warslammer.Data;
using Warslammer.Core;
using System.IO;

namespace Warslammer.Editor
{
    /// <summary>
    /// Automatically generates test unit data on editor load if they don't exist
    /// </summary>
    [InitializeOnLoad]
    public static class AutoGenerateTestUnits
    {
        static AutoGenerateTestUnits()
        {
            EditorApplication.delayCall += CheckAndGenerateTestUnits;
        }

        private static void CheckAndGenerateTestUnits()
        {
            string folderPath = "Assets/_Project/Data/Units";
            string testFilePath = Path.Combine(folderPath, "TestSwordsman.asset");

            // Only generate if test units don't already exist
            if (!File.Exists(testFilePath))
            {
                GenerateTestUnits();
            }
        }

        private static void GenerateTestUnits()
        {
            string folderPath = "Assets/_Project/Data/Units";

            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/_Project/Data";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "Data");
                }
                AssetDatabase.CreateFolder(parentFolder, "Units");
            }

            // Create test units
            CreateMeleeInfantry(folderPath);
            CreateRangedInfantry(folderPath);
            CreateCavalry(folderPath);
            CreateHero(folderPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AutoGenerateTestUnits] Created 4 test unit data assets in " + folderPath);
        }

        private static void CreateMeleeInfantry(string folderPath)
        {
            UnitData unit = ScriptableObject.CreateInstance<UnitData>();

            // Basic Info
            unit.unitName = "Test Swordsman";
            unit.unitID = "test_swordsman";
            unit.description = "Basic melee infantry unit for testing movement and combat.";
            unit.unitType = UnitType.Infantry;
            unit.baseSize = BaseSize.Small_25mm;
            unit.pointsCost = 10;

            // Combat Stats
            unit.maxHealth = 10;
            unit.movementSpeed = 6f;
            unit.armor = 2;
            unit.attack = 3;
            unit.defense = 2;
            unit.initiative = 5;
            unit.leadership = 7;

            // Attack Attributes
            unit.attackRange = RangeType.Melee;
            unit.attackRangeDistance = 1f;
            unit.numberOfAttacks = 1;
            unit.damageType = DamageSource.Physical;

            AssetDatabase.CreateAsset(unit, $"{folderPath}/TestSwordsman.asset");
        }

        private static void CreateRangedInfantry(string folderPath)
        {
            UnitData unit = ScriptableObject.CreateInstance<UnitData>();

            // Basic Info
            unit.unitName = "Test Archer";
            unit.unitID = "test_archer";
            unit.description = "Ranged infantry unit for testing ranged attacks and line of sight.";
            unit.unitType = UnitType.Infantry;
            unit.baseSize = BaseSize.Small_25mm;
            unit.pointsCost = 12;

            // Combat Stats
            unit.maxHealth = 8;
            unit.movementSpeed = 6f;
            unit.armor = 1;
            unit.attack = 3;
            unit.defense = 1;
            unit.initiative = 6;
            unit.leadership = 6;

            // Attack Attributes
            unit.attackRange = RangeType.Ranged;
            unit.attackRangeDistance = 24f;
            unit.numberOfAttacks = 1;
            unit.damageType = DamageSource.Physical;

            AssetDatabase.CreateAsset(unit, $"{folderPath}/TestArcher.asset");
        }

        private static void CreateCavalry(string folderPath)
        {
            UnitData unit = ScriptableObject.CreateInstance<UnitData>();

            // Basic Info
            unit.unitName = "Test Knight";
            unit.unitID = "test_knight";
            unit.description = "Fast cavalry unit for testing movement and charge mechanics.";
            unit.unitType = UnitType.Cavalry;
            unit.baseSize = BaseSize.Medium_40mm;
            unit.pointsCost = 18;

            // Combat Stats
            unit.maxHealth = 15;
            unit.movementSpeed = 10f;
            unit.armor = 3;
            unit.attack = 4;
            unit.defense = 3;
            unit.initiative = 7;
            unit.leadership = 8;

            // Attack Attributes
            unit.attackRange = RangeType.Melee;
            unit.attackRangeDistance = 1f;
            unit.numberOfAttacks = 2;
            unit.damageType = DamageSource.Physical;

            AssetDatabase.CreateAsset(unit, $"{folderPath}/TestKnight.asset");
        }

        private static void CreateHero(string folderPath)
        {
            UnitData unit = ScriptableObject.CreateInstance<UnitData>();

            // Basic Info
            unit.unitName = "Test Hero";
            unit.unitID = "test_hero";
            unit.description = "Powerful hero unit for testing special abilities and leadership.";
            unit.unitType = UnitType.Hero;
            unit.baseSize = BaseSize.Medium_40mm;
            unit.pointsCost = 50;

            // Combat Stats
            unit.maxHealth = 25;
            unit.movementSpeed = 8f;
            unit.armor = 4;
            unit.attack = 5;
            unit.defense = 4;
            unit.initiative = 8;
            unit.leadership = 10;

            // Attack Attributes
            unit.attackRange = RangeType.Melee;
            unit.attackRangeDistance = 1f;
            unit.numberOfAttacks = 3;
            unit.damageType = DamageSource.Physical;

            // Special Rules
            unit.canBeGeneral = true;
            unit.hasAura = true;
            unit.auraRadius = 12f;

            AssetDatabase.CreateAsset(unit, $"{folderPath}/TestHero.asset");
        }
    }
}
