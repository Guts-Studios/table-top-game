using UnityEngine;
using UnityEditor;
using Warslammer.Data;
using Warslammer.Core;
using System.IO;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to create default ability assets
    /// Menu: Warslammer > Setup > Create Default Abilities
    /// </summary>
    public class CreateAbilityAssets : UnityEditor.Editor
    {
        private const string ABILITIES_FOLDER = "Assets/_Project/Data/Abilities";

        [MenuItem("Warslammer/Setup/Create Default Abilities")]
        public static void CreateDefaultAbilities()
        {
            Debug.Log("=== Creating Default Ability Assets ===");

            // Ensure the Abilities folder exists
            if (!Directory.Exists(ABILITIES_FOLDER))
            {
                Directory.CreateDirectory(ABILITIES_FOLDER);
                AssetDatabase.Refresh();
                Debug.Log($"  - Created folder: {ABILITIES_FOLDER}");
            }

            // Create 6 basic spells
            CreateNuke();
            CreateShield();
            CreateSlow();
            CreateHeal();
            CreateSummon();
            CreateWall();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== Ability Assets Created! ===");
            EditorUtility.DisplayDialog(
                "Abilities Created",
                "Default ability assets have been created:\n\n" +
                "1. Fireball (Nuke) - AOE damage\n" +
                "2. Shield - Defense buff\n" +
                "3. Slow - Movement debuff\n" +
                "4. Heal - Restore HP\n" +
                "5. Summon - Spawn unit (placeholder)\n" +
                "6. Wall - Create barrier (placeholder)\n\n" +
                $"Location: {ABILITIES_FOLDER}",
                "OK"
            );
        }

        private static void CreateNuke()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Fireball.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Fireball already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Fireball";
            ability.abilityID = "fireball";
            ability.description = "Unleash a massive fireball that explodes on impact, dealing damage to all units in the area.";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 1;
            ability.cooldownTurns = 3;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Ranged;
            ability.range = 12f; // 12 inches
            ability.areaOfEffect = 3f; // 3 inch radius
            ability.canTargetAllies = false;
            ability.canTargetEnemies = true;
            ability.requiresLineOfSight = true;

            // Effects
            ability.damage = 5;
            ability.damageType = DamageSource.Fire;
            ability.healing = 0;
            ability.duration = 0; // Instant

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 0;
            ability.movementModifier = 0f;
            ability.armorModifier = 0;

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = true;
            ability.knockbackDistance = 1f;

            // Special Rules
            ability.specialRules = "AOE damage. Enemies at the center take full damage, reduced by distance.";
            ability.usableInMelee = false;
            ability.endsActivation = true;

            // AI Behavior
            ability.aiPriority = 8;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 100;

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Fireball");
        }

        private static void CreateShield()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Shield.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Shield already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Shield";
            ability.abilityID = "shield";
            ability.description = "Summon a protective barrier that increases defense for 2 turns.";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 1;
            ability.cooldownTurns = 4;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Ranged;
            ability.range = 6f;
            ability.areaOfEffect = 0f;
            ability.canTargetAllies = true;
            ability.canTargetEnemies = false;
            ability.requiresLineOfSight = true;

            // Effects
            ability.damage = 0;
            ability.healing = 0;
            ability.duration = 2; // Lasts 2 turns

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 3; // +3 defense
            ability.movementModifier = 0f;
            ability.armorModifier = 2; // +2 armor

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = false;

            // Special Rules
            ability.specialRules = "Grants +3 defense and +2 armor for 2 turns.";
            ability.usableInMelee = true;
            ability.endsActivation = false;

            // AI Behavior
            ability.aiPriority = 7;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 50; // Use when below 50% health

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Shield");
        }

        private static void CreateSlow()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Slow.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Slow already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Slow";
            ability.abilityID = "slow";
            ability.description = "Curse an enemy, reducing their movement speed for 3 turns.";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 1;
            ability.cooldownTurns = 2;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Ranged;
            ability.range = 12f;
            ability.areaOfEffect = 0f;
            ability.canTargetAllies = false;
            ability.canTargetEnemies = true;
            ability.requiresLineOfSight = true;

            // Effects
            ability.damage = 0;
            ability.healing = 0;
            ability.duration = 3; // Lasts 3 turns

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 0;
            ability.movementModifier = -3f; // -3 inches movement
            ability.armorModifier = 0;

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = false;

            // Special Rules
            ability.specialRules = "Reduces target's movement by 3 inches for 3 turns.";
            ability.usableInMelee = true;
            ability.endsActivation = false;

            // AI Behavior
            ability.aiPriority = 6;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 100;

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Slow");
        }

        private static void CreateHeal()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Heal.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Heal already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Heal";
            ability.abilityID = "heal";
            ability.description = "Restore health to a friendly unit.";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 1;
            ability.cooldownTurns = 1;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Ranged;
            ability.range = 6f;
            ability.areaOfEffect = 0f;
            ability.canTargetAllies = true;
            ability.canTargetEnemies = false;
            ability.requiresLineOfSight = true;

            // Effects
            ability.damage = 0;
            ability.healing = 6;
            ability.duration = 0; // Instant

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 0;
            ability.movementModifier = 0f;
            ability.armorModifier = 0;

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = false;

            // Special Rules
            ability.specialRules = "Restores 6 HP to target ally.";
            ability.usableInMelee = true;
            ability.endsActivation = false;

            // AI Behavior
            ability.aiPriority = 9;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 50; // Use when ally below 50% health

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Heal");
        }

        private static void CreateSummon()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Summon.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Summon already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Summon Minion";
            ability.abilityID = "summon_minion";
            ability.description = "Summon a weak minion to fight for you. (Placeholder - summon logic not yet implemented)";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 2;
            ability.cooldownTurns = 5;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Self;
            ability.range = 0f;
            ability.areaOfEffect = 0f;
            ability.canTargetAllies = false;
            ability.canTargetEnemies = false;
            ability.requiresLineOfSight = false;

            // Effects
            ability.damage = 0;
            ability.healing = 0;
            ability.duration = 0;

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 0;
            ability.movementModifier = 0f;
            ability.armorModifier = 0;

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = false;

            // Special Rules
            ability.specialRules = "PLACEHOLDER: Would summon a unit. Not yet implemented in Phase 6.";
            ability.usableInMelee = true;
            ability.endsActivation = true;

            // AI Behavior
            ability.aiPriority = 5;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 100;

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Summon (placeholder)");
        }

        private static void CreateWall()
        {
            string path = $"{ABILITIES_FOLDER}/Ability_Wall.asset";

            if (AssetDatabase.LoadAssetAtPath<AbilityData>(path) != null)
            {
                Debug.Log("  - Ability_Wall already exists, skipping");
                return;
            }

            AbilityData ability = ScriptableObject.CreateInstance<AbilityData>();

            // Basic Info
            ability.abilityName = "Stone Wall";
            ability.abilityID = "stone_wall";
            ability.description = "Summon a stone wall to block movement. (Placeholder - wall spawning not yet implemented)";
            ability.abilityType = AbilityType.Active;
            ability.timing = AbilityTiming.ActionPhase;

            // Costs
            ability.actionPointCost = 1;
            ability.cooldownTurns = 4;
            ability.healthCost = 0;

            // Range & Targeting
            ability.rangeType = RangeType.Ranged;
            ability.range = 6f;
            ability.areaOfEffect = 0f;
            ability.canTargetAllies = false;
            ability.canTargetEnemies = false;
            ability.requiresLineOfSight = true;

            // Effects
            ability.damage = 0;
            ability.healing = 0;
            ability.duration = 3; // Wall lasts 3 turns

            // Stat Modifiers
            ability.attackModifier = 0;
            ability.defenseModifier = 0;
            ability.movementModifier = 0f;
            ability.armorModifier = 0;

            // Status Effects
            ability.appliesStun = false;
            ability.appliesPoison = false;
            ability.appliesKnockback = false;

            // Special Rules
            ability.specialRules = "PLACEHOLDER: Would create a wall obstacle. Not yet implemented in Phase 6.";
            ability.usableInMelee = false;
            ability.endsActivation = false;

            // AI Behavior
            ability.aiPriority = 4;
            ability.aiMinHealthPercent = 0;
            ability.aiMaxHealthPercent = 100;

            AssetDatabase.CreateAsset(ability, path);
            Debug.Log("  - Created Ability_Wall (placeholder)");
        }
    }
}
