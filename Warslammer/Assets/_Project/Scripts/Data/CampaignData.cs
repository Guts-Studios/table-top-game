using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining a campaign
    /// </summary>
    [CreateAssetMenu(fileName = "New Campaign", menuName = "Warslammer/Campaign Data")]
    public class CampaignData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the campaign")]
        public string campaignName;
        
        [Tooltip("Unique identifier for the campaign")]
        public string campaignID;
        
        [Tooltip("Description of the campaign")]
        [TextArea(3, 6)]
        public string description;
        
        [Tooltip("Difficulty level")]
        public AIDifficulty difficulty = AIDifficulty.Normal;

        [Header("Story")]
        [Tooltip("Campaign background story")]
        [TextArea(5, 10)]
        public string storyText;
        
        [Tooltip("Campaign introduction/briefing")]
        [TextArea(3, 6)]
        public string introductionText;

        [Header("Missions")]
        [Tooltip("List of missions in this campaign (in order)")]
        public List<MissionData> missions = new List<MissionData>();
        
        [Tooltip("Are missions locked until previous ones are completed?")]
        public bool missionsAreSequential = true;

        [Header("Rewards")]
        [Tooltip("Units unlocked upon campaign completion")]
        public List<UnitData> unlockedUnits = new List<UnitData>();
        
        [Tooltip("Abilities unlocked upon campaign completion")]
        public List<AbilityData> unlockedAbilities = new List<AbilityData>();
        
        [Tooltip("Equipment unlocked upon campaign completion")]
        public List<EquipmentData> unlockedEquipment = new List<EquipmentData>();

        [Header("Progression")]
        [Tooltip("Starting army points for first mission")]
        public int startingArmyPoints = 500;
        
        [Tooltip("Army points gained per mission completed")]
        public int armyPointsPerMission = 100;
        
        [Tooltip("Maximum army points for final mission")]
        public int maxArmyPoints = 2000;

        [Header("Player Faction")]
        [Tooltip("Faction the player uses in this campaign")]
        public FactionData playerFaction;
        
        [Tooltip("Starting units for the campaign")]
        public List<UnitData> startingUnits = new List<UnitData>();

        [Header("Visual")]
        [Tooltip("Campaign banner/cover image")]
        public Sprite campaignBanner;
        
        [Tooltip("Campaign icon for UI")]
        public Sprite campaignIcon;

        /// <summary>
        /// Get the number of missions in this campaign
        /// </summary>
        public int GetMissionCount()
        {
            return missions != null ? missions.Count : 0;
        }

        /// <summary>
        /// Get a mission by index
        /// </summary>
        public MissionData GetMission(int index)
        {
            if (missions == null || index < 0 || index >= missions.Count)
                return null;
            
            return missions[index];
        }

        /// <summary>
        /// Check if a mission is unlocked based on campaign progress
        /// </summary>
        public bool IsMissionUnlocked(int missionIndex, int completedMissions)
        {
            if (!missionsAreSequential)
                return true; // All missions unlocked
            
            return missionIndex <= completedMissions;
        }

        /// <summary>
        /// Calculate army points available for a given mission
        /// </summary>
        public int GetArmyPointsForMission(int missionIndex)
        {
            int points = startingArmyPoints + (missionIndex * armyPointsPerMission);
            return Mathf.Min(points, maxArmyPoints);
        }

        /// <summary>
        /// Check if the campaign is complete
        /// </summary>
        public bool IsComplete(int completedMissions)
        {
            return completedMissions >= GetMissionCount();
        }

        /// <summary>
        /// Validate the campaign data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(campaignID))
            {
                campaignID = name.Replace(" ", "_").ToLower();
            }

            // Warn if no missions
            if (missions == null || missions.Count == 0)
            {
                Debug.LogWarning($"Campaign {campaignName} has no missions!");
            }

            // Warn if no player faction
            if (playerFaction == null)
            {
                Debug.LogWarning($"Campaign {campaignName} has no player faction set!");
            }

            // Warn if no starting units
            if (startingUnits == null || startingUnits.Count == 0)
            {
                Debug.LogWarning($"Campaign {campaignName} has no starting units!");
            }
        }
    }

    /// <summary>
    /// Data for a single mission within a campaign
    /// </summary>
    [System.Serializable]
    public class MissionData
    {
        [Tooltip("Name of the mission")]
        public string missionName;
        
        [Tooltip("Mission briefing/description")]
        [TextArea(3, 5)]
        public string briefing;
        
        [Tooltip("Type of mission")]
        public MissionType missionType;
        
        [Tooltip("Victory condition for this mission")]
        public VictoryCondition victoryCondition;
        
        [Tooltip("Enemy faction for this mission")]
        public FactionData enemyFaction;
        
        [Tooltip("Enemy army points")]
        public int enemyArmyPoints = 500;
        
        [Tooltip("Turn limit (0 for unlimited)")]
        public int turnLimit = 0;
        
        [Tooltip("Special objectives for the mission")]
        [TextArea(2, 4)]
        public string specialObjectives;
        
        [Tooltip("Rewards for completing this mission")]
        [TextArea(2, 3)]
        public string rewards;
    }
}