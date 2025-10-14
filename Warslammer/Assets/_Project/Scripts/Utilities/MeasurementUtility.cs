using UnityEngine;
using Warslammer.Core;

namespace Warslammer.Utilities
{
    /// <summary>
    /// Utility class for distance measurements and unit conversions
    /// Handles conversion between Unity units and tabletop inches
    /// </summary>
    public static class MeasurementUtility
    {
        /// <summary>
        /// Default scale: 1 Unity unit = 1 inch
        /// Can be modified for different battlefield scales
        /// </summary>
        public static float UnitsPerInch = 1f;

        /// <summary>
        /// Convert Unity units to inches
        /// </summary>
        /// <param name="unityUnits">Distance in Unity units</param>
        /// <returns>Distance in inches</returns>
        public static float UnityUnitsToInches(float unityUnits)
        {
            return unityUnits / UnitsPerInch;
        }

        /// <summary>
        /// Convert inches to Unity units
        /// </summary>
        /// <param name="inches">Distance in inches</param>
        /// <returns>Distance in Unity units</returns>
        public static float InchesToUnityUnits(float inches)
        {
            return inches * UnitsPerInch;
        }

        /// <summary>
        /// Get horizontal distance between two positions in inches
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Target position</param>
        /// <returns>Distance in inches</returns>
        public static float GetDistanceInches(Vector3 from, Vector3 to)
        {
            float unityDistance = from.HorizontalDistance(to);
            return UnityUnitsToInches(unityDistance);
        }

        /// <summary>
        /// Check if target is within range (in inches)
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Target position</param>
        /// <param name="rangeInches">Maximum range in inches</param>
        /// <returns>True if target is within range</returns>
        public static bool IsWithinRange(Vector3 from, Vector3 to, float rangeInches)
        {
            float distance = GetDistanceInches(from, to);
            return distance <= rangeInches;
        }

        /// <summary>
        /// Get the radius in Unity units for a base size
        /// </summary>
        /// <param name="baseSize">Base size enum</param>
        /// <returns>Radius in Unity units</returns>
        public static float GetBaseRadius(BaseSize baseSize)
        {
            float diameterMM = baseSize switch
            {
                BaseSize.Small_25mm => 25f,
                BaseSize.Medium_40mm => 40f,
                BaseSize.Large_60mm => 60f,
                BaseSize.Huge_80mm => 80f,
                BaseSize.Gargantuan_100mm => 100f,
                _ => 25f
            };

            // Convert mm to inches (25.4mm per inch), then to Unity units
            float diameterInches = diameterMM / 25.4f;
            float radiusInches = diameterInches / 2f;
            return InchesToUnityUnits(radiusInches);
        }

        /// <summary>
        /// Format distance as a readable string
        /// </summary>
        /// <param name="inches">Distance in inches</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <returns>Formatted string</returns>
        public static string FormatDistance(float inches, int decimals = 1)
        {
            return $"{inches.ToString($"F{decimals}")}\"";
        }

        /// <summary>
        /// Clamp a position to be within battlefield bounds
        /// </summary>
        /// <param name="position">Position to clamp</param>
        /// <param name="minBounds">Minimum battlefield bounds</param>
        /// <param name="maxBounds">Maximum battlefield bounds</param>
        /// <returns>Clamped position</returns>
        public static Vector3 ClampToBounds(Vector3 position, Vector3 minBounds, Vector3 maxBounds)
        {
            return new Vector3(
                Mathf.Clamp(position.x, minBounds.x, maxBounds.x),
                position.y,
                Mathf.Clamp(position.z, minBounds.z, maxBounds.z)
            );
        }

        /// <summary>
        /// Get the closest point on a circle to a target position
        /// </summary>
        /// <param name="center">Center of circle</param>
        /// <param name="radius">Radius of circle</param>
        /// <param name="targetPosition">Target position</param>
        /// <returns>Closest point on circle perimeter</returns>
        public static Vector3 GetClosestPointOnCircle(Vector3 center, float radius, Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - center).Flatten().normalized;
            return center + direction * radius;
        }

        /// <summary>
        /// Calculate angle between two positions (for rotation)
        /// </summary>
        /// <param name="from">Starting position</param>
        /// <param name="to">Target position</param>
        /// <returns>Angle in degrees</returns>
        public static float GetAngleBetween(Vector3 from, Vector3 to)
        {
            Vector3 direction = (to - from).Flatten();
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
    }
}