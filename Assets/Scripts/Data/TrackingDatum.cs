using UnityEngine;

[System.Serializable]
public struct TrackingDatum
{
    public static string header =
        "stage,trial,time,position.x,position.y,position.z,rotation.x,rotation.y,rotation.z,rotation.w,event";

    public int stage;
    public int trial;
    public float time;
    public Vector3 position;
    public Quaternion rotation;
    public TaskEventType taskEvent;

    public string Serialize()
    {
        return $"{stage},{trial},{time},{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w},{taskEvent.GetName()}";
    }
}