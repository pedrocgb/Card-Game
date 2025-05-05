using System.Collections.Generic;
using UnityEngine;

namespace Breezeblocks.Managers
{
    public class ObjectPooler : MonoBehaviour
    {
        #region Variables and Properties
        private static ObjectPooler Instance;

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int size;
        }

        public List<Pool> pools;
        private Dictionary<string, List<GameObject>> poolDictionary;
        private Dictionary<GameObject, Dictionary<System.Type, Component>> componentCache;
        #endregion

        // ========================================================================

        #region Static Methods
        public static GameObject SpawnFromPool(string Tag, Vector3 Position, Quaternion Rotation)
        {
            if (Instance == null)
                return null;

            return Instance.spawnFromPool(Tag, Position, Rotation);
        }

        public static T GetPooledComponent<T>(GameObject pooledObject) where T : Component
        {
            if (Instance == null)
                return null;

            return Instance.getPooledComponent<T>(pooledObject);
        }
        #endregion

        // ========================================================================

        #region Unity Methods
        private void Awake()
        {
            Instance = this;
            poolDictionary = new Dictionary<string, List<GameObject>>();
            componentCache = new Dictionary<GameObject, Dictionary<System.Type, Component>>();
        }

        private void Start()
        {
            foreach (Pool pool in pools)
            {
                List<GameObject> objectPool = new List<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    objectPool.Add(obj);

                    // Cache the components you need
                    CacheComponents(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }
        #endregion

        // ========================================================================

        #region Pool Methods
        private void CacheComponents(GameObject obj)
        {
            if (!componentCache.ContainsKey(obj))
            {
                componentCache[obj] = new Dictionary<System.Type, Component>();
            }

            // Add specific components you expect to cache
            Component[] components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                componentCache[obj][component.GetType()] = component;
            }
        }

        private T getPooledComponent<T>(GameObject obj) where T : Component
        {
            if (componentCache.ContainsKey(obj) && componentCache[obj].ContainsKey(typeof(T)))
            {
                return componentCache[obj][typeof(T)] as T;
            }
            return null;
        }

        public GameObject spawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                return null;
            }

            GameObject objectToSpawn = null;

            // Find an inactive object in the pool
            foreach (GameObject obj in poolDictionary[tag])
            {
                if (!obj.activeInHierarchy)
                {
                    objectToSpawn = obj;
                    break;
                }
            }

            // If no inactive object is found, instantiate a new one and add to the pool
            if (objectToSpawn == null)
            {
                Pool pool = pools.Find(p => p.tag == tag);
                if (pool != null)
                {
                    objectToSpawn = Instantiate(pool.prefab);
                    poolDictionary[tag].Add(objectToSpawn);

                    // Cache components for the new object
                    CacheComponents(objectToSpawn);
                }
                else
                {
                    Debug.LogWarning("No pool found with tag " + tag);
                    return null;
                }
            }

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            IPooledObjects[] poolable = objectToSpawn.GetComponents<IPooledObjects>();
            foreach (IPooledObjects p in poolable)
            {
                p?.OnSpawn();
            }

            return objectToSpawn;
        }
        #endregion

        // ========================================================================
    }
}
