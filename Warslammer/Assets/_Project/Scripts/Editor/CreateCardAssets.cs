using UnityEngine;
using UnityEditor;
using Warslammer.Combat;
using Warslammer.Core;
using System.IO;

namespace Warslammer.Editor
{
    /// <summary>
    /// Editor utility to create default combat card assets
    /// Menu: Warslammer > Setup > Create Default Cards
    /// </summary>
    public class CreateCardAssets : UnityEditor.Editor
    {
        private const string CARDS_FOLDER = "Assets/_Project/Data/Cards";
        private const string STANDARD_FOLDER = "Assets/_Project/Data/Cards/Standard";
        private const string FACTION_FOLDER = "Assets/_Project/Data/Cards/Faction";

        [MenuItem("Warslammer/Setup/Create Default Cards")]
        public static void CreateDefaultCards()
        {
            Debug.Log("=== Creating Default Card Assets ===");

            // Ensure folders exist
            EnsureFoldersExist();

            // Create 3 attack cards
            CreateAttackCard1();
            CreateAttackCard2();
            CreateAttackCard3();

            // Create 3 defense cards
            CreateDefenseCard1();
            CreateDefenseCard2();
            CreateDefenseCard3();

            // Create 4 placeholder faction cards
            CreateFactionCard1();
            CreateFactionCard2();
            CreateFactionCard3();
            CreateFactionCard4();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== Card Assets Created! ===");
            EditorUtility.DisplayDialog(
                "Cards Created",
                "Default combat card assets have been created:\n\n" +
                "STANDARD CARDS (6):\n" +
                "- Attack +1, +2, +3\n" +
                "- Defense +1, +2, +3\n\n" +
                "FACTION CARDS (4):\n" +
                "- Placeholder cards (customize per faction)\n\n" +
                $"Location: {CARDS_FOLDER}",
                "OK"
            );
        }

        private static void EnsureFoldersExist()
        {
            if (!Directory.Exists(CARDS_FOLDER))
            {
                Directory.CreateDirectory(CARDS_FOLDER);
                Debug.Log($"  - Created folder: {CARDS_FOLDER}");
            }

            if (!Directory.Exists(STANDARD_FOLDER))
            {
                Directory.CreateDirectory(STANDARD_FOLDER);
                Debug.Log($"  - Created folder: {STANDARD_FOLDER}");
            }

            if (!Directory.Exists(FACTION_FOLDER))
            {
                Directory.CreateDirectory(FACTION_FOLDER);
                Debug.Log($"  - Created folder: {FACTION_FOLDER}");
            }

            AssetDatabase.Refresh();
        }

        #region Attack Cards
        private static void CreateAttackCard1()
        {
            string path = $"{STANDARD_FOLDER}/Card_Attack_Plus1.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Attack_Plus1 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Strike +1";
            card.cardID = "attack_plus1";
            card.description = "Add +1 to your attack roll.";
            card.cardType = CardType.Attack;
            card.timing = CardTiming.AttackPhase;
            card.isStandardCard = true;
            card.faction = FactionType.None; // All factions can use
            card.attackBonus = 1;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(1f, 0.7f, 0.7f); // Light red

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Attack_Plus1");
        }

        private static void CreateAttackCard2()
        {
            string path = $"{STANDARD_FOLDER}/Card_Attack_Plus2.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Attack_Plus2 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Strike +2";
            card.cardID = "attack_plus2";
            card.description = "Add +2 to your attack roll.";
            card.cardType = CardType.Attack;
            card.timing = CardTiming.AttackPhase;
            card.isStandardCard = true;
            card.faction = FactionType.None;
            card.attackBonus = 2;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(1f, 0.5f, 0.5f); // Medium red

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Attack_Plus2");
        }

        private static void CreateAttackCard3()
        {
            string path = $"{STANDARD_FOLDER}/Card_Attack_Plus3.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Attack_Plus3 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Strike +3";
            card.cardID = "attack_plus3";
            card.description = "Add +3 to your attack roll.";
            card.cardType = CardType.Attack;
            card.timing = CardTiming.AttackPhase;
            card.isStandardCard = true;
            card.faction = FactionType.None;
            card.attackBonus = 3;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(1f, 0.3f, 0.3f); // Dark red

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Attack_Plus3");
        }
        #endregion

