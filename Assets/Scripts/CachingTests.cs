using UnityEngine;

public class CachingTests : CachedTransformBhv
{
    public ComponentType componentToTest = ComponentType.Transform;
    public int testCount = 10000;
    public bool isCaching = true;

    [SerializeField, ReadOnly]
    private float _runTime;

    private string _name;
    private bool _isActive;
    private Vector3 _cachedPosition;
    private Quaternion _cachedRotation;

    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < testCount; i++)
        {
            if (isCaching)
            {
                switch (componentToTest)
                {
                    case ComponentType.GameObject:
                        
                        _name = this.Name;
                        _isActive = this.Active;

                        break;

                    case ComponentType.Transform:
                        
                        _cachedPosition = Transform.position;
                        _cachedRotation = Transform.rotation;

                        break;
                }
            }
            else
            {
                switch (componentToTest)
                {
                    case ComponentType.GameObject:

                        _name = this.gameObject.name;
                        _isActive = this.gameObject.activeSelf;

                        break;

                    case ComponentType.Transform:

                        _cachedPosition = transform.position;
                        _cachedRotation = transform.rotation;

                        break;
                }
            }
        }

        float stopTime = Time.realtimeSinceStartup;

        _runTime = stopTime - startTime;
    }
}

public enum ComponentType
{
    GameObject,
    Transform,
}
