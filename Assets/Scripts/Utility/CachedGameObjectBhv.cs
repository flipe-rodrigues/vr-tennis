using UnityEngine;

public class CachedGameObjectBhv : MonoBehaviour
{
    // Public properties
    public string Name { get { return this.GameObject.name; } set { this.GameObject.name = value; } }
    public bool Active { get { return this.GameObject.activeSelf; } set { this.GameObject.SetActive(value); } }

    // Private properties
    private GameObject GameObject => _gameObject == null ? this.gameObject : _gameObject;

    // Private fields
    private GameObject _gameObject;

    protected virtual void Awake()
    {
        _gameObject = this.GameObject;
    }
}
