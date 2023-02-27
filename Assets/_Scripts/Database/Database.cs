using System.Threading.Tasks;
using UnityEngine;
using Google;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class Database
{
    private static string _url = "https://intotheroots.000webhostapp.com/";//"http://localhost/";

    private static Action _onSuccessfulRegister;
    private static Action _onSuccessfulLoad;
    private static Action _onSuccessfulSave;

    public static List<LeaderboardElement> Leaderboard = new List<LeaderboardElement>();

    public static GoogleSignInUser ConnectedUser { get; private set; }
        
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

    private static async Task<string[]> Request(string url, WWWForm form)
    {
        string[] requestResults = null;

        using(var request = UnityWebRequest.Post(url, form))
        {
            request.SendWebRequest();

            while(!request.isDone)
                await Task.Yield();

            if(request.result == UnityWebRequest.Result.Success)
                requestResults = request.downloadHandler.text.Split("\t");    

            else
                Debug.Log(request.error);
        }

        Print(requestResults);

        return requestResults;
    }
    
    private static WWWForm GenerateForm(int id = -1, string email = "-1", string name = "-1", int score = -1, string sqlSyncTime = "-1", string imageUrl = "-1")
    {
        string[] names = { "id", "email", "name", "score", "sync_time", "image_url" };
        string[] values = { id.ToString(), email, name, score.ToString(), sqlSyncTime, imageUrl };
        var form = new WWWForm();

        for(int i = 0; i < names.Length; i++)
            if(values[i] != "-1")
                form.AddField(names[i], values[i]);     

        return form;
    }

    public static async Task ConnectAsync(GoogleSignInUser connectedUser, ConfirmWindow confirmWindow)
    {
        ConnectedUser = connectedUser;

        SceneController.Instance._console.text += '\n' + "start connect to: " + ConnectedUser.Email;

        var requestResults = await Request(_url + "connect.php", GenerateForm(-1, ConnectedUser.Email));

        if(requestResults != null && requestResults[0] == "no user found")
            RunAsync(RegisterAsync());

        else if (requestResults != null && requestResults[0] == "successful" && PlayerData.Email != ConnectedUser.Email)
        {
            var id = int.Parse(requestResults[1]);
            var name = requestResults[2];
            var score = int.Parse(requestResults[3]);
            var syntTime = DateTime.Parse(requestResults[4]);

            string text = "Do you want to switch to an account " + name + " with the best score " + score + "?";

            confirmWindow.OpenWindow(text, "Switch", "Cancel", delegate { RunAsync(LoadAsync()); });
        }
    }

    public static async Task RegisterAsync()
    {
        SceneController.Instance._console.text += '\n' + "start register " + ConnectedUser.Email;

        var score = PlayerData.IsConnectedWithGoogle ? 0 : PlayerData.BestScore;
        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        var requestResults = await Request(_url + "register.php", GenerateForm(-1, ConnectedUser.Email, ConnectedUser.DisplayName,
            score, sqlSyncTime, ConnectedUser.ImageUrl.ToString()));

        if(requestResults != null && requestResults[0] == "successful")
        {
            if(PlayerData.IsConnectedWithGoogle)
                PlayerData.ResetProperties();
            
            var id = int.Parse(requestResults[1]);

            PlayerData.SetProperties(id, ConnectedUser.DisplayName, score, ConnectedUser.Email, syncTime);

             _onSuccessfulRegister?.Invoke();
        }
    }

    public static async Task LoadAsync()
    {
        SceneController.Instance._console.text += '\n' + "start load";

        var email = ConnectedUser != null ? ConnectedUser.Email : PlayerData.Email;
        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        var requestResults = await Request(_url + "load.php", GenerateForm(-1, email, "-1", -1, sqlSyncTime));

        if (requestResults != null && requestResults[0] == "successful")
        {
            var id = int.Parse(requestResults[1]);
            var name = requestResults[2];
            var score = int.Parse(requestResults[3]);

            PlayerData.SetProperties(id, name, score, email, syncTime);

            _onSuccessfulLoad?.Invoke();
        }
    }

    public static async Task LoginAsync(ConfirmWindow confirmWindow)
    {
        SceneController.Instance._console.text += '\n' + "start login";

        string[] requestResults = await Request(_url + "login.php", GenerateForm(PlayerData.Id));

        if (requestResults != null && requestResults[0] == "successful")
        {
            var sqlSyncTime = PlayerData.SyncTime.ToString("yyyy-MM-dd HH:mm:ss");

            if(sqlSyncTime != requestResults[1])
            {
                string text = "A version conflict was detected. Load the last saved version or stay with the existing one?";

                confirmWindow.OpenWindow(text, "Load", "Stay", delegate { RunAsync(LoadAsync()); }, delegate { RunAsync(SaveAsync()); });
            }

            else
            {
                if(PlayerData.NeedDatabaseSave)
                    RunAsync(SaveAsync());
            }
        }
    }

    public static async Task SaveAsync(Action onSuccessfulSave = null)
    {
        SceneController.Instance._console.text += '\n' + "start save";

        var syncTime = DateTime.Now;
        var sqlSyncTime = syncTime.ToString("yyyy-MM-dd HH:mm:ss");

        string[] requestResults = await Request(_url + "save.php", GenerateForm(PlayerData.Id, "-1", "-1", PlayerData.BestScore,
            sqlSyncTime));

        if (requestResults != null && requestResults[0] == "successful")
        {
            PlayerData.ChangeSyncTime(syncTime);

            _onSuccessfulSave?.Invoke();
        }
    }
    
    public static async Task LoadLeaderboardAsync()
    {
        SceneController.Instance._console.text += '\n' + "start load leaderboard";

        string[] requestResults = await Request(_url + "leaderboard.php", GenerateForm(PlayerData.Id));

        if(requestResults != null && requestResults[0] == "successful")
        {
            Leaderboard = new List<LeaderboardElement>();

            for(int i = 5; i < requestResults.Length; i += 5)
            {
                int.TryParse(requestResults[i - 4], out int position);
                int.TryParse(requestResults[i - 3], out int id);
                var name = requestResults[i - 2];
                int.TryParse(requestResults[i - 1], out int score);
                var imageUrl = requestResults[i];

                Leaderboard.Add(new LeaderboardElement(position, name, score, imageUrl, id == PlayerData.Id));
            }
        }
    }
        
    public static void RunAsync(Task task)
    {
        task.ContinueWith(t =>
        {
            Debug.LogError(t.Exception);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private static void Print(string[] requestResults)
    {
        if(requestResults == null)
            return;

        var requestResult = "from server: ";

        foreach(var line in requestResults)
            requestResult += line + " ";

        SceneController.Instance._console.text += '\n' + requestResult;
    }

    public static void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
