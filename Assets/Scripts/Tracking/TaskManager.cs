using UnityEngine;

public class TaskManager : Singleton<TaskManager>
{
    // Public properties

    public int TrialCounter { get; private set; }
    public int ValidTrialCounter { get; private set; }
    public bool IsTrialActive { get; private set; }

    // References
    public BallSpawningBhv ballSpawner;
    public MeshRenderer courtMeshRenderer;
    public GameObject net;
    public GameObject targetZone;

    private void OnEnable()
    {
        if (ballSpawner != null)
        {
            ballSpawner.onBallSpawned.AddListener(StartTrial);
        }
    }

    private void OnDestroy()
    {
        if (ballSpawner != null)
        {
            ballSpawner.onBallSpawned.RemoveListener(StartTrial);
        }
    }

    private void Start()
    {
        // Acquire the position of the Ball Spawner and the velocity of the ball and log it in the CSV
        TrackingManager.Instance.RecordEvent($"BallSpawnerPosition_{ballSpawner.Position}");
        TrackingManager.Instance.RecordEvent($"BallSpawnerRotation_{ballSpawner.Rotation}");
        TrackingManager.Instance.RecordEvent($"BallInitialVelocity_{ballSpawner.InitialVelocity}");
    }

    public void StartTrial()
    {
        if (IsTrialActive)
        {
            EndTrial(); // End the previous trial if still active
        }

        TrialCounter++;
        IsTrialActive = true;

        // Add a marker in the CSV
        TrackingManager.Instance.RecordEvent($"TrialStart_{TrialCounter}");

        // Activate the net and court mesh after trial 50
        if (TrialCounter > 50)
        {
            if (net != null) net.SetActive(true);
            if (courtMeshRenderer != null) courtMeshRenderer.enabled = true;
        }

        if (TrialCounter > 100)
        {
            if (targetZone != null) targetZone.SetActive(true);
        }
    }

    private void EndTrial()
    {
        if (!IsTrialActive) return;

        // Marca o fim do trial no CSV
        TrackingManager.Instance.RecordEvent("TrialEnd");

        IsTrialActive = false;
    }
}