using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining equipment that units can use
    /// </summary>
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Warslammer/Equipment Data")]
    public class EquipmentData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the equipment")]
        public string equipmentName;
        
        [Tooltip("Unique identifier for the equipment")]
        public string equipmentID;
        
        [Tooltip("Description of the equipment")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Type of equipment")]
        public EquipmentType equipmentType;
        
        [Tooltip("Points cost to add to a unit")]
        public int pointsCost = 0;

        [Header("Weapon Stats")]
        [Tooltip("Type of weapon (if this is a weapon)")]
        public WeaponType weaponType;
        
        [Tooltip("Attack bonus provided")]
        public int attackBonus = 0;
        
        [Tooltip("Damage bonus provided")]
        public int damageBonus = 0;
        
        [Tooltip("Range in inches (for ranged weapons)")]
        public float range = 0f;
        
        [Tooltip("Type of damage dealt")]
        public DamageSource damageType = DamageSource.Physical;
        
        [Tooltip("Number of additional attacks")]
        public int additionalAttacks = 0;

        [Header("Armor Stats")]
        [Tooltip("Armor bonus provided")]
        public int armorBonus = 0;
        
        [Tooltip("Defense bonus provided")]
        public int defenseBonus = 0;

        [Header("Movement Modifiers")]
        [Tooltip("Movement speed modifier")]
        public float movementModifier = 0f;
        
        [Tooltip("Does this equipment allow flying?")]
        public bool grantsFlying = false;

        [Header("Special Abilities")]
        [Tooltip("Abilities granted by this equipment")]
        public AbilityData grantedAbility;
        
        [Tooltip("Special rules provided by this equipment")]
        [TextArea(2, 4)]
        public string specialRules;

        [Header("Restrictions")]
        [Tooltip("Unit types that can use this equipment")]
        public UnitType[] allowedUnitTypes;
        
        [Tooltip("Is this equipment unique (only one per army)?")]
        public bool isUnique = false;

        [Header("Visual")]
        [Tooltip("Icon for the equipment")]
        public Sprite equipmentIcon;

        /// <summary>
        /// Check if a unit type can use this equipment
        /// </summary>
        public bool CanBeUsedBy(UnitType unitType)
        {
            if (allowedUnitTypes == null || allowedUnitTypes.Length == 0)
                return true; // No restrictions
            
            foreach (var allowedType in allowedUnitTypes)
            {
                if (allowedType == unitType)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get the total stat bonus value for display
        /// </summary>
        public int GetTotalStatBonus()
        {
            return attackBonus + damageBonus + armorBonus + defenseBonus;
        }

        /// <summary>
        /// Validate the equipment data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(equipmentID))
            {
                equipmentID = name.Replace(" ", "_").ToLower();
            }

            // Warn if weapon type is set but not marked as weapon
            if (equipmentType != EquipmentType.Weapon && weaponType != WeaponType.Sword)
            {
                if (attackBonus > 0 || damageBonus > 0 || range > 0)
                {
                    Debug.LogWarning($"Equipment {equipmentName} has weapon stats but is not marked as a weapon!");
                }
            }

            // Warn if armor type but no armor bonus
            if (equipmentType == EquipmentType.Armor && armorBonus <= 0)
            {
                Debug.LogWarning($"Equipment {equipmentName} is armor but provides no armor bonus!");
            }
        }
    }
}