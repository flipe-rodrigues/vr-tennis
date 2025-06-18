using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public float SamplingInterval => _samplingInterval;
    public bool IsDoneSaving => _trackers.All(tracker => tracker.IsDoneSaving);

    // Public fields
    public int samplingRate = 500;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _samplingInterval;
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trackers;

    protected override void OnValidate()
    {
        base.OnValidate();

        _trackers = FindObjectsByType<TrackingBhv>(FindObjectsSortMode.None).ToList();

        _samplingInterval = 1f / samplingRate;
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }

    public void RecordEvent(TaskEventType taskEvent) 
    {
        if (!DataManager.Instance.saveData)
        {
            return;
        }

        foreach (TrackingBhv tracker in _trackers)
        {
            tracker.Record(taskEvent);
        }
    }
}

