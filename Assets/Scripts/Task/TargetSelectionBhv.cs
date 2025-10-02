using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TargetSelectionBhv : MonoBehaviour
{
    // Readonly fields
    [SerializeField, ReadOnly]
    private List<TargetBhv> _targets;

    private void OnEnable()
    {
        TaskManager.onTrialStart += this.EnableRandomTarget;
    }

    private void OnDisable()
    {
        TaskManager.onTrialStart -= this.EnableRandomTarget;
    }

    private void OnValidate()
    {
        if (_targets == null || _targets.Count == 0)
        {
            _targets = this.GetComponentsInChildren<TargetBhv>().ToList();
        }
    }

    private void EnableRandomTarget()
    {
        int index = Random.Range(0, _targets.Count);

        foreach (TargetBhv target in _targets)
        {
            target.Active = target == _targets[index];
        }
    }
}