        #region Defense Cards
        private static void CreateDefenseCard1()
        {
            string path = $"{STANDARD_FOLDER}/Card_Defense_Plus1.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Defense_Plus1 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Guard +1";
            card.cardID = "defense_plus1";
            card.description = "Add +1 to your defense roll.";
            card.cardType = CardType.Defense;
            card.timing = CardTiming.DefensePhase;
            card.isStandardCard = true;
            card.faction = FactionType.None;
            card.defenseBonus = 1;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(0.7f, 0.7f, 1f); // Light blue

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Defense_Plus1");
        }

        private static void CreateDefenseCard2()
        {
            string path = $"{STANDARD_FOLDER}/Card_Defense_Plus2.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Defense_Plus2 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Guard +2";
            card.cardID = "defense_plus2";
            card.description = "Add +2 to your defense roll.";
            card.cardType = CardType.Defense;
            card.timing = CardTiming.DefensePhase;
            card.isStandardCard = true;
            card.faction = FactionType.None;
            card.defenseBonus = 2;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(0.5f, 0.5f, 1f); // Medium blue

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Defense_Plus2");
        }

        private static void CreateDefenseCard3()
        {
            string path = $"{STANDARD_FOLDER}/Card_Defense_Plus3.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Defense_Plus3 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Guard +3";
            card.cardID = "defense_plus3";
            card.description = "Add +3 to your defense roll.";
            card.cardType = CardType.Defense;
            card.timing = CardTiming.DefensePhase;
            card.isStandardCard = true;
            card.faction = FactionType.None;
            card.defenseBonus = 3;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(0.3f, 0.3f, 1f); // Dark blue

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Defense_Plus3");
        }
        #endregion

        #region Faction Cards (Placeholders)
        private static void CreateFactionCard1()
        {
            string path = $"{FACTION_FOLDER}/Card_Faction_Placeholder1.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Faction_Placeholder1 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Faction Special 1";
            card.cardID = "faction_special_1";
            card.description = "PLACEHOLDER: Customize this for each faction. Example: Reroll all 1s.";
            card.cardType = CardType.Special;
            card.timing = CardTiming.AttackPhase;
            card.isStandardCard = false;
            card.faction = FactionType.None; // SET THIS PER FACTION
            card.rerollOnes = true;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(1f, 1f, 0.5f); // Yellow

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Faction_Placeholder1");
        }

        private static void CreateFactionCard2()
        {
            string path = $"{FACTION_FOLDER}/Card_Faction_Placeholder2.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Faction_Placeholder2 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Faction Special 2";
            card.cardID = "faction_special_2";
            card.description = "PLACEHOLDER: Customize this for each faction. Example: +2 damage.";
            card.cardType = CardType.Special;
            card.timing = CardTiming.DamagePhase;
            card.isStandardCard = false;
            card.faction = FactionType.None; // SET THIS PER FACTION
            card.bonusDamage = 2;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = false;
            card.cardColor = new Color(0.5f, 1f, 0.5f); // Green

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Faction_Placeholder2");
        }

        private static void CreateFactionCard3()
        {
            string path = $"{FACTION_FOLDER}/Card_Faction_Placeholder3.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Faction_Placeholder3 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Faction Special 3";
            card.cardID = "faction_special_3";
            card.description = "PLACEHOLDER: Customize this for each faction. Example: Ignore armor.";
            card.cardType = CardType.Special;
            card.timing = CardTiming.DamagePhase;
            card.isStandardCard = false;
            card.faction = FactionType.None; // SET THIS PER FACTION
            card.ignoreArmor = true;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = true; // One use per battle!
            card.cardColor = new Color(1f, 0.5f, 1f); // Purple

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Faction_Placeholder3");
        }

        private static void CreateFactionCard4()
        {
            string path = $"{FACTION_FOLDER}/Card_Faction_Placeholder4.asset";
            if (AssetDatabase.LoadAssetAtPath<CardData>(path) != null)
            {
                Debug.Log("  - Card_Faction_Placeholder4 already exists, skipping");
                return;
            }

            CardData card = ScriptableObject.CreateInstance<CardData>();
            card.cardName = "Faction Special 4";
            card.cardID = "faction_special_4";
            card.description = "PLACEHOLDER: Customize this for each faction. Example: Prevent all damage.";
            card.cardType = CardType.Defense;
            card.timing = CardTiming.DefensePhase;
            card.isStandardCard = false;
            card.faction = FactionType.None; // SET THIS PER FACTION
            card.preventDamage = true;
            card.oneUsePerTurn = true;
            card.oneUsePerBattle = true; // One use per battle!
            card.cardColor = new Color(0.5f, 1f, 1f); // Cyan

            AssetDatabase.CreateAsset(card, path);
            Debug.Log("  - Created Card_Faction_Placeholder4");
        }
        #endregion
    }
}
