using System.Threading.Tasks;
using UnityEngine;
using System;
using System.Collections.Generic;

public static class Database
{
    private static string _url = "https://intotheroots.000webhostapp.com/";

    private static string _connectedEmail = "";

    public static string CurrentEmail => PlayerData.Instance.Email;

    public static List<LeaderboardElement> Leaderboard;

    public static string ConnectedEmail
    {
        get
        {
            if(_connectedEmail == "")
                _connectedEmail = CurrentEmail;

            return _connectedEmail;
        }

        set => _connectedEmail = value;
    }

    public static async Task ConnectAsync(string connectedEmail, Action<int, string, int> confirmLoad = null)
    {
        ConnectedEmail = connectedEmail;

        var request = new WWW(_url + "connect.php", GenerateForm(false, out DateTime syncTime));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "no user found")
            RegisterAsync();
 
        else if (requestResults[0] == "successful")
            ConfirmLoad(requestResults, confirmLoad);
    }

    private static void ConfirmLoad(string[] requestResults, Action<int, string, int> confirmLoad = null)
    {
        if(CurrentEmail != ConnectedEmail)
        {    
            ParseProperties(requestResults, out int id, out string name, out int score);

            confirmLoad?.Invoke(id, name, score);
        }
    }

    private static async Task RegisterAsync()
    {
        var request = new WWW(_url + "register.php", GenerateForm(true, out DateTime syncTime));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if(requestResults[0] == "successful")
        {
            if(PlayerData.Instance.IsConnectedWithGoogle)
            {
                PlayerData.Instance.ResetProperties();
                PlayerData.Instance.ChangeConnectedWithGoogleStatus(true);
                PlayerData.Instance.ChangeLastSyncTime(syncTime);
            }

            int.TryParse(requestResults[1], out int id);

            PlayerData.Instance.ChangeId(id);
            PlayerData.Instance.ChangeEmail(ConnectedEmail);

            PlayerData.Instance.Save();
        }
    }

    public static async Task LoadAsync()
    {
        var request = new WWW(_url + "load.php", GenerateForm(true, out DateTime syncTime));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            ParseProperties(requestResults, out int id, out string name, out int score);

            PlayerData.Instance.ChangeEmail(ConnectedEmail);
            PlayerData.Instance.ChangeConnectedWithGoogleStatus(true);
            PlayerData.Instance.ChangeLastSyncTime(syncTime);           
            PlayerData.Instance.SetProperties(id, name, score);
            PlayerData.Instance.Save();
        }
    }

    public static async Task LoginAsync()
    {
        var request = new WWW(_url + "leaderboard.php", GenerateForm(false, out DateTime syncTime));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            var sqlTime = PlayerData.Instance.LastSyncTime.ToString("yyyy-MM-dd HH:mm:ss");

            int.TryParse(requestResults[1], out int id);

            if(sqlTime != requestResults[1])
            {
                LoadAsync();
            }
        }

        Print(requestResults);
    }

    public static async Task SaveAsync(Action onSuccessfulSave = null)
    {
        var request = new WWW(_url + "save.php", GenerateForm(true, out DateTime syncTime));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            onSuccessfulSave?.Invoke();

            PlayerData.Instance.ChangeLastSyncTime(syncTime);
        }

        Print(requestResults);
    }

    public static async Task LoadLeaderboardAsync()
    {
        var request = new WWW(_url + "leaderboard.php", GenerateForm(true, out DateTime syncTime));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if(requestResults[0] == "successful")
        {
            Leaderboard = new List<LeaderboardElement>();

            int position = 1;

            for(int i = 1; i < requestResults.Length; i++)
            {
                if(i == 11)
                {
                    int.TryParse(requestResults[i], out position);

                    Leaderboard.Add(new LeaderboardElement(position, PlayerData.Instance.Name, PlayerData.Instance.BestScore, null));

                    break;
                }

                var name = requestResults[i];
                i++;
                int.TryParse(requestResults[i], out int score);

                Leaderboard.Add(new LeaderboardElement(position, name, score, null));

                position++;
            }
        }

        Print(requestResults);
    }

    private static WWWForm GenerateForm(bool addSyncTime, out DateTime syncTime)
    {
        var form = new WWWForm();
        var playerData = PlayerData.Instance;
        syncTime = DateTime.Now;
        
        form.AddField("id", PlayerData.Instance.Id);
        form.AddField("email", ConnectedEmail);
        form.AddField("name", playerData.Name);
        form.AddField("score", playerData.BestScore);

        if(addSyncTime)
        {
            var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

            form.AddField("sync_time", sqlSyncTime);
        }

        return form;
    }

    private static void ParseProperties(string[] requestResults, out int id, out string name, out int score)
    {
        int.TryParse(requestResults.Length > 1 ? requestResults[1] : "0", out id);
        int.TryParse(requestResults.Length > 3 ? requestResults[3] : "0", out score);

        name = requestResults[2];
    }

    private static void Print(string[] requestResults)
    {
        var toPrint = "From Server: ";

        foreach(var line in requestResults)
            toPrint += line + " ";

        Debug.Log(toPrint);

        PlayerData.Instance.Print();
    }
}
