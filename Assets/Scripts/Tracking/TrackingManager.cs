using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public string DataPath => _dataPath;

    // Public fields
    public bool saveData;

    // Read only fields
    [SerializeField, ReadOnly]
    private string _dataPath;
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trackers;

    // Private fields
    private Dictionary<string, StreamWriter> _streamWriters = new Dictionary<string, StreamWriter>();

    public void OnValidate()
    {
        _dataPath = Path.Combine(Application.persistentDataPath, "Data", "Tracking");

        if (!Directory.Exists(_dataPath))
        {
            Directory.CreateDirectory(_dataPath);
        }

        _trackers = FindObjectsByType<TrackingBhv>(FindObjectsSortMode.InstanceID)
            .Where(x => x.isActiveAndEnabled)
            .ToList();
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }

    private void Start()
    {
        this.HomogeneizeAcrossCulturalSettings();
    }

    private void HomogeneizeAcrossCulturalSettings()
    {
        CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        customCulture.NumberFormat.NumberGroupSeparator = ",";
        CultureInfo.DefaultThreadCurrentCulture = customCulture;
        CultureInfo.DefaultThreadCurrentUICulture = customCulture;
    }

    public void SaveAndClear()
    {
        if (!saveData)
        {
            return;
        }

        foreach (var tracker in _trackers)
        {
            tracker.SaveAndClear();
        }
    }

    public static string GetFormattedTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    public void RecordEvent(string eventName) 
    {
        if (!saveData)
        {
            return;
        }

        foreach (var tracker in _trackers)
        {
            tracker.RecordEvent(eventName);
        }
    }
}

