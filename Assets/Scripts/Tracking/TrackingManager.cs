using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public fields
    public bool saveData;

    // Read only fields
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trajectoryTrackers;

    // Private fields
    private Dictionary<string, StreamWriter> _streamWriters = new Dictionary<string, StreamWriter>();

    //public void OnValidate()
    //{
    //    _trajectoryTrackers = FindObjectsByType<TrajectoryTrackingBhv>(FindObjectsSortMode.InstanceID)
    //        .Where(x => x.isActiveAndEnabled)
    //        .ToList();
    //}

    private void OnValidate()
    {
        _trajectoryTrackers = this.GetComponentsInChildren<TrackingBhv>().ToList();
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }

    private void Start()
    {
        this.HomogeneizeAcrossCulturalSettings();

        if (!saveData)
        {
            return;
        }

        string trajectory_file = string.Concat(
            UIManager.subjectCode, "_",
            UIManager.subjectAge, "_",
            UIManager.subjectSex, "_",
            UIManager.subjectTennisExp, "_",
            UIManager.subjectVRExp, "_trajectory_");

        this.CreateIfInexistent(Application.dataPath + "/Data" + "/Trajectories");

        foreach (var tracker in _trajectoryTrackers)
        {
            string trackerName = tracker.name.Replace(" ", "").Replace("Tracker", "");

            if (!_streamWriters.ContainsKey(trackerName))
            {
                string trackerFile = string.Concat(trackerName, "_", trajectory_file, GetFormattedTimestamp());
                StreamWriter writer = System.IO.File.CreateText(Application.dataPath + "//Data" + "//Trajectories//" + trackerFile + ".csv");
                _streamWriters[trackerName] = writer;
            }
        }
        this.WriteHeaders();

    }
    private void HomogeneizeAcrossCulturalSettings()
    {
        CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        customCulture.NumberFormat.NumberGroupSeparator = ",";
        CultureInfo.DefaultThreadCurrentCulture = customCulture;
        CultureInfo.DefaultThreadCurrentUICulture = customCulture;
    }

    private void CreateIfInexistent(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private void WriteHeaders()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Time,");
        sb.Append("PosX,");
        sb.Append("PosY,");
        sb.Append("PosZ,");
        sb.Append("RotX,");
        sb.Append("RotY,");
        sb.Append("RotZ,");
        sb.Append("RotW,");
        sb.Append("Event"); 

        string headerLine = sb.ToString();

        foreach (var writer in _streamWriters.Values)
        {
            writer.WriteLine(headerLine);
        }
    }

    void FixedUpdate()
    {
        if (!saveData)
        {
            return;
        }

        foreach (var tracker in _trajectoryTrackers)
        {
            string trackerName = tracker.name.Replace(" ", "").Replace("Tracker", "");

            Vector3 position = tracker.transform.position;
            Quaternion rotation = tracker.transform.rotation;
            
            StringBuilder sb = new StringBuilder();

            sb.Append(Time.timeSinceLevelLoad);
            sb.Append(",");
            sb.Append(position.x);
            sb.Append(",");
            sb.Append(position.y);
            sb.Append(",");
            sb.Append(position.z);
            sb.Append(",");
            sb.Append(rotation.x);
            sb.Append(",");
            sb.Append(rotation.y);
            sb.Append(",");
            sb.Append(rotation.z);
            sb.Append(",");
            sb.Append(rotation.w);

            _streamWriters[trackerName].WriteLine(sb.ToString());
        }
    }

    private string GetFormattedTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMdd_HHmmss");
    }

    public void RecordEvent(string eventName) 
    {
        if (!saveData) return;

        foreach (var writer in _streamWriters.Values)
        {
            // Formato: Time,a,a,a,a,a,a,a,EventName
            writer.WriteLine($"{Time.timeSinceLevelLoad},a,a,a,a,a,a,a,{eventName}");
        }
    }

    public void LogCollision(int layer1, int layer2)
    {
        if (!saveData) return;

        string layerName1 = LayerMask.LayerToName(layer1);
        string layerName2 = LayerMask.LayerToName(layer2);
        string eventText = $"Collision_{layerName1}_{layerName2}";

        // Log to all active trackers (or filter by involved objects)
        foreach (var writer in _streamWriters.Values)
        {
            writer.WriteLine($"{Time.timeSinceLevelLoad},a,a,a,a,a,a,a,{eventText}");
        }
    }

    void OnDisable()
    {
        if (!saveData)
        {
            return;
        }
        foreach (var writer in _streamWriters.Values)
        {
            writer.Flush();
            writer.Close();
        }
    }
}

