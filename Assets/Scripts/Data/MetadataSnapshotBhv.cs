using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class MetadataSnapshotBhv : MonoBehaviour
{
    [Header("Component References")]
    public BallRigidbodyBhv ball;
    public RacketRigidbodyBhv racket;
    public BallSpawningBhv ballSpawner;
    public TaskManager taskManager;

    [Header("Physics Materials")]
    public List<PhysicsMaterial> physXMaterials = new List<PhysicsMaterial>();

    // Read only fields
    [SerializeField, ReadOnly]
    private string _fileName;

    // Private fields
    private string _filePath;

    private void OnValidate()
    {
        _fileName = DataManager.GetFileName("metadata", "json");
    }

    private void Start()
    {
        _filePath = Path.Combine(DataManager.dataPath, _fileName);
        
        if (DataManager.Instance.saveData)
        {
            this.MetadataToJSON();
        }
    }

    private void MetadataToJSON()
    {
        List<MonoBehaviour> components = new List<MonoBehaviour>
        {
            ball,
            racket,
            ballSpawner,
            taskManager
        };

        string json;
        string delim = $",{Environment.NewLine}";

        File.AppendAllText(_filePath,$"{{{Environment.NewLine}");

        foreach (var component in components)
        {
            json = JsonUtility.ToJson(component, true);

            File.AppendAllText(_filePath, $"\"{component.name.Replace(" ","")}\":");
            File.AppendAllText(_filePath, $"{json}{delim}");
        }

        for (int i = 0; i < physXMaterials.Count; i++)
        {
            if (i == physXMaterials.Count - 1)
            {
                delim = $"{Environment.NewLine}}}";
            }

            PhysXMaterialData physXMaterialData = new PhysXMaterialData
            {
                dynamicFriction = physXMaterials[i].dynamicFriction,
                staticFriction = physXMaterials[i].staticFriction,
                bounciness = physXMaterials[i].bounciness,
                frictionCombine = physXMaterials[i].frictionCombine.ToString(),
                bouncinessCombine = physXMaterials[i].bounceCombine.ToString()
            };

            json = JsonUtility.ToJson(physXMaterialData, true);

            File.AppendAllText(_filePath, $"\"{physXMaterials[i].name.Replace(" ", "")}PhysX\":");
            File.AppendAllText(_filePath, $"{json}{delim}");
        }
    }
}

[System.Serializable]
public class PhysXMaterialData
{
    public float dynamicFriction;
    public float staticFriction;
    public float bounciness;
    public string frictionCombine;
    public string bouncinessCombine;
}