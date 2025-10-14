using UnityEngine;
using UnityEditor;
using Warslammer.Combat;
using Warslammer.Core;
using System.IO;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to create default weapon assets
    /// Menu: Warslammer > Setup > Create Default Weapons
    /// </summary>
    public class CreateWeaponAssets : UnityEditor.Editor
    {
        private const string WEAPONS_FOLDER = "Assets/_Project/Data/Weapons";

        [MenuItem("Warslammer/Setup/Create Default Weapons")]
        public static void CreateDefaultWeapons()
        {
            Debug.Log("=== Creating Default Weapon Assets ===");

            // Ensure the Weapons folder exists
            if (!Directory.Exists(WEAPONS_FOLDER))
            {
                Directory.CreateDirectory(WEAPONS_FOLDER);
                AssetDatabase.Refresh();
                Debug.Log($"  - Created folder: {WEAPONS_FOLDER}");
            }

            // Create weapons
            CreateSword();
            CreateBow();
            CreateSpear();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== Weapon Assets Created! ===");
            EditorUtility.DisplayDialog(
                "Weapons Created",
                "Default weapon assets have been created:\n\n" +
                "- Weapon_Sword (Melee, 2d6, 3 dmg)\n" +
                "- Weapon_Bow (Ranged 24\", 2d6, 2 dmg)\n" +
                "- Weapon_Spear (Melee 2\", 1d6, 2 dmg, +1 vs Cavalry)\n\n" +
                $"Location: {WEAPONS_FOLDER}",
                "OK"
            );
        }

        private static void CreateSword()
        {
            string path = $"{WEAPONS_FOLDER}/Weapon_Sword.asset";

            // Check if already exists
            WeaponData existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (existing != null)
            {
                Debug.Log($"  - Weapon_Sword already exists, skipping");
                return;
            }

            // Create new WeaponData
            WeaponData sword = ScriptableObject.CreateInstance<WeaponData>();

            // Basic Info
            sword.weaponName = "Sword";
            sword.description = "A balanced melee weapon. Reliable damage output with moderate range.";

            // Range
            sword.rangeType = RangeType.Melee;
            sword.minRangeInches = 0f;
            sword.maxRangeInches = 1f;

            // Attack Stats
            sword.attackDice = 2;
            sword.toHitTarget = 4;
            sword.baseDamage = 3;
            sword.armorPenetration = 0;

            // Special Rules
            sword.damageSource = DamageSource.Physical;
            sword.allowsCriticals = true;
            sword.ignoresCover = false;
            sword.unblockable = false;
            sword.hasBlast = false;
            sword.hasBonusVsType = false;

            // Status Effects
            sword.canInflictBleed = false;
            sword.canInflictRooted = false;

            // Attack Modifiers
            sword.rerollOnes = false;
            sword.rerollFailed = false;
            sword.attackModifier = 0;

            // Requirements
            sword.requiresTwoHands = false;
            sword.limitedUse = false;

            // Save asset
            AssetDatabase.CreateAsset(sword, path);
            Debug.Log($"  - Created Weapon_Sword");
        }

        private static void CreateBow()
        {
            string path = $"{WEAPONS_FOLDER}/Weapon_Bow.asset";

            // Check if already exists
            WeaponData existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (existing != null)
            {
                Debug.Log($"  - Weapon_Bow already exists, skipping");
                return;
            }

            // Create new WeaponData
            WeaponData bow = ScriptableObject.CreateInstance<WeaponData>();

            // Basic Info
            bow.weaponName = "Bow";
            bow.description = "Long-range projectile weapon. Can attack from a safe distance but has minimum range.";

            // Range
            bow.rangeType = RangeType.Ranged;
            bow.minRangeInches = 3f;
            bow.maxRangeInches = 24f;

            // Attack Stats
            bow.attackDice = 2;
            bow.toHitTarget = 4;
            bow.baseDamage = 2;
            bow.armorPenetration = 0;

            // Special Rules
            bow.damageSource = DamageSource.Physical;
            bow.allowsCriticals = true;
            bow.ignoresCover = false;
            bow.unblockable = false;
            bow.hasBlast = false;
            bow.hasBonusVsType = false;

            // Status Effects
            bow.canInflictBleed = false;
            bow.canInflictRooted = false;

            // Attack Modifiers
            bow.rerollOnes = false;
            bow.rerollFailed = false;
            bow.attackModifier = 0;

            // Requirements
            bow.requiresTwoHands = true;
            bow.limitedUse = false;

            // Save asset
            AssetDatabase.CreateAsset(bow, path);
            Debug.Log($"  - Created Weapon_Bow");
        }

        private static void CreateSpear()
        {
            string path = $"{WEAPONS_FOLDER}/Weapon_Spear.asset";

            // Check if already exists
            WeaponData existing = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (existing != null)
            {
                Debug.Log($"  - Weapon_Spear already exists, skipping");
                return;
            }

            // Create new WeaponData
            WeaponData spear = ScriptableObject.CreateInstance<WeaponData>();

            // Basic Info
            spear.weaponName = "Spear";
            spear.description = "Reach weapon with bonus damage against cavalry. Extended melee range.";

            // Range
            spear.rangeType = RangeType.Melee;
            spear.minRangeInches = 0f;
            spear.maxRangeInches = 2f;

            // Attack Stats
            spear.attackDice = 1;
            spear.toHitTarget = 4;
            spear.baseDamage = 2;
            spear.armorPenetration = 0;

            // Special Rules
            spear.damageSource = DamageSource.Physical;
            spear.allowsCriticals = true;
            spear.ignoresCover = false;
            spear.unblockable = false;
            spear.hasBlast = false;
            spear.hasBonusVsType = true;
            spear.bonusVsUnitType = UnitType.Cavalry;
            spear.bonusDamageAmount = 1;

            // Status Effects
            spear.canInflictBleed = false;
            spear.canInflictRooted = false;

            // Attack Modifiers
            spear.rerollOnes = false;
            spear.rerollFailed = false;
            spear.attackModifier = 0;

            // Requirements
            spear.requiresTwoHands = false;
            spear.limitedUse = false;

            // Save asset
            AssetDatabase.CreateAsset(spear, path);
            Debug.Log($"  - Created Weapon_Spear");
        }
    }
}
