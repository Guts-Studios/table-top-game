using UnityEngine;
using System.Collections.Generic;

namespace Warslammer.Utilities
{
    /// <summary>
    /// Useful extension methods for Unity and C# types
    /// </summary>
    public static class Extensions
    {
        #region Vector3 Extensions
        /// <summary>
        /// Get the horizontal distance (ignoring Y axis) between two positions
        /// </summary>
        public static float HorizontalDistance(this Vector3 from, Vector3 to)
        {
            Vector3 fromFlat = new Vector3(from.x, 0, from.z);
            Vector3 toFlat = new Vector3(to.x, 0, to.z);
            return Vector3.Distance(fromFlat, toFlat);
        }

        /// <summary>
        /// Set the X component of a Vector3
        /// </summary>
        public static Vector3 WithX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Set the Y component of a Vector3
        /// </summary>
        public static Vector3 WithY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Set the Z component of a Vector3
        /// </summary>
        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Flatten a Vector3 by setting Y to 0
        /// </summary>
        public static Vector3 Flatten(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
        #endregion

        #region Transform Extensions
        /// <summary>
        /// Reset a transform's local position, rotation, and scale
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Set the layer of a GameObject and all its children
        /// </summary>
        public static void SetLayerRecursive(this Transform transform, int layer)
        {
            transform.gameObject.layer = layer;
            foreach (Transform child in transform)
            {
                child.SetLayerRecursive(layer);
            }
        }

        /// <summary>
        /// Destroy all children of a transform
        /// </summary>
        public static void DestroyChildren(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Get all children of a transform
        /// </summary>
        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in transform)
            {
                children.Add(child);
            }
            return children;
        }
        #endregion

        #region GameObject Extensions
        /// <summary>
        /// Get or add a component to a GameObject
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Check if a GameObject has a component
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }
        #endregion

        #region Color Extensions
        /// <summary>
        /// Set the alpha value of a color
        /// </summary>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Multiply RGB values while preserving alpha
        /// </summary>
        public static Color MultiplyRGB(this Color color, float multiplier)
        {
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
        }
        #endregion

        #region List Extensions
        /// <summary>
        /// Get a random element from a list
        /// </summary>
        public static T GetRandom<T>(this List<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);
            
            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Shuffle a list randomly
        /// </summary>
        public static void Shuffle<T>(this List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Check if a list is null or empty
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }
        #endregion

        #region Float Extensions
        /// <summary>
        /// Remap a value from one range to another
        /// </summary>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
        }

        /// <summary>
        /// Check if a float is approximately equal to another (within tolerance)
        /// </summary>
        public static bool Approximately(this float value, float other, float tolerance = 0.01f)
        {
            return Mathf.Abs(value - other) <= tolerance;
        }
        #endregion

        #region String Extensions
        /// <summary>
        /// Check if a string is null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Truncate a string to a maximum length
        /// </summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            
            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }
        #endregion

        #region LayerMask Extensions
        /// <summary>
        /// Check if a LayerMask contains a specific layer
        /// </summary>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) != 0;
        }
        #endregion
    }
}