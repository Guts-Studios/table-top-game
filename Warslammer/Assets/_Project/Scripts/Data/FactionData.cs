using System.Collections.Generic;
using UnityEngine;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining a faction/army
    /// </summary>
    [CreateAssetMenu(fileName = "New Faction", menuName = "Warslammer/Faction Data")]
    public class FactionData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the faction")]
        public string factionName;
        
        [Tooltip("Unique identifier for the faction")]
        public string factionID;
        
        [Tooltip("Description of the faction's lore and playstyle")]
        [TextArea(4, 8)]
        public string description;
        
        [Tooltip("Faction color scheme for UI")]
        public Color primaryColor = Color.white;
        
        [Tooltip("Secondary faction color")]
        public Color secondaryColor = Color.gray;

        [Header("Visual")]
        [Tooltip("Faction emblem/icon")]
        public Sprite factionIcon;
        
        [Tooltip("Faction banner sprite")]
        public Sprite factionBanner;

        [Header("Available Units")]
        [Tooltip("All units available to this faction")]
        public List<UnitData> availableUnits = new List<UnitData>();
        
        [Tooltip("Hero/Leader units for this faction")]
        public List<UnitData> heroUnits = new List<UnitData>();

        [Header("Faction Traits")]
        [Tooltip("Special rules or abilities unique to this faction")]
        [TextArea(3, 6)]
        public string factionTraits;
        
        [Tooltip("Faction-wide passive abilities")]
        public List<AbilityData> factionAbilities = new List<AbilityData>();

        [Header("Army Building")]
        [Tooltip("Minimum points for a legal army")]
        public int minimumArmyPoints = 500;
        
        [Tooltip("Can units from other factions be included?")]
        public bool allowAllies = false;
        
        [Tooltip("Factions that can be taken as allies")]
        public List<FactionData> allowedAllies = new List<FactionData>();

        [Header("Lore")]
        [Tooltip("Background story of the faction")]
        [TextArea(5, 10)]
        public string lore;

        /// <summary>
        /// Check if a unit belongs to this faction
        /// </summary>
        public bool ContainsUnit(UnitData unit)
        {
            return availableUnits.Contains(unit) || heroUnits.Contains(unit);
        }

        /// <summary>
        /// Get all units including heroes
        /// </summary>
        public List<UnitData> GetAllUnits()
        {
            List<UnitData> allUnits = new List<UnitData>();
            allUnits.AddRange(availableUnits);
            allUnits.AddRange(heroUnits);
            return allUnits;
        }

        /// <summary>
        /// Check if another faction can be taken as allies
        /// </summary>
        public bool CanAllyWith(FactionData otherFaction)
        {
            if (!allowAllies) return false;
            return allowedAllies.Contains(otherFaction);
        }

        /// <summary>
        /// Validate the faction data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(factionID))
            {
                factionID = name.Replace(" ", "_").ToLower();
            }

            // Warn if no units are assigned
            if (availableUnits.Count == 0 && heroUnits.Count == 0)
            {
                Debug.LogWarning($"Faction {factionName} has no units assigned!");
            }
        }
    }
}