using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TrackingManager : Singleton<TrackingManager>
{
    // Public properties
    public bool IsDoneSaving => _trackers.All(tracker => tracker.IsDoneSaving);

    // Read only fields
    [SerializeField, ReadOnly]
    private List<TrackingBhv> _trackers;

    protected override void OnValidate()
    {
        base.OnValidate();

        _trackers = FindObjectsByType<TrackingBhv>(FindObjectsSortMode.None).ToList();
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

