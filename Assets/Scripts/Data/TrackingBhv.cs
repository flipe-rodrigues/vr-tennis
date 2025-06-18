using UnityEngine;
using System.IO;

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
    private string _binaryPath;
    private float _samplingTimer;

    private void OnValidate()
    {
        _binaryFilename = DataManager.GetFilename(this.name, ".bin");
        _binaryPath = Path.Combine(DataManager.savePath, _binaryFilename);
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
        }
    }

    private void FixedUpdate()
    {
        _samplingTimer += Time.fixedDeltaTime;

        if (_samplingTimer >= TrackingManager.Instance.SamplingInterval)
        {
            this.Record();

            _samplingTimer = 0f;
        }
    }

    public void Record(TaskEventType taskEvent = TaskEventType.None)
    {
        if (!DataManager.Instance.saveData || ApplicationManager.Instance.HasStartedQuitting)
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

        _binaryWriter.WriteTrackingDatum(datum);
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
        string csvFilename = DataManager.GetFilename(this.name);
        string csvPath = Path.Combine(DataManager.savePath, csvFilename);

        StreamWriter csvWriter = File.CreateText(csvPath);
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
