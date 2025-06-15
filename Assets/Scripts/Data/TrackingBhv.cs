using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

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
    private string _binaryFileName;
    [SerializeField, ReadOnly]
    private string _csvFileName;
    [SerializeField, ReadOnly]
    private bool _isSaving;

    // Private fields
    private List<TrackingDatum> _trackingData;
    private BinaryWriter _binaryWriter;
    private StreamWriter _csvWriter;
    private string _binaryFilePath;
    private string _csvFilePath;

    private void OnValidate()
    {
        _binaryFileName = DataManager.GetFileName(this.name, ".bin");
        _binaryFilePath = Path.Combine(DataManager.savePath, _binaryFileName);

        _csvFileName = DataManager.GetFileName(this.name);
        _csvFilePath = Path.Combine(DataManager.savePath, _csvFileName);
    }

    private void Start()
    {
        if (DataManager.Instance.saveData)
        {
            _binaryWriter = new BinaryWriter(File.Open(_binaryFilePath, FileMode.Create));
        }
    }

    public void InitializeDataList(int expectedDataSize)
    {
        _trackingData = new List<TrackingDatum>(expectedDataSize);
    }

    private void FixedUpdate()
    {
        this.Record();
    }

    public void Record(TaskEventType taskEvent = TaskEventType.None)
    {
        if (_isSaving)
        {
            return;
        }

        TrackingDatum datum = new TrackingDatum
        {
            stage = TaskManager.Instance.StageIndex,
            trial = TaskManager.Instance.TrialIndex,
            time = Time.time,
            position = this.Position,
            rotation = this.Rotation,
            taskEvent = taskEvent
        };

        _binaryWriter.Write(datum.stage);
        _binaryWriter.Write(datum.trial);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.time }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.x }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.y }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.z }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.x }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.y }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.z }).integerValue);
        _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.w }).integerValue);
        _binaryWriter.Write((int)datum.taskEvent);

        //_trackingData.Add(datum);
    }

    public void SaveAndClear()
    {
        //this.SaveToBinaryAndClear();
    }

    private void SaveToCsvAndClear()
    {
        if (_csvWriter == null)
        {
            return;
        }
        this.StartCoroutine(SaveToCsvAndClearCoroutine());
    }

    private void SaveToBinaryAndClear()
    {
        if (_binaryWriter == null)
        {
            return;
        }

        this.StartCoroutine(SaveToBinaryAndClearCoroutine());
    }

    private IEnumerator SaveToCsvAndClearCoroutine()
    {
        _isSaving = true;

        foreach (TrackingDatum datum in _trackingData)
        {
            _csvWriter.WriteLine(datum.Serialize());

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        this.Clear();

        _isSaving = false;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct ConverterStruct
    {
        [FieldOffset(0)] public int integerValue;
        [FieldOffset(0)] public float floatValue;
    }

    private IEnumerator SaveToBinaryAndClearCoroutine()
    {
        _isSaving = true;

        foreach (TrackingDatum datum in _trackingData)
        {
            _binaryWriter.Write(datum.stage);
            _binaryWriter.Write(datum.trial);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.time }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.x }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.y }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.position.z }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.x }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.y }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.z }).integerValue);
            _binaryWriter.Write((new ConverterStruct { floatValue = datum.rotation.w }).integerValue);
            _binaryWriter.Write((int)datum.taskEvent);

            yield return ApplicationManager.waitForFixedUpdateInstance;
        }

        this.Clear();

        _isSaving = false;
    }

    public void Clear()
    {
        _trackingData.Clear();
    }

    private void OnDestroy()
    {
        _binaryWriter?.Dispose();
        _csvWriter?.Dispose();
    }

    private void OnApplicationQuit()
    {
        _binaryWriter?.Flush();
        _binaryWriter?.Close();

        this.ConvertBinaryToCSV();

        if (File.Exists(_csvFilePath))
        {
            File.Delete(_binaryFilePath);
        }
    }

    public void ConvertBinaryToCSV()
    {
        _csvWriter = File.CreateText(_csvFilePath);
        _csvWriter.WriteLine(TrackingDatum.header);

        using BinaryReader binaryReader = new BinaryReader(File.OpenRead(_binaryFilePath));

        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            int stage = binaryReader.ReadInt32();
            int trial = binaryReader.ReadInt32();
            float time = binaryReader.ReadSingle();
            float px = binaryReader.ReadSingle();
            float py = binaryReader.ReadSingle();
            float pz = binaryReader.ReadSingle();
            float rx = binaryReader.ReadSingle();
            float ry = binaryReader.ReadSingle();
            float rz = binaryReader.ReadSingle();
            float rw = binaryReader.ReadSingle();
            int eventCode = binaryReader.ReadInt32();

            _csvWriter.WriteLine($"{stage},{trial},{time},{px},{py},{pz},{rx},{ry},{rz},{rw},{((TaskEventType)eventCode).GetName()}");
        }

        _csvWriter.Flush();
        _csvWriter.Close();

        binaryReader.Close();
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
