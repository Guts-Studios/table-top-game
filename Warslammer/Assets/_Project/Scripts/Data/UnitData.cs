using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining all data for a unit type
    /// </summary>
    [CreateAssetMenu(fileName = "New Unit", menuName = "Warslammer/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Display name of the unit")]
        public string unitName;
        
        [Tooltip("Unique identifier for the unit")]
        public string unitID;
        
        [Tooltip("Description of the unit")]
        [TextArea(3, 6)]
        public string description;
        
        [Tooltip("Type of unit")]
        public UnitType unitType;
        
        [Tooltip("Size of the unit's base")]
        public BaseSize baseSize;
        
        [Tooltip("Points cost for army building")]
        public int pointsCost;

        [Header("Faction")]
        [Tooltip("Faction this unit belongs to")]
        public FactionData faction;

        [Header("Combat Stats")]
        [Tooltip("Maximum health points")]
        public int maxHealth = 10;
        
        [Tooltip("Movement distance in inches")]
        public float movementSpeed = 6f;
        
        [Tooltip("Base armor value")]
        public int armor = 0;
        
        [Tooltip("Base attack power")]
        public int attack = 1;
        
        [Tooltip("Base defense value")]
        public int defense = 0;
        
        [Tooltip("Initiative value for turn order")]
        public int initiative = 5;
        
        [Tooltip("Leadership value for morale checks")]
        public int leadership = 7;

        [Header("Attack Attributes")]
        [Tooltip("Type of attack range")]
        public RangeType attackRange = RangeType.Melee;
        
        [Tooltip("Attack range in inches (for ranged attacks)")]
        public float attackRangeDistance = 0f;
        
        [Tooltip("Number of attacks per action")]
        public int numberOfAttacks = 1;
        
        [Tooltip("Type of damage dealt")]
        public DamageSource damageType = DamageSource.Physical;

        [Header("Resistances")]
        [Tooltip("Damage resistances (reduced damage from these sources)")]
        public List<DamageSource> resistances = new List<DamageSource>();
        
        [Tooltip("Damage vulnerabilities (increased damage from these sources)")]
        public List<DamageSource> vulnerabilities = new List<DamageSource>();
        
        [Tooltip("Damage immunities (no damage from these sources)")]
        public List<DamageSource> immunities = new List<DamageSource>();

        [Header("Abilities")]
        [Tooltip("List of abilities this unit has")]
        public List<AbilityData> abilities = new List<AbilityData>();

        [Header("Equipment")]
        [Tooltip("Default equipment loadout")]
        public List<EquipmentData> defaultEquipment = new List<EquipmentData>();

        [Header("Visual")]
        [Tooltip("Sprite for the unit")]
        public Sprite unitSprite;
        
        [Tooltip("Prefab for the unit (if using 3D model)")]
        public GameObject unitPrefab;
        
        [Tooltip("Icon for UI display")]
        public Sprite unitIcon;

        [Header("Terrain Interactions")]
        [Tooltip("Can this unit fly over terrain?")]
        public bool canFly = false;
        
        [Tooltip("Terrain types this unit cannot move through")]
        public List<TerrainType> impassableTerrain = new List<TerrainType>();

        [Header("Special Rules")]
        [Tooltip("Can this unit be a general/leader?")]
        public bool canBeGeneral = false;
        
        [Tooltip("Does this unit provide aura bonuses?")]
        public bool hasAura = false;
        
        [Tooltip("Aura radius in inches")]
        public float auraRadius = 0f;

        /// <summary>
        /// Calculate the total points cost including equipment
        /// </summary>
        public int GetTotalPointsCost()
        {
            int total = pointsCost;
            foreach (var equipment in defaultEquipment)
            {
                if (equipment != null)
                {
                    total += equipment.pointsCost;
                }
            }
            return total;
        }

        /// <summary>
        /// Check if this unit is resistant to a damage type
        /// </summary>
        public bool IsResistantTo(DamageSource damageSource)
        {
            return resistances.Contains(damageSource);
        }

        /// <summary>
        /// Check if this unit is vulnerable to a damage type
        /// </summary>
        public bool IsVulnerableTo(DamageSource damageSource)
        {
            return vulnerabilities.Contains(damageSource);
        }

        /// <summary>
        /// Check if this unit is immune to a damage type
        /// </summary>
        public bool IsImmuneTo(DamageSource damageSource)
        {
            return immunities.Contains(damageSource);
        }

        /// <summary>
        /// Validate the unit data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(unitID))
            {
                unitID = name.Replace(" ", "_").ToLower();
            }

            // Ensure ranged units have a range set
            if (attackRange == RangeType.Ranged && attackRangeDistance <= 0f)
            {
                Debug.LogWarning($"Unit {unitName} is ranged but has no attack range set!");
            }

            // Ensure aura units have a radius
            if (hasAura && auraRadius <= 0f)
            {
                Debug.LogWarning($"Unit {unitName} has aura but no radius set!");
            }
        }
    }
}