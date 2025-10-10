using UnityEngine;
using System.IO;
using System.Collections;

public class TrackingBhv : CachedTransformBhv
{
    // Public properties
    public bool IsDoneSaving => _isDoneSaving;

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
    private string _binaryFilename;
    [SerializeField, ReadOnly]
    private bool _isDoneSaving;

    // Private fields
    private BinaryWriter _binaryWriter;
    private TaskEventType _taskEvent;
    private string _binaryPath;
    private string _csvPath;
    private WaitForSeconds _waitForTrackingInterval;

    private void OnValidate()
    {
        _binaryFilename = DataManager.GetFilename(this.name, ".bin");
        _binaryPath = Path.Combine(DataManager.Instance.SavePath, _binaryFilename);
        _csvPath = Path.ChangeExtension(_binaryPath, ".csv");
    }

    protected override void Awake()
    {
        base.Awake();

        this.OnValidate();
    }

    private void Start()
    {
        if (DataManager.Instance.saveData)
        {
            _binaryWriter = new BinaryWriter(File.Open(_binaryPath, FileMode.Create));

            _waitForTrackingInterval = new WaitForSeconds(TrackingManager.Instance.SamplingInterval);
            StartCoroutine(this.TrackingUpdateCoroutine());
        }
    }

    private IEnumerator TrackingUpdateCoroutine()
    {
        while (DataManager.Instance.saveData && !ApplicationManager.Instance.HasStartedQuitting)
        {
            this.Record();

            yield return _waitForTrackingInterval;
        }
    }

    private void Record()
    {
        TrackingDatum datum = new TrackingDatum
        {
            stage = TaskManager.Instance.StageIndex,
            trial = TaskManager.Instance.TrialIndex,
            time = Time.time,
            position = this.Position,
            rotation = this.Rotation,
            taskEvent = _taskEvent
        };

        _taskEvent = TaskEventType.None;

        _binaryWriter.WriteTrackingDatum(datum);
    }

    public void BaitNextTaskEvent(TaskEventType taskEvent)
    {
        _taskEvent = taskEvent;
    }

    protected virtual void OnEnable()
    {
        ApplicationManager.onQuitStart += this.HandleQuitRequest;
    }

    protected virtual void OnDisable()
    {
        ApplicationManager.onQuitStart -= this.HandleQuitRequest;
    }

    private void HandleQuitRequest()
    {
        this.OnApplicationQuit();
    }

    private void OnDestroy()
    {
        _binaryWriter?.Dispose();
    }

    private void OnApplicationQuit()
    {
        if (!DataManager.Instance.saveData || _isDoneSaving)
        {
            return;
        }

        _binaryWriter?.Flush();
        _binaryWriter?.Close();

        this.ConvertBinaryToCSV();
    }

    public void ConvertBinaryToCSV()
    {
        StreamWriter csvWriter = File.CreateText(_csvPath);
        csvWriter.WriteLine(TrackingDatum.header);

        using BinaryReader binaryReader = new BinaryReader(File.OpenRead(_binaryPath));

        while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
        {
            binaryReader.ReadTrackingDatum(out TrackingDatum datum);

            csvWriter.WriteLine(datum.Serialize());
        }

        csvWriter.Flush();
        csvWriter.Close();

        binaryReader.Close();

        File.Delete(_binaryPath);

        _isDoneSaving = true;
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
