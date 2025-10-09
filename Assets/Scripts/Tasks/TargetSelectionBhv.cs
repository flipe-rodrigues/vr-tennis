using System;
using UnityEngine;
using UnityEngine.Events;

public class TargetSelectionBhv : CachedTransformBhv
{
    // Public fields
    [Range(0, 25f)]
    public float spawnRadius = 0.5f;
    public bool randomizePosition = true;
    public bool randomizeRotation = true;
    public bool deactivateOnReturn = false;
    public Color gizmoColor;
    public UnityEvent<TargetBhv> onTargetSelection = new UnityEvent<TargetBhv>();

    // Private fields
    private ObjectPool<TargetBhv> _targetPool;
    private TargetBhv _currentTarget;

    protected override void Awake()
    {
        base.Awake();

        TargetBhv[] targets = this.GetComponentsInChildren<TargetBhv>();

        if (targets.Length == 0)
        {
            return;
        }

        _targetPool = new ObjectPool<TargetBhv>(targets);
    }

    private void OnEnable()
    {
        TaskManager.onTrialStart += this.HandleTrialStart;
    }

    private void OnDisable()
    {
        TaskManager.onTrialStart -= this.HandleTrialStart;
    }

    private void HandleTrialStart()
    {
        if (_targetPool == null)
        {
            return;
        }

        this.SelectTarget();
    }

    private void SelectTarget()
    {
        if (_currentTarget != null)
        {
            _targetPool.Return(_currentTarget, deactivate: deactivateOnReturn);
        }

        _currentTarget = _targetPool.GetRandom();

        if (randomizePosition)
        {
            _currentTarget.Position = this.Position + UnityEngine.Random.insideUnitSphere * spawnRadius;
        }
        if (randomizeRotation)
        {
            _currentTarget.Rotation = UnityEngine.Random.rotation;
        }

        _currentTarget.Restart();

        onTargetSelection?.Invoke(_currentTarget);
    }

    private void OnDrawGizmosSelected()
    {
        if (!randomizePosition)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(this.Position, spawnRadius);
    }
}
