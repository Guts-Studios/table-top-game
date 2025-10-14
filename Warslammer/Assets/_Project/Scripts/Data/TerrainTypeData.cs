using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining terrain type properties
    /// </summary>
    [CreateAssetMenu(fileName = "New Terrain Type", menuName = "Warslammer/Terrain Type Data")]
    public class TerrainTypeData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the terrain type")]
        public string terrainName;
        
        [Tooltip("Unique identifier for the terrain type")]
        public string terrainID;
        
        [Tooltip("Description of the terrain")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Type of terrain")]
        public TerrainType terrainType;

        [Header("Movement Effects")]
        [Tooltip("Movement cost multiplier (1.0 = normal, 2.0 = half speed, etc.)")]
        [Range(0.1f, 5.0f)]
        public float movementCostMultiplier = 1.0f;
        
        [Tooltip("Is this terrain impassable to non-flying units?")]
        public bool isImpassable = false;
        
        [Tooltip("Does this terrain block line of sight?")]
        public bool blocksLineOfSight = false;
        
        [Tooltip("Height of terrain for line of sight calculations")]
        public float terrainHeight = 0f;

        [Header("Cover & Defense")]
        [Tooltip("Does this terrain provide cover?")]
        public bool providesCover = false;
        
        [Tooltip("Defense bonus for units in this terrain")]
        public int defenseBonus = 0;
        
        [Tooltip("Armor bonus for units in this terrain")]
        public int armorBonus = 0;
        
        [Tooltip("Penalty to ranged attacks targeting units in this terrain")]
        public int rangedAttackPenalty = 0;

        [Header("Special Effects")]
        [Tooltip("Does entering this terrain cause damage?")]
        public bool causesDamage = false;
        
        [Tooltip("Damage per turn for units in this terrain")]
        public int damagePerTurn = 0;
        
        [Tooltip("Type of damage dealt")]
        public DamageSource damageType = DamageSource.Physical;
        
        [Tooltip("Does this terrain heal units?")]
        public bool providesHealing = false;
        
        [Tooltip("Healing per turn for units in this terrain")]
        public int healingPerTurn = 0;

        [Header("Elevation")]
        [Tooltip("Is this elevated terrain (hill, building)?")]
        public bool isElevated = false;
        
        [Tooltip("Elevation level (for height advantage calculations)")]
        public int elevationLevel = 0;
        
        [Tooltip("Attack bonus for units on this elevated terrain")]
        public int elevationAttackBonus = 0;
        
        [Tooltip("Range bonus for ranged units on this terrain")]
        public float rangeBonus = 0f;

        [Header("Visual")]
        [Tooltip("Sprite or texture for this terrain")]
        public Sprite terrainSprite;
        
        [Tooltip("Prefab for 3D terrain representation")]
        public GameObject terrainPrefab;
        
        [Tooltip("Color tint for terrain highlighting")]
        public Color terrainColor = Color.white;

        [Header("Special Rules")]
        [Tooltip("Special rules for this terrain type")]
        [TextArea(2, 4)]
        public string specialRules;

        /// <summary>
        /// Calculate the actual movement cost for moving through this terrain
        /// </summary>
        public float CalculateMovementCost(float baseMovement)
        {
            if (isImpassable) return float.MaxValue;
            return baseMovement * movementCostMultiplier;
        }

        /// <summary>
        /// Get the total defensive bonus from this terrain
        /// </summary>
        public int GetTotalDefensiveBonus()
        {
            return defenseBonus + armorBonus;
        }

        /// <summary>
        /// Check if this terrain provides any bonuses
        /// </summary>
        public bool HasBonuses()
        {
            return defenseBonus > 0 || armorBonus > 0 || elevationAttackBonus > 0 || 
                   rangeBonus > 0 || providesHealing;
        }

        /// <summary>
        /// Check if this terrain has negative effects
        /// </summary>
        public bool HasPenalties()
        {
            return causesDamage || movementCostMultiplier > 1.0f || isImpassable;
        }

        /// <summary>
        /// Validate the terrain data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(terrainID))
            {
                terrainID = name.Replace(" ", "_").ToLower();
            }

            // Impassable terrain shouldn't provide bonuses
            if (isImpassable && HasBonuses())
            {
                Debug.LogWarning($"Terrain {terrainName} is impassable but provides bonuses!");
            }

            // Elevated terrain should have elevation level set
            if (isElevated && elevationLevel <= 0)
            {
                Debug.LogWarning($"Terrain {terrainName} is elevated but has no elevation level set!");
            }

            // Damaging terrain should have damage amount set
            if (causesDamage && damagePerTurn <= 0)
            {
                Debug.LogWarning($"Terrain {terrainName} causes damage but has no damage amount set!");
            }

            // Healing terrain should have healing amount set
            if (providesHealing && healingPerTurn <= 0)
            {
                Debug.LogWarning($"Terrain {terrainName} provides healing but has no healing amount set!");
            }
        }
    }
}