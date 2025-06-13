using UnityEngine;
using System.Globalization;
using System.IO;
using System;

public class DataManager : Singleton<DataManager>
{
    // Static fields
    public static string dataPath = 
        Path.Combine(Application.isEditor? Application.dataPath : Application.persistentDataPath, "Data").
        Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    
    // Public fields
    public bool saveData;

    // Read only fields
    [SerializeField, ReadOnly]
    private string _dataPath;

    private void OnValidate()
    {
        _dataPath = dataPath;
    }

    protected override void Awake()
    {
        base.Awake();

        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
    }

    private void Start()
    {
        this.HomogeneizeAcrossCulturalSettings();
    }

    private void HomogeneizeAcrossCulturalSettings()
    {
        CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        customCulture.NumberFormat.NumberGroupSeparator = ",";
        CultureInfo.DefaultThreadCurrentCulture = customCulture;
        CultureInfo.DefaultThreadCurrentUICulture = customCulture;
    }

    public static string GetFileName(string gameObjectName, string format = ".csv")
    {
        return 
            $"{UIManager.subjectName}_" +
            $"{UIManager.subjectAge}_" +
            $"{UIManager.subjectSex}_" +
            $"{UIManager.subjectTennisExp}_" +
            $"{UIManager.subjectVRExp}_" +
            $"{gameObjectName.ToLower().Replace(" ", "-")}_" +
            $"{GetFormattedTimestamp()}." +
            $"{format}";
    }

    public static string GetFormattedTimestamp()
    {
        return DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }
}
