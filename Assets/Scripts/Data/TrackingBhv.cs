using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class TrackingBhv : CachedTransformBhv
{
    // Public properties
    public bool IsSaving => _isSaving;

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
    [SerializeField, ReadOnly]
    private bool _isSaving;

    // Private fields
    private List<TrackingDatum> _trackingData;
    private StreamWriter _fileWriter;
    private string _filePath;

    private void OnValidate()
    {
        _fileName = this.GetFileName();
    }

    // MOVE ELSEWHERE!!!!!!!!!!!!!!!!!!!!!!!
    private string GetFileName()
    {
        return string.Concat(
            UIManager.subjectName, "_",
            UIManager.subjectAge, "_",
            UIManager.subjectSex, "_",
            UIManager.subjectTennisExp, "_",
            UIManager.subjectVRExp, "_",
            this.name.ToLower().Replace(" ", "-"), "_",
            SaveSystem.GetFormattedTimestamp(),
            ".csv");
    }

    private void Start()
    {
        _filePath = Path.Combine(SaveSystem.dataPath, _fileName);

        if (SaveSystem.Instance.saveData)
        {
            _fileWriter = File.CreateText(_filePath);

            _fileWriter.WriteLine(TrackingDatum.header);
        }

        _trackingData = new List<TrackingDatum>(TrackingManager.Instance.ExpectedDataSize);
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
        if (_isSaving)
        {
            return;
        }

        TrackingDatum datum = new TrackingDatum {
            stage = TaskManager.Instance.StageIndex,
            trial = TaskManager.Instance.TrialIndex,
            time = Time.time,
            position = this.Position,
            rotation = this.Rotation,
            eventName = eventName
        };

        _trackingData.Add(datum);
    }

    public void SaveAndClear()
    {
        if (_fileWriter == null)
        {
            return;
        }

        this.StartCoroutine(SaveAndClearCoroutine());
    }

    private IEnumerator SaveAndClearCoroutine()
    {
        _isSaving = true;

        foreach (TrackingDatum datum in _trackingData)
        {
            _fileWriter.WriteLine(datum.Serialize());

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        _isSaving = false;

        this.Clear();
    }

    public void Clear()
    {
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
