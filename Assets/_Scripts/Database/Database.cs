using System.Threading.Tasks;
using UnityEngine;
using Google;
using System;
using System.Collections.Generic;

public static class Database
{
    private static string _url = "https://intotheroots.000webhostapp.com/";

    private static string _connectedEmail = "";

    private static Action _onSuccessfulRegister;
    private static Action _onSuccessfulLoad;
    private static Action _onSuccessfulSave;
    private static Action _onLoginWithoutConflicts;

    public static List<LeaderboardElement> Leaderboard;

    public static GoogleSignInUser ConnectedUser { get; private set; }

    public static string CurrentEmail => PlayerData.Instance.Email;

    public static string ConnectedEmail => ConnectedUser == null ? CurrentEmail : ConnectedUser.Email;
        
    public static void SubscribeToSuccessfulRegister(Action callBack)
    {
        _onSuccessfulRegister += callBack;
    }

    public static void SubscribeToSuccessfulLoad(Action callBack)
    {
        _onSuccessfulLoad += callBack;
    }

    public static void SubscribeToSuccessfulSave(Action callBack)
    {
        _onSuccessfulSave += callBack;
    }

    public static void SubscribeToLoginWithoutConflicts(Action callBack)
    {
        _onLoginWithoutConflicts += callBack;
    }

    public static async Task ConnectAsync(GoogleSignInUser connectedUser, ConfirmWindow confirmWindow)
    {
        ConnectedUser = connectedUser;

        var request = new WWW(_url + "connect.php", GenerateForm(0, ConnectedEmail));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "no user found")
            await RegisterAsync();
 
        else if (requestResults[0] == "successful" && CurrentEmail != ConnectedEmail)
        {
            ParseProperties(requestResults, out int id, out string name, out int score);

            string text = "Do you want switch to '" + name + "' with the maximum score '" + score + "'?";

            confirmWindow.OpenWindow(text, delegate { Database.LoadAsync(); });
        }
            
    }

    private static async Task RegisterAsync()
    {
        var score = PlayerData.Instance.IsConnectedWithGoogle ? 0 : PlayerData.Instance.BestScore;
        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        var request = new WWW(_url + "register.php", GenerateForm(0, ConnectedEmail, ConnectedUser.DisplayName,
            score, sqlSyncTime, ConnectedUser.ImageUrl.ToString()));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if(requestResults[0] == "successful")
        {
            if(PlayerData.Instance.IsConnectedWithGoogle)
            {
                PlayerData.Instance.ResetProperties();
            }

            int.TryParse(requestResults[1], out int id);

            PlayerData.Instance.ChangeEmail(ConnectedEmail);
            PlayerData.Instance.ChangeLastSyncTime(syncTime);
            PlayerData.Instance.SetProperties(id, ConnectedUser.DisplayName, score);

            _onSuccessfulRegister?.Invoke();
        }

        Print(requestResults);
    }

    public static async Task LoadAsync()
    {
        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        var request = new WWW(_url + "load.php", GenerateForm(0, ConnectedEmail, "", 0, sqlSyncTime));

        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            ParseProperties(requestResults, out int id, out string name, out int score);

            PlayerData.Instance.ChangeEmail(ConnectedEmail);
            PlayerData.Instance.ChangeLastSyncTime(syncTime);           
            PlayerData.Instance.SetProperties(id, name, score);

            _onSuccessfulLoad?.Invoke();
        }

        Print(requestResults);
    }

    public static async Task LoginAsync(ConfirmWindow confirmWindow)
    {
        var request = new WWW(_url + "login.php", GenerateForm(PlayerData.Instance.Id));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            var sqlSyncTime = PlayerData.Instance.LastSyncTime.ToString("yyyy-MM-dd HH:mm:ss");

            if(sqlSyncTime != requestResults[1])
            {
                string text = "Version conflict detected, download data from the server?";

                confirmWindow.OpenWindow(text, delegate { Database.LoadAsync(); }, delegate { Database.SaveAsync(); });
            }

            else
            {
                _onLoginWithoutConflicts?.Invoke();
            }
        }

        Print(requestResults);
    }

    public static async Task SaveAsync(Action onSuccessfulSave = null)
    {
        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        var request = new WWW(_url + "save.php", GenerateForm(PlayerData.Instance.Id, "", "", PlayerData.Instance.BestScore,
            sqlSyncTime));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if (requestResults[0] == "successful")
        {
            onSuccessfulSave?.Invoke();

            PlayerData.Instance.ChangeLastSyncTime(syncTime);

            _onSuccessfulSave?.Invoke();
        }

        Print(requestResults);
    }

    public static async Task LoadLeaderboardAsync()
    {
        var request = new WWW(_url + "leaderboard.php", GenerateForm(PlayerData.Instance.Id));
        
        while (!request.isDone)
            await Task.Yield();

        string[] requestResults = request.text.Split('\t');

        if(requestResults[0] == "successful")
        {
            Leaderboard = new List<LeaderboardElement>();

            var i = 1;

            while(i < requestResults.Length)
            {
                int.TryParse(requestResults[i], out int position);
                var name = requestResults[i + 1];
                int.TryParse(requestResults[i + 2], out int score);
                var imageUrl = requestResults[i + 3];

                Leaderboard.Add(new LeaderboardElement(position, name, score, imageUrl));

                i += 4;
            }
        }

        Print(requestResults);
    }

    private static WWWForm GenerateForm(int id = 0, string email = "", string name = "", int score = 0, string sqlSyncTime = "", string imageUrl = "")
    {
        var form = new WWWForm();
        
        form.AddField("id", id);
        form.AddField("email", email);
        form.AddField("name", name);
        form.AddField("score", score);
        form.AddField("sync_time", sqlSyncTime);
        form.AddField("image_url", imageUrl);

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
