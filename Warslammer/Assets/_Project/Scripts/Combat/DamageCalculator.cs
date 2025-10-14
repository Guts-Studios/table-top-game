using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Combat
{
    /// <summary>
    /// Calculates final damage after armor saves and modifiers
    /// Handles damage reduction, penetration, and special rules
    /// </summary>
    public class DamageCalculator : MonoBehaviour
    {
        #region Singleton
        private static DamageCalculator _instance;
        
        /// <summary>
        /// Global access point for the DamageCalculator
        /// </summary>
        public static DamageCalculator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<DamageCalculator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DamageCalculator");
                        _instance = go.AddComponent<DamageCalculator>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }
        #endregion

        #region Damage Calculation
        /// <summary>
        /// Calculate final damage from attack and defense rolls
        /// </summary>
        /// <param name="baseDamage">Base weapon damage</param>
        /// <param name="attackSuccesses">Number of attack successes</param>
        /// <param name="defenseSuccesses">Number of defense blocks</param>
        /// <param name="armorValue">Defender's armor value</param>
        /// <param name="damageModifiers">Additional damage modifiers</param>
        /// <returns>Final damage amount</returns>
        public int CalculateDamage(int baseDamage, int attackSuccesses, int defenseSuccesses, int armorValue, int damageModifiers = 0)
        {
            // Calculate net successes (attacks - blocks)
            int netSuccesses = Mathf.Max(0, attackSuccesses - defenseSuccesses);
            
            // Calculate raw damage: base damage + net successes + modifiers
            int rawDamage = baseDamage + netSuccesses + damageModifiers;
            
            // Apply armor reduction
            int finalDamage = Mathf.Max(0, rawDamage - armorValue);
            
            Debug.Log($"[DamageCalculator] Base: {baseDamage}, Successes: {netSuccesses}, Modifiers: {damageModifiers}, Armor: {armorValue} => Final: {finalDamage}");
            
            return finalDamage;
        }

        /// <summary>
        /// Calculate damage with armor penetration
        /// </summary>
        /// <param name="baseDamage">Base weapon damage</param>
        /// <param name="attackSuccesses">Number of attack successes</param>
        /// <param name="defenseSuccesses">Number of defense blocks</param>
        /// <param name="armorValue">Defender's armor value</param>
        /// <param name="armorPenetration">Armor penetration value</param>
        /// <param name="damageModifiers">Additional damage modifiers</param>
        /// <returns>Final damage amount</returns>
        public int CalculateDamageWithPenetration(int baseDamage, int attackSuccesses, int defenseSuccesses, 
            int armorValue, int armorPenetration, int damageModifiers = 0)
        {
            // Calculate net successes
            int netSuccesses = Mathf.Max(0, attackSuccesses - defenseSuccesses);
            
            // Calculate raw damage
            int rawDamage = baseDamage + netSuccesses + damageModifiers;
            
            // Apply armor with penetration
            int effectiveArmor = Mathf.Max(0, armorValue - armorPenetration);
            int finalDamage = Mathf.Max(0, rawDamage - effectiveArmor);
            
            Debug.Log($"[DamageCalculator] Damage: {rawDamage}, Armor: {armorValue}, AP: {armorPenetration} => Final: {finalDamage}");
            
            return finalDamage;
        }

        /// <summary>
        /// Calculate damage bonus from critical hits
        /// </summary>
        /// <param name="baseDamage">Base damage</param>
        /// <param name="criticalCount">Number of critical hits</param>
        /// <param name="criticalMultiplier">Damage multiplier per critical (default 1.0)</param>
        /// <returns>Additional damage from criticals</returns>
        public int CalculateCriticalDamage(int baseDamage, int criticalCount, float criticalMultiplier = 1.0f)
        {
            if (criticalCount <= 0)
                return 0;

            int criticalDamage = Mathf.RoundToInt(baseDamage * criticalMultiplier * criticalCount);
            
            Debug.Log($"[DamageCalculator] Critical damage: {criticalCount} crits x {baseDamage} x {criticalMultiplier} = {criticalDamage}");
            
            return criticalDamage;
        }
        #endregion

        #region Armor Saves
        /// <summary>
        /// Calculate armor save roll needed to reduce damage
        /// </summary>
        /// <param name="armorValue">Defender's armor value</param>
        /// <param name="armorPenetration">Attack's armor penetration</param>
        /// <returns>Target number for armor save (or 0 if no save possible)</returns>
        public int CalculateArmorSaveTarget(int armorValue, int armorPenetration = 0)
        {
            int effectiveArmor = armorValue - armorPenetration;
            
            if (effectiveArmor <= 0)
                return 0; // No save possible
            
            // Convert armor value to save target (e.g., 3 armor = 4+ save)
            int saveTarget = 7 - effectiveArmor;
            return Mathf.Clamp(saveTarget, 2, 6); // Save range is 2+ to 6+
        }

        /// <summary>
        /// Check if armor save was successful
        /// </summary>
        /// <param name="rollResult">Die roll result</param>
        /// <param name="saveTarget">Target number needed</param>
        /// <returns>True if save succeeded</returns>
        public bool IsArmorSaveSuccessful(int rollResult, int saveTarget)
        {
            return rollResult >= saveTarget;
        }
        #endregion

        #region Damage Modifiers
        /// <summary>
        /// Calculate flanking bonus damage
        /// </summary>
        /// <param name="isFlanking">Is attacker flanking?</param>
        /// <returns>Damage bonus</returns>
        public int GetFlankingBonus(bool isFlanking)
        {
            return isFlanking ? 1 : 0;
        }

        /// <summary>
        /// Calculate high ground bonus damage
        /// </summary>
        /// <param name="hasHighGround">Does attacker have high ground?</param>
        /// <returns>Damage bonus</returns>
        public int GetHighGroundBonus(bool hasHighGround)
        {
            return hasHighGround ? 1 : 0;
        }

        /// <summary>
        /// Calculate charge bonus damage
        /// </summary>
        /// <param name="isCharging">Is attacker charging?</param>
        /// <returns>Damage bonus</returns>
        public int GetChargeBonus(bool isCharging)
        {
            return isCharging ? 2 : 0;
        }
        #endregion

        #region Special Rules
        /// <summary>
        /// Apply minimum damage rule (e.g., always at least 1 damage on hit)
        /// </summary>
        /// <param name="damage">Calculated damage</param>
        /// <param name="hitOccurred">Did the attack hit?</param>
        /// <returns>Damage with minimum applied</returns>
        public int ApplyMinimumDamage(int damage, bool hitOccurred)
        {
            if (hitOccurred && damage < 1)
                return 1;
            return damage;
        }

        /// <summary>
        /// Apply maximum damage cap
        /// </summary>
        /// <param name="damage">Calculated damage</param>
        /// <param name="maxDamage">Maximum damage allowed</param>
        /// <returns>Capped damage</returns>
        public int ApplyMaximumDamage(int damage, int maxDamage)
        {
            return Mathf.Min(damage, maxDamage);
        }

        /// <summary>
        /// Calculate overkill damage (for morale effects)
        /// </summary>
        /// <param name="damage">Damage dealt</param>
        /// <param name="targetCurrentHP">Target's current HP</param>
        /// <returns>Overkill amount (0 if no overkill)</returns>
        public int CalculateOverkill(int damage, int targetCurrentHP)
        {
            if (damage > targetCurrentHP)
                return damage - targetCurrentHP;
            return 0;
        }
        #endregion

        #region Damage Types
        /// <summary>
        /// Apply damage type modifier based on resistances
        /// </summary>
        /// <param name="baseDamage">Base damage amount</param>
        /// <param name="damageSource">Type of damage</param>
        /// <param name="isResistant">Is target resistant?</param>
        /// <param name="isVulnerable">Is target vulnerable?</param>
        /// <param name="isImmune">Is target immune?</param>
        /// <returns>Modified damage</returns>
        public int ApplyDamageTypeModifier(int baseDamage, DamageSource damageSource, 
            bool isResistant, bool isVulnerable, bool isImmune)
        {
            if (isImmune)
            {
                Debug.Log($"[DamageCalculator] Target is immune to {damageSource} damage!");
                return 0;
            }

            if (isResistant)
            {
                int reducedDamage = Mathf.RoundToInt(baseDamage * 0.5f);
                Debug.Log($"[DamageCalculator] Target resists {damageSource}: {baseDamage} -> {reducedDamage}");
                return reducedDamage;
            }

            if (isVulnerable)
            {
                int increasedDamage = Mathf.RoundToInt(baseDamage * 1.5f);
                Debug.Log($"[DamageCalculator] Target is vulnerable to {damageSource}: {baseDamage} -> {increasedDamage}");
                return increasedDamage;
            }

            return baseDamage;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get damage severity rating (for UI/feedback)
        /// </summary>
        /// <param name="damage">Damage amount</param>
        /// <param name="targetMaxHP">Target's max HP</param>
        /// <returns>Severity (0=none, 1=light, 2=moderate, 3=heavy, 4=critical)</returns>
        public int GetDamageSeverity(int damage, int targetMaxHP)
        {
            if (damage <= 0)
                return 0;

            float percentage = (float)damage / targetMaxHP;

            if (percentage >= 0.5f)
                return 4; // Critical
            else if (percentage >= 0.25f)
                return 3; // Heavy
            else if (percentage >= 0.1f)
                return 2; // Moderate
            else
                return 1; // Light
        }
        #endregion
    }
}