using UnityEngine;
using Warslammer.Units;
using Warslammer.Utilities;

namespace Warslammer.Combat
{
    /// <summary>
    /// Calculates if targets are in weapon range (melee and ranged)
    /// Handles different range types and special cases
    /// </summary>
    public class RangeCalculator : MonoBehaviour
    {
        #region Singleton
        private static RangeCalculator _instance;
        
        /// <summary>
        /// Global access point for the RangeCalculator
        /// </summary>
        public static RangeCalculator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RangeCalculator>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("RangeCalculator");
                        _instance = go.AddComponent<RangeCalculator>();
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

        #region Range Checks
        /// <summary>
        /// Check if target is in weapon range
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="target">Target unit</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>True if target is in range</returns>
        public bool IsInRange(Unit attacker, Unit target, WeaponData weapon)
        {
            if (attacker == null || target == null || weapon == null)
                return false;

            float distance = GetDistanceInches(attacker, target);
            return weapon.IsInRange(distance);
        }

        /// <summary>
        /// Check if a position is in weapon range
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>True if position is in range</returns>
        public bool IsInRange(Unit attacker, Vector3 targetPosition, WeaponData weapon)
        {
            if (attacker == null || weapon == null)
                return false;

            float distance = MeasurementUtility.GetDistanceInches(attacker.Position, targetPosition);
            return weapon.IsInRange(distance);
        }

        /// <summary>
        /// Get distance between two units in inches
        /// </summary>
        /// <param name="from">From unit</param>
        /// <param name="to">To unit</param>
        /// <returns>Distance in inches</returns>
        public float GetDistanceInches(Unit from, Unit to)
        {
            if (from == null || to == null)
                return float.MaxValue;

            return MeasurementUtility.GetDistanceInches(from.Position, to.Position);
        }

        /// <summary>
        /// Get maximum weapon range for a unit
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        /// <returns>Maximum range in inches</returns>
        public float GetMaxRange(WeaponData weapon)
        {
            if (weapon == null)
                return 0f;

            return weapon.maxRangeInches;
        }

        /// <summary>
        /// Get minimum weapon range for a unit
        /// </summary>
        /// <param name="weapon">Weapon to check</param>
        /// <returns>Minimum range in inches</returns>
        public float GetMinRange(WeaponData weapon)
        {
            if (weapon == null)
                return 0f;

            return weapon.minRangeInches;
        }
        #endregion

        #region Melee Range
        /// <summary>
        /// Check if target is in melee range
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="target">Target unit</param>
        /// <returns>True if in melee range</returns>
        public bool IsInMeleeRange(Unit attacker, Unit target)
        {
            if (attacker == null || target == null)
                return false;

            // Melee range is typically 1-2 inches depending on weapon
            float distance = GetDistanceInches(attacker, target);
            return distance <= 2f; // Standard melee range
        }

        /// <summary>
        /// Check if unit is engaged in melee with any enemy
        /// </summary>
        /// <param name="unit">Unit to check</param>
        /// <param name="enemyUnits">List of potential enemies</param>
        /// <returns>True if engaged</returns>
        public bool IsEngagedInMelee(Unit unit, System.Collections.Generic.List<Unit> enemyUnits)
        {
            if (unit == null || enemyUnits == null)
                return false;

            foreach (Unit enemy in enemyUnits)
            {
                if (enemy != null && IsInMeleeRange(unit, enemy))
                    return true;
            }

            return false;
        }
        #endregion

        #region Range Bands
        /// <summary>
        /// Get range band for distance
        /// </summary>
        /// <param name="distance">Distance in inches</param>
        /// <param name="weapon">Weapon</param>
        /// <returns>Range band (0=out of range, 1=long, 2=medium, 3=short)</returns>
        public int GetRangeBand(float distance, WeaponData weapon)
        {
            if (weapon == null || !weapon.IsInRange(distance))
                return 0; // Out of range

            float maxRange = weapon.maxRangeInches;
            float shortRange = maxRange * 0.33f;
            float mediumRange = maxRange * 0.66f;

            if (distance <= shortRange)
                return 3; // Short range
            else if (distance <= mediumRange)
                return 2; // Medium range
            else
                return 1; // Long range
        }

        /// <summary>
        /// Check if attack is at long range
        /// </summary>
        /// <param name="distance">Distance in inches</param>
        /// <param name="weapon">Weapon</param>
        /// <returns>True if long range</returns>
        public bool IsLongRange(float distance, WeaponData weapon)
        {
            return GetRangeBand(distance, weapon) == 1;
        }

        /// <summary>
        /// Check if attack is at short range
        /// </summary>
        /// <param name="distance">Distance in inches</param>
        /// <param name="weapon">Weapon</param>
        /// <returns>True if short range</returns>
        public bool IsShortRange(float distance, WeaponData weapon)
        {
            return GetRangeBand(distance, weapon) == 3;
        }
        #endregion

        #region Area of Effect
        /// <summary>
        /// Get all units within blast radius of a position
        /// </summary>
        /// <param name="centerPosition">Center of blast</param>
        /// <param name="blastRadius">Radius in inches</param>
        /// <param name="allUnits">All units to check</param>
        /// <returns>List of units in blast radius</returns>
        public System.Collections.Generic.List<Unit> GetUnitsInBlastRadius(
            Vector3 centerPosition, 
            float blastRadius, 
            System.Collections.Generic.List<Unit> allUnits)
        {
            System.Collections.Generic.List<Unit> unitsInRange = new System.Collections.Generic.List<Unit>();

            if (allUnits == null)
                return unitsInRange;

            foreach (Unit unit in allUnits)
            {
                if (unit == null)
                    continue;

                float distance = MeasurementUtility.GetDistanceInches(centerPosition, unit.Position);
                if (distance <= blastRadius)
                {
                    unitsInRange.Add(unit);
                }
            }

            return unitsInRange;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Get closest target in range
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="potentialTargets">List of potential targets</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>Closest target in range, or null</returns>
        public Unit GetClosestTargetInRange(Unit attacker, System.Collections.Generic.List<Unit> potentialTargets, WeaponData weapon)
        {
            if (attacker == null || potentialTargets == null || weapon == null)
                return null;

            Unit closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Unit target in potentialTargets)
            {
                if (target == null || target == attacker)
                    continue;

                float distance = GetDistanceInches(attacker, target);
                
                if (weapon.IsInRange(distance) && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }

            return closestTarget;
        }

        /// <summary>
        /// Get all targets in range
        /// </summary>
        /// <param name="attacker">Attacking unit</param>
        /// <param name="potentialTargets">List of potential targets</param>
        /// <param name="weapon">Weapon to use</param>
        /// <returns>List of targets in range</returns>
        public System.Collections.Generic.List<Unit> GetAllTargetsInRange(
            Unit attacker, 
            System.Collections.Generic.List<Unit> potentialTargets, 
            WeaponData weapon)
        {
            System.Collections.Generic.List<Unit> targetsInRange = new System.Collections.Generic.List<Unit>();

            if (attacker == null || potentialTargets == null || weapon == null)
                return targetsInRange;

            foreach (Unit target in potentialTargets)
            {
                if (target == null || target == attacker)
                    continue;

                if (IsInRange(attacker, target, weapon))
                {
                    targetsInRange.Add(target);
                }
            }

            return targetsInRange;
        }
        #endregion
    }
}