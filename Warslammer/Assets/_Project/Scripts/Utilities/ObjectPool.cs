using System.Collections.Generic;
using UnityEngine;

namespace Warslammer.Utilities
{
    /// <summary>
    /// Generic object pooling system for performance optimization
    /// Reduces garbage collection by reusing objects
    /// </summary>
    /// <typeparam name="T">Type of object to pool (must be Component)</typeparam>
    public class ObjectPool<T> where T : Component
    {
        #region Properties
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _availableObjects;
        private readonly List<T> _allObjects;
        private readonly int _initialSize;
        private readonly int _maxSize;
        
        /// <summary>
        /// Number of objects currently available in the pool
        /// </summary>
        public int AvailableCount => _availableObjects.Count;
        
        /// <summary>
        /// Total number of objects in the pool (active and inactive)
        /// </summary>
        public int TotalCount => _allObjects.Count;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new object pool
        /// </summary>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        /// <param name="initialSize">Initial number of objects to create</param>
        /// <param name="maxSize">Maximum pool size (0 for unlimited)</param>
        public ObjectPool(T prefab, Transform parent = null, int initialSize = 10, int maxSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            _initialSize = initialSize;
            _maxSize = maxSize;
            
            _availableObjects = new Queue<T>();
            _allObjects = new List<T>();
            
            // Pre-populate the pool
            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }
        #endregion

        #region Pool Operations
        /// <summary>
        /// Get an object from the pool
        /// </summary>
        /// <returns>Pooled object instance</returns>
        public T Get()
        {
            T obj;
            
            if (_availableObjects.Count > 0)
            {
                // Reuse existing object
                obj = _availableObjects.Dequeue();
            }
            else
            {
                // Create new object if pool is empty
                if (_maxSize > 0 && _allObjects.Count >= _maxSize)
                {
                    Debug.LogWarning($"[ObjectPool] Pool reached max size ({_maxSize}). Reusing oldest object.");
                    obj = _allObjects[0];
                }
                else
                {
                    obj = CreateNewObject();
                }
            }
            
            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Get an object at a specific position and rotation
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>Pooled object instance</returns>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }

        /// <summary>
        /// Return an object to the pool
        /// </summary>
        /// <param name="obj">Object to return</param>
        public void Return(T obj)
        {
            if (obj == null)
                return;
            
            obj.gameObject.SetActive(false);
            
            if (_parent != null)
                obj.transform.SetParent(_parent);
            
            if (!_availableObjects.Contains(obj))
            {
                _availableObjects.Enqueue(obj);
            }
        }

        /// <summary>
        /// Return an object after a delay
        /// </summary>
        /// <param name="obj">Object to return</param>
        /// <param name="delay">Delay in seconds</param>
        public void ReturnAfterDelay(T obj, float delay)
        {
            if (obj == null)
                return;
            
            obj.StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
        }

        private System.Collections.IEnumerator ReturnAfterDelayCoroutine(T obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            Return(obj);
        }
        #endregion

        #region Pool Management
        /// <summary>
        /// Create a new object and add it to the pool
        /// </summary>
        private T CreateNewObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            
            _allObjects.Add(obj);
            _availableObjects.Enqueue(obj);
            
            return obj;
        }

        /// <summary>
        /// Expand the pool by a specific amount
        /// </summary>
        /// <param name="count">Number of objects to add</param>
        public void Expand(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_maxSize > 0 && _allObjects.Count >= _maxSize)
                {
                    Debug.LogWarning($"[ObjectPool] Cannot expand pool beyond max size ({_maxSize})");
                    break;
                }
                
                CreateNewObject();
            }
        }

        /// <summary>
        /// Clear the pool and destroy all objects
        /// </summary>
        public void Clear()
        {
            foreach (T obj in _allObjects)
            {
                if (obj != null)
                    Object.Destroy(obj.gameObject);
            }
            
            _allObjects.Clear();
            _availableObjects.Clear();
        }

        /// <summary>
        /// Return all active objects to the pool
        /// </summary>
        public void ReturnAll()
        {
            foreach (T obj in _allObjects)
            {
                if (obj != null && obj.gameObject.activeSelf)
                {
                    Return(obj);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Simple object pool manager for managing multiple pools
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;
        
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PoolManager");
                    _instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private Dictionary<string, object> _pools = new Dictionary<string, object>();

        /// <summary>
        /// Create or get a pool for a specific prefab
        /// </summary>
        public ObjectPool<T> GetPool<T>(T prefab, int initialSize = 10, int maxSize = 0) where T : Component
        {
            string key = prefab.GetType().Name + "_" + prefab.name;
            
            if (!_pools.ContainsKey(key))
            {
                Transform poolParent = new GameObject(key + "_Pool").transform;
                poolParent.SetParent(transform);
                
                ObjectPool<T> pool = new ObjectPool<T>(prefab, poolParent, initialSize, maxSize);
                _pools[key] = pool;
            }
            
            return _pools[key] as ObjectPool<T>;
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            _pools.Clear();
            
            // Destroy all child pool containers
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}