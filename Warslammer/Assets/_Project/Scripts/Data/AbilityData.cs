using System.Collections.Generic;
using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Data
{
    /// <summary>
    /// ScriptableObject defining an ability that units can use
    /// </summary>
    [CreateAssetMenu(fileName = "New Ability", menuName = "Warslammer/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Name of the ability")]
        public string abilityName;
        
        [Tooltip("Unique identifier for the ability")]
        public string abilityID;
        
        [Tooltip("Description of what the ability does")]
        [TextArea(3, 6)]
        public string description;
        
        [Tooltip("Type of ability")]
        public AbilityType abilityType;
        
        [Tooltip("When this ability can be used")]
        public AbilityTiming timing;

        [Header("Costs")]
        [Tooltip("Action points required to use")]
        public int actionPointCost = 1;
        
        [Tooltip("Cooldown in turns before it can be used again")]
        public int cooldownTurns = 0;
        
        [Tooltip("Health cost to use this ability")]
        public int healthCost = 0;

        [Header("Range & Targeting")]
        [Tooltip("Type of range for this ability")]
        public RangeType rangeType = RangeType.Self;
        
        [Tooltip("Range in inches (0 for self/melee)")]
        public float range = 0f;
        
        [Tooltip("Area of effect radius in inches")]
        public float areaOfEffect = 0f;
        
        [Tooltip("Can target friendly units")]
        public bool canTargetAllies = false;
        
        [Tooltip("Can target enemy units")]
        public bool canTargetEnemies = true;
        
        [Tooltip("Requires line of sight")]
        public bool requiresLineOfSight = true;

        [Header("Effects")]
        [Tooltip("Base damage dealt (if any)")]
        public int damage = 0;
        
        [Tooltip("Type of damage dealt")]
        public DamageSource damageType = DamageSource.Physical;
        
        [Tooltip("Healing provided (if any)")]
        public int healing = 0;
        
        [Tooltip("Duration of effect in turns (0 for instant)")]
        public int duration = 0;

        [Header("Stat Modifiers")]
        [Tooltip("Bonus/penalty to attack")]
        public int attackModifier = 0;
        
        [Tooltip("Bonus/penalty to defense")]
        public int defenseModifier = 0;
        
        [Tooltip("Bonus/penalty to movement")]
        public float movementModifier = 0f;
        
        [Tooltip("Bonus/penalty to armor")]
        public int armorModifier = 0;

        [Header("Status Effects")]
        [Tooltip("Does this ability stun the target?")]
        public bool appliesStun = false;
        
        [Tooltip("Does this ability apply poison?")]
        public bool appliesPoison = false;
        
        [Tooltip("Does this ability knock back the target?")]
        public bool appliesKnockback = false;
        
        [Tooltip("Knockback distance in inches")]
        public float knockbackDistance = 0f;

        [Header("Special Rules")]
        [Tooltip("Special rules or effects not covered by standard fields")]
        [TextArea(2, 4)]
        public string specialRules;
        
        [Tooltip("Can this ability be used while engaged in melee?")]
        public bool usableInMelee = true;
        
        [Tooltip("Does this ability end the unit's activation?")]
        public bool endsActivation = false;

        [Header("Visual & Audio")]
        [Tooltip("Icon for the ability")]
        public Sprite abilityIcon;
        
        [Tooltip("VFX prefab to spawn when used")]
        public GameObject vfxPrefab;
        
        [Tooltip("Sound effect to play when used")]
        public AudioClip soundEffect;

        [Header("AI Behavior")]
        [Tooltip("Priority for AI to use this ability (higher = more likely)")]
        [Range(0, 10)]
        public int aiPriority = 5;
        
        [Tooltip("Minimum health percentage before AI will use (0-100)")]
        [Range(0, 100)]
        public int aiMinHealthPercent = 0;
        
        [Tooltip("Maximum health percentage before AI will use (0-100)")]
        [Range(0, 100)]
        public int aiMaxHealthPercent = 100;

        /// <summary>
        /// Check if this ability can be used on a specific target
        /// </summary>
        public bool CanTarget(bool isAlly, bool isEnemy)
        {
            if (rangeType == RangeType.Self) return false;
            if (isAlly && !canTargetAllies) return false;
            if (isEnemy && !canTargetEnemies) return false;
            return true;
        }

        /// <summary>
        /// Check if this ability is on cooldown
        /// </summary>
        public bool IsOnCooldown(int turnsSinceLastUse)
        {
            return turnsSinceLastUse < cooldownTurns;
        }

        /// <summary>
        /// Check if this ability deals damage
        /// </summary>
        public bool DealsDamage()
        {
            return damage > 0;
        }

        /// <summary>
        /// Check if this ability provides healing
        /// </summary>
        public bool ProvidesHealing()
        {
            return healing > 0;
        }

        /// <summary>
        /// Check if this ability applies any status effects
        /// </summary>
        public bool HasStatusEffects()
        {
            return appliesStun || appliesPoison || appliesKnockback;
        }

        /// <summary>
        /// Validate the ability data
        /// </summary>
        private void OnValidate()
        {
            // Ensure unique ID is set
            if (string.IsNullOrEmpty(abilityID))
            {
                abilityID = name.Replace(" ", "_").ToLower();
            }

            // Ensure ranged abilities have range set
            if (rangeType == RangeType.Ranged && range <= 0f)
            {
                Debug.LogWarning($"Ability {abilityName} is ranged but has no range set!");
            }

            // Ensure aura abilities have AOE set
            if (rangeType == RangeType.Aura && areaOfEffect <= 0f)
            {
                Debug.LogWarning($"Ability {abilityName} is an aura but has no area of effect set!");
            }

            // Ensure knockback has distance
            if (appliesKnockback && knockbackDistance <= 0f)
            {
                Debug.LogWarning($"Ability {abilityName} applies knockback but has no distance set!");
            }

            // Validate AI health ranges
            if (aiMinHealthPercent > aiMaxHealthPercent)
            {
                Debug.LogWarning($"Ability {abilityName} has invalid AI health range!");
            }
        }
    }
}