using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static BulletPool Instance;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (poolDictionary.ContainsKey(tag))
        {
            if (poolDictionary[tag].Count == 0 || poolDictionary[tag].Peek().activeSelf)
            {
                Pool pool = pools.Find(p => p.tag == tag);
                if (pool != null)
                {
                    pool.size++;
                    GameObject newObject = Instantiate(pool.prefab);
                    newObject.SetActive(false);
                    poolDictionary[tag].Enqueue(newObject);
                    
                    newObject.transform.SetParent(this.gameObject.transform);
                }
                else
                {
                    Debug.LogError("No se encontró un pool con el tag " + tag + " en la lista de pools.");
                    return null;
                }
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }
        else
        {
            Debug.LogWarning("Pool con el tag " + tag + " no existe.");
            return null;
        }
    }
}
