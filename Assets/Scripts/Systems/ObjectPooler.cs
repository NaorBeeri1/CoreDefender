using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    [Header("Pool Configurations")]
    [SerializeField] private List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[CoreDefender] Pool with tag '{tag}' doesn't exist.");
            return null;
        }

        Queue<GameObject> objectQueue = poolDictionary[tag];

        // Ensure we don't pull a destroyed object from the queue
        GameObject objectToSpawn = null;
        while (objectQueue.Count > 0)
        {
            GameObject candidate = objectQueue.Dequeue();
            if (candidate != null)
            {
                objectToSpawn = candidate;
                break;
            }
        }

        // If the pool ran out of valid objects, instantiate a new one dynamically
        if (objectToSpawn == null)
        {
            foreach (Pool pool in pools)
            {
                if (pool.tag == tag)
                {
                    objectToSpawn = Instantiate(pool.prefab);
                    break;
                }
            }
        }

        if (objectToSpawn != null)
        {
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Enqueue it back for future reuse
            objectQueue.Enqueue(objectToSpawn);
        }

        return objectToSpawn;
    }
}