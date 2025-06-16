using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public static class Extensions
{
    //public static IEnumerable<Vector3> Pop(this IEnumerable<Vector3> vectors)
    //{
        
    //}

    public static Vector3 Mean(this IEnumerable<Vector3> vectors)
    {
        Vector3 mean = Vector3.zero;

        foreach (Vector3 vector in vectors)
        {
            mean += vector / vectors.Count();
        }

        return mean;
    }

    public static Vector3 MidPoint(this IEnumerable<Vector3> vectors)
    {
        Vector3 median = new Vector3();

        median.x = vectors.Select(v => v.x).Distinct().ToArray().Average();
        median.y = vectors.Select(v => v.y).Distinct().ToArray().Average();
        median.z = vectors.Select(v => v.z).Distinct().ToArray().Average();

        return median;
    }

    public static float Median(this IEnumerable<float> floats)
    {
        int count = floats.Count();

        floats = floats.OrderBy(f => f);

        int midPoint = count / 2;

        if (count % 2 == 0)
            return (floats.ElementAt(midPoint - 1) + floats.ElementAt(midPoint)) / 2f;
        else
            return floats.ElementAt(midPoint);
    }

    public static Vector3 ElementWiseMultiplication(this Vector3 v, Vector3 u)
    {
        return new Vector3(v.x * u.x, v.y * u.y, v.z * u.z);
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector3 ClampBetween(this Vector3 v, Vector3 min, Vector3 max)
    {
        return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
    }

    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;

        return color;
    }

    public static float TauToLambda(this float tau)
    {
        return 1f - Mathf.Exp(-Time.fixedDeltaTime / tau);
    }

    public static void WriteTrackingDatum(this BinaryWriter writer, TrackingDatum datum)
    {
        writer.Write(datum.stage);
        writer.Write(datum.trial);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.time }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.position.x }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.position.y }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.position.z }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.rotation.x }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.rotation.y }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.rotation.z }).intValue);
        writer.Write((new BitwiseIntFloatConverter { floatValue = datum.rotation.w }).intValue);
        writer.Write((int)datum.taskEvent);
    }

    public static void ReadTrackingDatum(this BinaryReader reader, out TrackingDatum datum)
    {
        datum = new TrackingDatum
        {
            stage = reader.ReadInt32(),
            trial = reader.ReadInt32(),
            time = reader.ReadSingle(),
            position = new Vector3(
                reader.ReadSingle(), 
                reader.ReadSingle(), 
                reader.ReadSingle()),
            rotation = new Quaternion(
                reader.ReadSingle(), 
                reader.ReadSingle(), 
                reader.ReadSingle(), 
                reader.ReadSingle()),
            taskEvent = (TaskEventType)reader.ReadInt32()
        };
    }
}


[StructLayout(LayoutKind.Explicit)]
public struct BitwiseIntFloatConverter
{
    [FieldOffset(0)] public int intValue;
    [FieldOffset(0)] public float floatValue;
}
