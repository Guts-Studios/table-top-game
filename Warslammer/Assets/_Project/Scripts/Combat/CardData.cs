using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Combat
{
    /// <summary>
    /// Defines a combat card that can be played during battle
    /// Cards are faction-specific and provide combat bonuses
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "Warslammer/Combat/Card Data")]
    public class CardData : ScriptableObject
    {
        #region Basic Info
        [Header("Card Info")]
        [Tooltip("Display name of the card")]
        public string cardName = "Card";

        [Tooltip("Unique ID for this card")]
        public string cardID;

        [Tooltip("Description of what the card does")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Icon for the card")]
        public Sprite cardIcon;

        [Tooltip("Card background/frame sprite")]
        public Sprite cardFrame;
        #endregion

        #region Card Type
        [Header("Card Type")]
        [Tooltip("Type of card")]
        public CardType cardType = CardType.Attack;

        [Tooltip("When this card can be played")]
        public CardTiming timing = CardTiming.AttackPhase;

        [Tooltip("Which faction can use this card (empty = all factions)")]
        public FactionType faction = FactionType.None;

        [Tooltip("Is this a standard card (everyone has) or faction-specific?")]
        public bool isStandardCard = true;
        #endregion

        #region Effects
        [Header("Combat Modifiers")]
        [Tooltip("Bonus to attack rolls")]
        public int attackBonus = 0;

        [Tooltip("Bonus to defense rolls")]
        public int defenseBonus = 0;

        [Tooltip("Additional dice to roll")]
        public int bonusDice = 0;

        [Tooltip("Reroll failed dice")]
        public bool rerollFailed = false;

        [Tooltip("Reroll all 1s")]
        public bool rerollOnes = false;

        [Tooltip("Auto-succeed on target number")]
        public bool autoSuccess = false;

        [Tooltip("Number of auto-successes")]
        public int autoSuccessCount = 0;

        [Header("Damage Modifiers")]
        [Tooltip("Bonus damage")]
        public int bonusDamage = 0;

        [Tooltip("Ignore armor")]
        public bool ignoreArmor = false;

        [Tooltip("Multiply damage")]
        public float damageMultiplier = 1f;

        [Header("Special Effects")]
        [Tooltip("Prevent damage")]
        public bool preventDamage = false;

        [Tooltip("Heal HP")]
        public int healAmount = 0;

        [Tooltip("Apply status effect")]
        public bool appliesStatusEffect = false;

        [Tooltip("Status effect to apply")]
        public string statusEffectName = "";

        [Tooltip("Status effect duration")]
        public int statusEffectDuration = 1;
        #endregion

        #region Usage Rules
        [Header("Usage Rules")]
        [Tooltip("Can only be played once per battle")]
        public bool oneUsePerBattle = false;

        [Tooltip("Can only be played once per turn")]
        public bool oneUsePerTurn = true;

        [Tooltip("Requires target unit")]
        public bool requiresTarget = false;

        [Tooltip("Can target self")]
        public bool canTargetSelf = true;

        [Tooltip("Can target allies")]
        public bool canTargetAllies = true;

        [Tooltip("Can target enemies")]
        public bool canTargetEnemies = true;
        #endregion

        #region Visual & Audio
        [Header("Presentation")]
        [Tooltip("Color tint for card")]
        public Color cardColor = Color.white;

        [Tooltip("VFX prefab when card is played")]
        public GameObject playVFXPrefab;

        [Tooltip("Sound effect when card is played")]
        public AudioClip playSound;
        #endregion

        #region Utility Methods
        /// <summary>
        /// Get a formatted description with values
        /// </summary>
        public string GetFormattedDescription()
        {
            string desc = description;

            // Replace placeholders with actual values
            desc = desc.Replace("{attack}", attackBonus.ToString());
            desc = desc.Replace("{defense}", defenseBonus.ToString());
            desc = desc.Replace("{damage}", bonusDamage.ToString());
            desc = desc.Replace("{dice}", bonusDice.ToString());

            return desc;
        }

        /// <summary>
        /// Check if card can be played in current context
        /// </summary>
        public bool CanBePlayed(CardTiming currentTiming, FactionType playerFaction)
        {
            // Check timing
            if (timing != CardTiming.Any && timing != currentTiming)
                return false;

            // Check faction
            if (faction != FactionType.None && faction != playerFaction)
                return false;

            return true;
        }

        /// <summary>
        /// Get a quick summary of card effects
        /// </summary>
        public string GetEffectSummary()
        {
            string summary = "";

            if (attackBonus > 0)
                summary += $"+{attackBonus} ATK ";
            if (defenseBonus > 0)
                summary += $"+{defenseBonus} DEF ";
            if (bonusDice > 0)
                summary += $"+{bonusDice} Dice ";
            if (bonusDamage > 0)
                summary += $"+{bonusDamage} DMG ";
            if (rerollFailed)
                summary += "Reroll Failed ";
            if (rerollOnes)
                summary += "Reroll 1s ";
            if (ignoreArmor)
                summary += "Ignore Armor ";
            if (preventDamage)
                summary += "Prevent Damage ";
            if (healAmount > 0)
                summary += $"Heal {healAmount} ";

            return summary.Trim();
        }
        #endregion

        #region Validation
        private void OnValidate()
        {
            // Generate ID if empty
            if (string.IsNullOrEmpty(cardID))
            {
                cardID = name.ToLower().Replace(" ", "_");
            }

            // Ensure timing matches card type
            if (cardType == CardType.Attack && timing == CardTiming.DefensePhase)
            {
                Debug.LogWarning($"[CardData] {cardName} is an Attack card but has Defense timing!");
            }

            if (cardType == CardType.Defense && timing == CardTiming.AttackPhase)
            {
                Debug.LogWarning($"[CardData] {cardName} is a Defense card but has Attack timing!");
            }
        }
        #endregion
    }

    #region Enums
    /// <summary>
    /// Type of card
    /// </summary>
    public enum CardType
    {
        Attack,      // Boost attack rolls/damage
        Defense,     // Boost defense/prevent damage
        Special,     // Faction-specific unique effect
        Reaction,    // Play in response to opponent
        Utility      // Other effects
    }

    /// <summary>
    /// When a card can be played
    /// </summary>
    public enum CardTiming
    {
        Any,             // Can be played any time
        AttackPhase,     // During attacker's attack
        DefensePhase,    // During defender's defense
        BeforeRoll,      // Before dice are rolled
        AfterRoll,       // After dice are rolled
        DamagePhase      // During damage calculation
    }
    #endregion
}
