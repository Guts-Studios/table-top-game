using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining AI behavior profile
    /// </summary>
    [CreateAssetMenu(fileName = "New AI Profile", menuName = "Warslammer/AI Profile Data")]
    public class AIProfileData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the AI profile")]
        public string profileName;
        
        [Tooltip("Unique identifier for the profile")]
        public string profileID;
        
        [Tooltip("Description of AI behavior")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("AI difficulty level")]
        public AIDifficulty difficulty = AIDifficulty.Normal;

        [Header("Aggression")]
        [Tooltip("How aggressively AI pursues enemy units (0-10)")]
        [Range(0, 10)]
        public int aggression = 5;
        
        [Tooltip("Willingness to take risks (0-10)")]
        [Range(0, 10)]
        public int riskTolerance = 5;

        [Header("Tactical Preferences")]
        [Tooltip("Preference for ranged vs melee combat (0=melee, 10=ranged)")]
        [Range(0, 10)]
        public int rangedPreference = 5;
        
        [Tooltip("How much AI values unit preservation (0-10)")]
        [Range(0, 10)]
        public int defensiveness = 5;
        
        [Tooltip("Priority for capturing objectives (0-10)")]
        [Range(0, 10)]
        public int objectiveFocus = 5;

        [Header("Decision Making")]
        [Tooltip("How often AI re-evaluates strategy (turns)")]
        public int strategyRevaluationFrequency = 3;
        
        [Tooltip("Reaction time delay in seconds (for more human-like behavior)")]
        [Range(0f, 3f)]
        public float reactionDelay = 0.5f;
        
        [Tooltip("Chance to make sub-optimal moves (0-100)")]
        [Range(0, 100)]
        public int mistakeChance = 10;

        [Header("Unit Priorities")]
        [Tooltip("Priority for targeting hero units")]
        [Range(0, 10)]
        public int heroTargetPriority = 7;
        
        [Tooltip("Priority for targeting damaged units")]
        [Range(0, 10)]
        public int damagedUnitPriority = 6;
        
        [Tooltip("Priority for targeting support units")]
        [Range(0, 10)]
        public int supportUnitPriority = 5;

        [Header("Special Behaviors")]
        [Tooltip("Will AI retreat when heavily damaged?")]
        public bool willRetreat = true;
        
        [Tooltip("Health threshold to trigger retreat (0-100)")]
        [Range(0, 100)]
        public int retreatHealthThreshold = 30;
        
        [Tooltip("Will AI use abilities aggressively?")]
        public bool usesAbilitiesAggressively = true;
        
        [Tooltip("Will AI focus fire on single targets?")]
        public bool prefersFocusFire = false;

        // TODO: Implement advanced AI behavior modifiers
        // - Flanking preference
        // - Cover usage priority
        // - Ability usage strategies
        // - Formation preferences

        /// <summary>
        /// Get a weighted priority score for targeting a unit
        /// </summary>
        public float CalculateTargetPriority(UnitType unitType, float healthPercent, bool isSupport)
        {
            float priority = 5f; // Base priority

            // Adjust for unit type
            if (unitType == UnitType.Hero)
                priority += heroTargetPriority * 0.5f;

            // Adjust for health
            if (healthPercent < 50f)
                priority += damagedUnitPriority * 0.5f;

            // Adjust for support role
            if (isSupport)
                priority += supportUnitPriority * 0.5f;

            return priority;
        }

        /// <summary>
        /// Check if AI should retreat based on health
        /// </summary>
        public bool ShouldRetreat(float healthPercent)
        {
            if (!willRetreat) return false;
            return healthPercent <= retreatHealthThreshold;
        }

        /// <summary>
        /// Get the decision delay for this AI profile
        /// </summary>
        public float GetDecisionDelay()
        {
            return reactionDelay;
        }

        /// <summary>
        /// Check if AI should make a mistake this turn
        /// </summary>
        public bool ShouldMakeMistake()
        {
            return Random.Range(0, 100) < mistakeChance;
        }

        /// <summary>
        /// Validate the AI profile data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(profileID))
            {
                profileID = name.Replace(" ", "_").ToLower();
            }

            // Adjust mistake chance based on difficulty
            switch (difficulty)
            {
                case AIDifficulty.Easy:
                    if (mistakeChance < 20)
                        Debug.LogWarning($"AI Profile {profileName} is Easy but has low mistake chance!");
                    break;
                case AIDifficulty.Expert:
                    if (mistakeChance > 5)
                        Debug.LogWarning($"AI Profile {profileName} is Expert but has high mistake chance!");
                    break;
            }
        }
    }
}