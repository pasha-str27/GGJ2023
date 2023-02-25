using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] private ConfirmWindow _confirmWindow;

    [SerializeField] private string email;
    [SerializeField] private int bestScore;

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    public void OnConnectClick()
    {
        //GoogleConnect.Instance.Connect(OnSuccessfulConnect);

        OnSuccessfulConnect(email);
    }

    private async void OnSuccessfulConnect(string email)
    {
        await Database.ConnectAsync(email, ConfirmLoad);
    }

    [ContextMenu("Save")]
    private void TEMP_SAVE()
    {
        PlayerData.Instance.ChangeBestScore(bestScore);
    }
    
    private void ConfirmLoad(int id, string name, int bestScore)
    {      
        string text = "Do you want switch to '" + name + "' with the maximum score '" + bestScore + "'?";

        _confirmWindow.OpenWindow(text, delegate { Database.LoadAsync(); });
    }
}
