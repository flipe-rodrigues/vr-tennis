using UnityEngine;
using System.Text.RegularExpressions;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject();

                    singletonObject.name = typeof(T).ToString();

                    _instance = singletonObject.AddComponent<T>();
                }
            }

            return _instance;
        }
    }
    protected virtual void OnValidate()
    {
        string className = this.GetType().Name;

        string spacedName = Regex.Replace(className, "(?<!^)([A-Z])", " $1");

        this.name = spacedName;
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);

            return;
        }

        else
        {
            _instance = GetComponent<T>();
        }

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}