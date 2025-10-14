using UnityEngine;
using System.Collections.Generic;

namespace Warslammer.Utilities
{
    /// <summary>
    /// Utility class for collision detection between units
    /// Uses 2D circle colliders for unit bases
    /// </summary>
    public static class CollisionUtility
    {
        /// <summary>
        /// Check if two circles overlap
        /// </summary>
        /// <param name="center1">Center of first circle</param>
        /// <param name="radius1">Radius of first circle</param>
        /// <param name="center2">Center of second circle</param>
        /// <param name="radius2">Radius of second circle</param>
        /// <returns>True if circles overlap</returns>
        public static bool CirclesOverlap(Vector3 center1, float radius1, Vector3 center2, float radius2)
        {
            float distance = center1.HorizontalDistance(center2);
            return distance < (radius1 + radius2);
        }

        /// <summary>
        /// Check if a position would cause overlap with any existing unit
        /// </summary>
        /// <param name="position">Position to check</param>
        /// <param name="radius">Radius of the unit being checked</param>
        /// <param name="unitsToCheck">List of units to check against</param>
        /// <param name="ignoreUnit">Unit to ignore (typically the one being moved)</param>
        /// <returns>True if position would cause overlap</returns>
        public static bool WouldOverlapUnits(Vector3 position, float radius, List<Transform> unitsToCheck, Transform ignoreUnit = null)
        {
            foreach (Transform unit in unitsToCheck)
            {
                if (unit == ignoreUnit || unit == null)
                    continue;

                // Get the unit's radius (assuming units have a collider or we use a default)
                float otherRadius = GetUnitRadius(unit);
                
                if (CirclesOverlap(position, radius, unit.position, otherRadius))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the radius of a unit (from collider or default)
        /// </summary>
        /// <param name="unit">Unit to get radius for</param>
        /// <returns>Radius in Unity units</returns>
        public static float GetUnitRadius(Transform unit)
        {
            // Try to get radius from CircleCollider2D
            CircleCollider2D collider2D = unit.GetComponent<CircleCollider2D>();
            if (collider2D != null)
                return collider2D.radius * Mathf.Max(unit.localScale.x, unit.localScale.z);

            // Try to get radius from SphereCollider
            SphereCollider sphereCollider = unit.GetComponent<SphereCollider>();
            if (sphereCollider != null)
                return sphereCollider.radius * Mathf.Max(unit.localScale.x, unit.localScale.z);

            // Try to get radius from CapsuleCollider
            CapsuleCollider capsuleCollider = unit.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
                return capsuleCollider.radius * Mathf.Max(unit.localScale.x, unit.localScale.z);

            // Default radius (about 0.5 inches / 12.7mm)
            return 0.5f;
        }

        /// <summary>
        /// Find the nearest valid position that doesn't overlap
        /// </summary>
        /// <param name="desiredPosition">Desired position</param>
        /// <param name="radius">Radius of the unit</param>
        /// <param name="unitsToCheck">Units to avoid</param>
        /// <param name="ignoreUnit">Unit to ignore</param>
        /// <param name="maxSearchRadius">Maximum distance to search for valid position</param>
        /// <returns>Nearest valid position, or original if none found</returns>
        public static Vector3 FindNearestValidPosition(Vector3 desiredPosition, float radius, List<Transform> unitsToCheck, Transform ignoreUnit = null, float maxSearchRadius = 5f)
        {
            // If desired position is valid, return it
            if (!WouldOverlapUnits(desiredPosition, radius, unitsToCheck, ignoreUnit))
                return desiredPosition;

            // Search in a spiral pattern
            int steps = 16; // Number of angles to check
            float searchIncrement = 0.5f; // Distance increment for each ring

            for (float searchRadius = searchIncrement; searchRadius <= maxSearchRadius; searchRadius += searchIncrement)
            {
                for (int i = 0; i < steps; i++)
                {
                    float angle = (360f / steps) * i;
                    Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * searchRadius;
                    Vector3 testPosition = desiredPosition + offset;

                    if (!WouldOverlapUnits(testPosition, radius, unitsToCheck, ignoreUnit))
                        return testPosition;
                }
            }

            // No valid position found, return original
            return desiredPosition;
        }

        /// <summary>
        /// Check if a ray intersects a circle (for line of sight)
        /// </summary>
        /// <param name="rayStart">Start of ray</param>
        /// <param name="rayEnd">End of ray</param>
        /// <param name="circleCenter">Center of circle</param>
        /// <param name="circleRadius">Radius of circle</param>
        /// <returns>True if ray intersects circle</returns>
        public static bool RayIntersectsCircle(Vector3 rayStart, Vector3 rayEnd, Vector3 circleCenter, float circleRadius)
        {
            // Flatten to 2D (ignore Y)
            Vector2 start = new Vector2(rayStart.x, rayStart.z);
            Vector2 end = new Vector2(rayEnd.x, rayEnd.z);
            Vector2 center = new Vector2(circleCenter.x, circleCenter.z);

            // Calculate closest point on line segment to circle center
            Vector2 lineDir = (end - start).normalized;
            float lineLength = Vector2.Distance(start, end);
            
            Vector2 toCenter = center - start;
            float projection = Vector2.Dot(toCenter, lineDir);
            
            // Clamp projection to line segment
            projection = Mathf.Clamp(projection, 0f, lineLength);
            
            Vector2 closestPoint = start + lineDir * projection;
            float distanceToCircle = Vector2.Distance(closestPoint, center);

            return distanceToCircle <= circleRadius;
        }

        /// <summary>
        /// Get all units within a certain radius
        /// </summary>
        /// <param name="center">Center position</param>
        /// <param name="radius">Search radius</param>
        /// <param name="unitsToCheck">Units to check</param>
        /// <returns>List of units within radius</returns>
        public static List<Transform> GetUnitsInRadius(Vector3 center, float radius, List<Transform> unitsToCheck)
        {
            List<Transform> unitsInRange = new List<Transform>();

            foreach (Transform unit in unitsToCheck)
            {
                if (unit == null)
                    continue;

                float distance = center.HorizontalDistance(unit.position);
                if (distance <= radius)
                    unitsInRange.Add(unit);
            }

            return unitsInRange;
        }

        /// <summary>
        /// Check if a point is inside a circle
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="circleCenter">Center of circle</param>
        /// <param name="circleRadius">Radius of circle</param>
        /// <returns>True if point is inside circle</returns>
        public static bool PointInCircle(Vector3 point, Vector3 circleCenter, float circleRadius)
        {
            float distance = point.HorizontalDistance(circleCenter);
            return distance <= circleRadius;
        }

        /// <summary>
        /// Calculate penetration depth between two overlapping circles
        /// </summary>
        /// <param name="center1">Center of first circle</param>
        /// <param name="radius1">Radius of first circle</param>
        /// <param name="center2">Center of second circle</param>
        /// <param name="radius2">Radius of second circle</param>
        /// <returns>Penetration depth (0 if not overlapping)</returns>
        public static float GetPenetrationDepth(Vector3 center1, float radius1, Vector3 center2, float radius2)
        {
            float distance = center1.HorizontalDistance(center2);
            float combinedRadius = radius1 + radius2;
            
            if (distance >= combinedRadius)
                return 0f;

            return combinedRadius - distance;
        }
    }
}