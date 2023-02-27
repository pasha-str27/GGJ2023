using UnityEngine;
using UnityEngine.UI;

public class SceneController : SingletonComponent<SceneController>
{
    [Header("Database UI")]
    [SerializeField] private LeaderboardWindow _leaderboardWindow;

    [SerializeField] private ConfirmWindow _confirmWindow;

    [SerializeField] private Button _lanternButton;

    [SerializeField] private GameObject _lanternOff;
    [SerializeField] private GameObject _lanternOn;

    [Header("TEMP")]
    [SerializeField] private Button _button;
    [SerializeField] private TMPro.TMP_InputField _input;
    [SerializeField] private string email;
    [SerializeField] private string username;
    [SerializeField] public TMPro.TextMeshProUGUI _console;

    private void Awake()
    {
        PlayerData.Init(_confirmWindow);
        _leaderboardWindow.Init();

        ResetLanternButton();

        Database.SubscribeToSuccessfulRegister(ResetLanternButton);
        Database.SubscribeToSuccessfulLoad(ResetLanternButton);
    }

    public void OnConnectClick()
    {
        if(!PlayerData.IsConnectedWithGoogle)
        {
            //Google.GoogleSignInUser user = new Google.GoogleSignInUser();
            //user.DisplayName = username;
            //user.Email = email;
            //user.ImageUrl = new System.Uri("https://picsum.photos/id/237/200/300");

            //Database.RunAsync(Database.ConnectAsync(user, _confirmWindow));
            GoogleConnect.StartConnect(OnSuccesfulConnect);
        }

        else
        {
            var text = "Are you sure you want to log out of your account?";

            _confirmWindow.OpenWindow(text, "Log out", "Cancel", delegate
            { 
                PlayerData.ResetProperties();
                ResetLanternButton();
                _leaderboardWindow.UpdateLeaderboard();
            });

        }
    }

    private void OnSuccesfulConnect(Google.GoogleSignInUser user)
    {
        Database.RunAsync(Database.ConnectAsync(user, _confirmWindow));
    }

    private void ResetLanternButton()
    {
        _lanternOff.SetActive(!PlayerData.IsConnectedWithGoogle);
        _lanternOn.SetActive(PlayerData.IsConnectedWithGoogle);
    }

    public void ChangeScore_TEMP()
    {
        if(int.TryParse(_input.text, out int score))
        {
            PlayerData.ChangeBestScore(score);
            _button.enabled = false;

            Invoke(nameof(UnblockButton_TEMP), 10f);
        }
    }

    public void UnblockButton_TEMP()
    {   
        _button.enabled = true;
    }
}
