using UnityEngine;
using System.Globalization;
using System.IO;
using System;

public class DataManager : Singleton<DataManager>
{
    // Static fields
    public static string savePath = GetFormattedSavePath(Application.isEditor ? Application.dataPath : Application.persistentDataPath);

    // Public fields
    public bool saveData;
    public bool saveMetadata;

    // Read only fields
    [SerializeField, ReadOnly]
    private string _editorSavePath;
    [SerializeField, ReadOnly]
    private string _buildSavePath;

    protected override void OnValidate()
    {
        base.OnValidate();

        _editorSavePath = GetFormattedSavePath(Application.dataPath);
        _buildSavePath = GetFormattedSavePath(Application.persistentDataPath);
    }

    protected override void Awake()
    {
        base.Awake();

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
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

    private static string GetFormattedSavePath(string dataPath)
    {
        return Path.Combine(dataPath, "Data").Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }
}
