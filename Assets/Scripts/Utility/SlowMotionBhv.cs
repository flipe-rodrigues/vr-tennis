using UnityEngine;
using UnityEngine.UIElements;

public class SlowMotionBhv : CachedTransformBhv
{
    // Public fields
    [Range(.01f, 1)]
    public float maxTimeScale = .1f;
    [Range(.01f, 1)]
    public float minTimeScale = .005f;
    public float distanceModifier = .5f;

    // Read only fields
    [SerializeField, ReadOnly]
    private float _distance;
    [SerializeField, ReadOnly]
    private float _lerp;

    // Private fields
    private Renderer _renderer;
    private float _timeScale;
    bool wasInView;

    protected override void Awake()
    {
        base.Awake();

        _renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        _timeScale = 1f;
    }

    private void Update()
    {
        if (this.IsInView())
        {
            _distance = (TennisManager.Instance.Ball.Position - TennisManager.Instance.Racket.Position).magnitude - TennisManager.Instance.Ball.radius;

            _lerp = _distance * distanceModifier;

            _timeScale = Mathf.Lerp(minTimeScale, maxTimeScale, _lerp);

            wasInView = true;
        }
        else if (wasInView)
        {
            _timeScale = 1f;

            wasInView = false;
        }

        Time.timeScale = _timeScale;
    }

    public bool IsInView()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        return GeometryUtility.TestPlanesAABB(planes, _renderer.bounds);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
