using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Overlays;
using Unity.XR.CoreUtils.Datums;

public class TrackingBhv : CachedTransformBhv
{
    // Public fields
    public Color gizmoColor = Color.red;
    [Min(.001f)]
    public float gizmoRadius = 0.025f;
    [Range(0, 1)]
    public float gizmoSelectedAlpha = 0.75f;
    [Range(0, 1)]
    public float gizmoIdleAlpha = 0.25f;

    // Read only fields
    [SerializeField, ReadOnly]
    private string _fileName;
    private string _filePath;

    // Private fields
    private List<TrackingDatum> _trackingData = new List<TrackingDatum>();
    private StreamWriter _fileWriter;

    private void OnValidate()
    {
        _fileName = this.GetFileName();
    }

    private string GetFileName()
    {
        return string.Concat(
            UIManager.subjectName, "_",
            UIManager.subjectAge, "_",
            UIManager.subjectSex, "_",
            UIManager.subjectTennisExp, "_",
            UIManager.subjectVRExp, "_",
            this.name.ToLower().Replace(" ", "-"), "_",
            TrackingManager.GetFormattedTimestamp(),
            ".csv");
    }

    private void Start()
    {
        _filePath = Path.Combine(TrackingManager.Instance.DataPath, _fileName);

        if (TrackingManager.Instance.saveData)
        {
            _fileWriter = File.CreateText(_filePath);

            _fileWriter.WriteLine(TrackingDatum.header);
        }
    }

    private void FixedUpdate()
    {
        this.Record();
    }

    private void Record()
    {
        this.RecordEvent("");
    }

    public void RecordEvent(string eventName)
    {
        TrackingDatum datum = new TrackingDatum(
            stage: TaskManager.Instance.StageIndex,
            trial: TaskManager.Instance.TrialIndex,
            time: Time.time,
            position: this.Position,
            rotation: this.Rotation,
            eventName: eventName
        );

        _trackingData.Add(datum);
    }

    public void SaveAndClear()
    {
        if (_fileWriter == null)
        {
            return;
        }

        foreach (TrackingDatum datum in _trackingData)
        {
            _fileWriter.WriteLine(datum.Serialize());
        }

        _trackingData.Clear();
    }

    private void OnDisable()
    {
        if (_fileWriter == null)
        {
            return;
        }

        _fileWriter.Flush();
        _fileWriter.Close();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor.SetAlpha(gizmoIdleAlpha);
        Gizmos.DrawSphere(this.Position, gizmoRadius);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor.SetAlpha(gizmoSelectedAlpha);
        Gizmos.DrawSphere(this.Position, gizmoRadius);
    }
}
