using UnityEngine;
using System.IO;
using System;

public class PlayerData
{
    private class Properties
    {
        public int id;

        public string name;
        public string email;

        public int bestScore;

        public bool isConnectedWithGoogle;

        public DateTime lastSyncTime;
        public bool needDatabaseSave;

        public Properties(string name, string email, int bestScore, bool isConnectedWithGoogle,
            DateTime lastSyncTime, bool needDatabaseSave)
        {
            this.name = name;
            this.email = email;
            this.bestScore = bestScore;
            this.isConnectedWithGoogle = isConnectedWithGoogle;
            this.lastSyncTime = lastSyncTime;
            this.needDatabaseSave = needDatabaseSave;
        }
    }

    private static PlayerData _instance;
    
    private Properties _properties;

    private string _filePath;

    public int Id => _properties.id;

    public string Email => _properties.email;

    public string Name => _properties.name;

    public int BestScore => _properties.bestScore;

    public bool IsConnectedWithGoogle => _properties.isConnectedWithGoogle;

    public bool NeedDatabaseSave => _properties.needDatabaseSave;

    public DateTime LastSyncTime => _properties.lastSyncTime;

    public static PlayerData Instance
    {
        get
        {
            if(_instance == null)
                _instance = new PlayerData();

            return _instance;
        }
    }

    public PlayerData()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "player_data.json");

        if(File.Exists(_filePath))
            Load();

        else
            ResetProperties();

        Database.SubscribeToSuccessfulSave(delegate { ChangeNeedDatabaseSaveStatus(false); });
        Database.SubscribeToSuccessfulRegister(delegate { ChangeConnectedWithGoogleStatus(true); });
    }

    private void Load()
    {
        //ResetProperties();
        _properties = JSONConverter.FromJson<Properties>(File.ReadAllText(_filePath));
    }

    public void Save(bool saveToDatabase = true)
    {
        File.WriteAllText(_filePath, JSONConverter.ToJson(_properties));

        if(saveToDatabase && _properties.isConnectedWithGoogle)
        {
            _properties.needDatabaseSave = true;

            Database.SaveAsync(delegate { _properties.needDatabaseSave = false; });
        }
    }

    public void ChangeId(int id)
    {
        _properties.id = id;

        Save(false);
    }
     
    public void ChangeName(string name)
    {
        _properties.name = name;

        Save(false);
    }

    public void ChangeEmail(string email)
    {
        _properties.email = email;

        Save(false);
    }

    public void ChangeBestScore(int score)
    {
        _properties.bestScore = score;

        Save();
    }

    public void ChangeConnectedWithGoogleStatus(bool status)
    {
        _properties.isConnectedWithGoogle = status;

        Save(false);
    }

    public void ChangeNeedDatabaseSaveStatus(bool status)
    {
        _properties.needDatabaseSave = status;

        Save(false);
    }

    public void ChangeLastSyncTime(DateTime time)
    {
        _properties.lastSyncTime = time;

        Save(false);
    }

    public void ResetProperties()
    {
        _properties = new Properties("new_user", "", 0, false, new DateTime(2000, 1, 1), false);

        Save(false);
    }

    public void SetProperties(int id, string name, int bestScore)
    {
        _properties.id = id;
        _properties.name = name;
        _properties.bestScore = bestScore;

        Save(false);
    }

    public void Print()
    {
        Debug.Log("Saved: " + _properties.id + " " + _properties.email + " " + _properties.name + " " + _properties.bestScore);
    }
}
