using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class MetadataSnapshotBhv : MonoBehaviour
{
    // Read only fields
    [SerializeField, ReadOnly]
    private string _fileName;

    // Public fields
    [Header("Components")]
    public TaskManager taskManager;
    public BallLaunchBhv ballSpawner;
    public BallRigidbodyBhv ball;
    public RacketRigidbodyBhv racket;

    [Header("Physics Materials")]
    public PhysicsMaterial ballPhysXMaterial;
    public PhysicsMaterial courtPhysXMaterial;
    public PhysicsMaterial netPhysXMaterial;

    // Private fields
    private string _filePath;

    private void OnValidate()
    {
        _fileName = DataManager.GetFileName("metadata", "json");

        taskManager = TaskManager.Instance;
        ballSpawner = FindFirstObjectByType<BallLaunchBhv>();
        racket = TennisManager.Instance.Racket;
    }

    private void Start()
    {
        _filePath = Path.Combine(DataManager.savePath, _fileName);
        
        if (DataManager.Instance.saveMetadata)
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

            File.AppendAllText(_filePath, $"\"{component.name.Replace(" ","")}\": ");
            File.AppendAllText(_filePath, $"{json}{delim}");
        }

        List<PhysicsMaterial> physXMaterials = new List<PhysicsMaterial>
        {
            ballPhysXMaterial,
            courtPhysXMaterial,
            netPhysXMaterial
        };

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

            File.AppendAllText(_filePath, $"\"{physXMaterials[i].name.Replace(" ", "")}PhysX\": ");
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