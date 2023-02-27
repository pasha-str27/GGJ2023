using UnityEngine;
using System.IO;
using System;

public static class PlayerData
{
    private class Properties
    {
        public int id;

        public string name;
        public string email;

        public int bestScore;

        public bool isConnectedWithGoogle;

        public DateTime syncTime;
        public bool needDatabaseSave;

        public Properties(string name, string email, int bestScore, bool isConnectedWithGoogle,
            DateTime syncTime, bool needDatabaseSave)
        {
            this.id = 0;
            this.name = name;
            this.email = email;
            this.bestScore = bestScore;
            this.isConnectedWithGoogle = isConnectedWithGoogle;
            this.syncTime = syncTime;
            this.needDatabaseSave = needDatabaseSave;
        }
    }
    
    private static Properties _properties;

    private static string _filePath;

    public static int Id => _properties.id;

    public static string Email => _properties.email;

    public static string Name => _properties.name;

    public static int BestScore => _properties.bestScore;

    public static bool IsConnectedWithGoogle => _properties.isConnectedWithGoogle;

    public static bool NeedDatabaseSave => _properties.needDatabaseSave;

    public static DateTime SyncTime => _properties.syncTime;

    public static void Init(ConfirmWindow confirmWindow)
    {
        _filePath = Path.Combine(Application.persistentDataPath, "player_data.json");

        if(File.Exists(_filePath))
            Load();
            //ResetProperties();

        else
            ResetProperties();

        if(IsConnectedWithGoogle)
            Database.RunAsync(Database.LoginAsync(confirmWindow));

        Database.SubscribeToSuccessfulSave(delegate { ChangeNeedDatabaseSaveStatus(false); });
        Database.SubscribeToSuccessfulLoad(delegate { ChangeConnectedWithGoogleStatus(true); });
        Database.SubscribeToSuccessfulRegister(delegate { ChangeConnectedWithGoogleStatus(true); });
    }

    private static void Load()
    {
        _properties = JSONConverter.FromJson<Properties>(File.ReadAllText(_filePath));
    }

    public static void Save(bool saveToDatabase = true)
    {
        File.WriteAllText(_filePath, JSONConverter.ToJson(_properties));

        if(saveToDatabase && _properties.isConnectedWithGoogle)
        {
            _properties.needDatabaseSave = true;

            Database.RunAsync(Database.SaveAsync(delegate { _properties.needDatabaseSave = false; }));
        }
    }

    public static void ChangeId(int id)
    {
        _properties.id = id;

        Save(false);
    }
     
    public static void ChangeName(string name)
    {
        _properties.name = name;

        Save(false);
    }

    public static void ChangeEmail(string email)
    {
        _properties.email = email;

        Save(false);
    }

    public static void ChangeBestScore(int score)
    {
        _properties.bestScore = score;

        Save();
    }

    public static void ChangeConnectedWithGoogleStatus(bool status)
    {
        _properties.isConnectedWithGoogle = status;

        Save(false);
    }

    public static void ChangeNeedDatabaseSaveStatus(bool status)
    {
        _properties.needDatabaseSave = status;

        Save(false);
    }

    public static void ChangeSyncTime(DateTime syncTime)
    {
        _properties.syncTime = syncTime;

        Save(false);
    }

    public static void ResetProperties()
    {
        _properties = new Properties("user", "", 0, false, new DateTime(2000, 1, 1), false);

        Save(false);
    }

    public static void SetProperties(int id, string name, int bestScore, string email, DateTime syncTime)
    {
        _properties.id = id;
        _properties.name = name;
        _properties.bestScore = bestScore;
        _properties.email = email;
        _properties.syncTime = syncTime;

        Save(false);
    }
}
