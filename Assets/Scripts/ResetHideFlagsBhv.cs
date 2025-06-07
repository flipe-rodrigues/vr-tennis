using UnityEngine;

public class ResetHideFlagsBhv : MonoBehaviour
{
    private void OnValidate()
    {
        gameObject.hideFlags = HideFlags.None;
    }
}
