using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : MonoBehaviour
{
    private Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            objects.Add(child.gameObject.name, child.gameObject);
            child.gameObject.SetActive(false);
            Debug.Log("Objeto encontrado: " + child.gameObject.name);
        }
    }

    public void ActivateObject(string objectName)
    {
        foreach (KeyValuePair<string, GameObject> entry in objects)
        {
            entry.Value.SetActive(entry.Key == objectName);
        }
    }
}
