using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;  // El nombre del tipo de proyectil
        public GameObject prefab;  // El prefab del proyectil
        public int size;  // Tamaño del pool
    }

    public static BulletPool Instance;

    // Lista de tipos de proyectiles (puedes agregar más en el inspector)
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Inicializar el pool de cada tipo de proyectil
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Crear los objetos en el pool
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // Método para obtener un proyectil del pool
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool con el tag " + tag + " no existe.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Volver a encolar el objeto
        poolDictionary[tag].Enqueue(objectToSpawn);

        return objectToSpawn;
    }
}