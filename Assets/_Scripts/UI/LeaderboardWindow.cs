using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LeaderboardWindow : MonoBehaviour
{
    [SerializeField] private List<GameObject> _leaderboadElements = new List<GameObject>();

    [SerializeField] private GameObject _playerMark;

    public void Init()
    {
        Database.SubscribeToSuccessfulRegister(UpdateLeaderboard);
        Database.SubscribeToSuccessfulLoad(UpdateLeaderboard);
        Database.SubscribeToSuccessfulSave(UpdateLeaderboard);

        ClearAllPositions();
        UpdateLeaderboard();
    }

    public async void UpdateLeaderboard()
    {
        await Database.LoadLeaderboardAsync();

        var leaderboard = Database.Leaderboard;

        if(leaderboard.Count > 0)
        {
            ClearAllPositions();

            for(int i = 0; i < leaderboard.Count; i++)
            {
                var positionLabel = _leaderboadElements[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                var iconImage = _leaderboadElements[i].transform.GetChild(1).GetComponent<Image>();
                var nameLabel = _leaderboadElements[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                var scoreLabel = _leaderboadElements[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>();

                positionLabel.text = leaderboard[i].position + "";
                nameLabel.text = leaderboard[i].isYourPosition ? "you" : leaderboard[i].name;
                scoreLabel.text = leaderboard[i].score + "";

                iconImage.enabled = true;
                await LoadSrite(iconImage, leaderboard[i].iamgeUrl);
            }

            _playerMark.SetActive(leaderboard.Count == 6);
            _leaderboadElements[5].SetActive(leaderboard.Count == 6);
        }
    }

    private void ClearAllPositions()
    {
        foreach(var position in _leaderboadElements)
        {
            var positionLabel = position.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var iconImage = position.transform.GetChild(1).GetComponent<Image>();
            var nameLabel = position.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var scoreLabel = position.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            positionLabel.text = "";
            iconImage.enabled = false;
            nameLabel.text = "";
            scoreLabel.text = "";
        }
    }

    private async Task LoadSrite(Image image, string url)
    {
        using(var request = UnityWebRequestTexture.GetTexture(url))
        {
            request.SendWebRequest();

            while(!request.isDone)
                await Task.Yield();

            var texture = DownloadHandlerTexture.GetContent(request);

            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }
    }
}
