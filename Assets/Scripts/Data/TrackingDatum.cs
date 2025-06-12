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
    public string eventName;

    public string Serialize()
    {
        return $"{stage},{trial},{time},{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w},{eventName}";
    }
}

//[System.Serializable]
//public class TrackingDatum
//{
//    // Static fields
//    public static string header =
//        "stage,trial,time,position.x,position.y,position.z,rotation.x,rotation.y,rotation.z,rotation.w,event";

//    // Public fields
//    public int stage;
//    public int trial;
//    public float time;
//    public Vector3 position;
//    public Quaternion rotation;
//    public string eventName;

//    public TrackingDatum(int stage, int trial, float time, Vector3 position, Quaternion rotation, string eventName)
//    {
//        this.stage = stage;
//        this.trial = trial;
//        this.time = time;
//        this.position = position;
//        this.rotation = rotation;
//        this.eventName = eventName;
//    }

//    public string Serialize()
//    {
//        return $"{stage},{trial},{time},{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w},{eventName}";
//    }
//}
