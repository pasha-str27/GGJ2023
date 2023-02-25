using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Database UI")]
    [SerializeField] private LeaderboardWindow _leaderboardWindow;

    [SerializeField] private ConfirmWindow _confirmWindow;

    [Header("TEMP")]
    [SerializeField] private TMPro.TMP_InputField _input;
    [SerializeField] private string email;
    [SerializeField] private string username;

    private void Awake()
    {
        DatabaseAwake();
    }

    private async void DatabaseAwake()
    {
        Database.SubscribeToSuccessfulRegister(_leaderboardWindow.UpdateLeaderboard);
        Database.SubscribeToSuccessfulSave(_leaderboardWindow.UpdateLeaderboard);
        Database.SubscribeToSuccessfulLoad(_leaderboardWindow.UpdateLeaderboard);
        Database.SubscribeToLoginWithoutConflicts( _leaderboardWindow.UpdateLeaderboard);

        if(PlayerData.Instance.IsConnectedWithGoogle)
        {
            await Database.LoginAsync(_confirmWindow);

            if(PlayerData.Instance.NeedDatabaseSave)
            {
                await Database.SaveAsync();
            }
        }
    }

    public void OnConnectClick()
    {
        GoogleConnect.Instance.Connect(OnSuccessfulConnect);
        // Google.GoogleSignInUser user = new Google.GoogleSignInUser();
        // user.Email = email;
        // user.DisplayName = username;
        // user.ImageUrl = new System.Uri("https://picsum.photos/seed/picsum/200/300", System.UriKind.Absolute);

        // OnSuccessfulConnect(user);
    }

    private async void OnSuccessfulConnect(Google.GoogleSignInUser connectedUser)
    {
        await Database.ConnectAsync(connectedUser, _confirmWindow);
    }

    public void ChangeScore_TEMP()
    {
        if(int.TryParse(_input.text, out int score))
        {
            PlayerData.Instance.ChangeBestScore(score);
        }
    }
    
    private void ConfirmLoad(int id, string name, int bestScore)
    {      
        string text = "Do you want switch to '" + name + "' with the maximum score '" + bestScore + "'?";

        _confirmWindow.OpenWindow(text, delegate { Database.LoadAsync(); });
    }

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
