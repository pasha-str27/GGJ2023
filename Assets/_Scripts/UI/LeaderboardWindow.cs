using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.UI;

public class LeaderboardWindow : MonoBehaviour
{
    [SerializeField] private List<GameObject> _leaderboadElements = new List<GameObject>();

    [SerializeField] private GameObject _playerMark;

    public async void UpdateLeaderboard()
    {
        await Database.LoadLeaderboardAsync();

        var leaderboard = Database.Leaderboard;

        for(int i = 0; i < leaderboard.Count; i++)
        {
            var positionLabel = _leaderboadElements[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var iconImage = _leaderboadElements[i].transform.GetChild(1).GetComponent<Image>();
            var nameLabel = _leaderboadElements[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            var scoreLabel = _leaderboadElements[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            positionLabel.text = leaderboard[i].position + "";
            nameLabel.text = leaderboard[i].name;
            scoreLabel.text = leaderboard[i].score + "";

            iconImage.enabled = true;
            await LoadSrite(iconImage, leaderboard[i].iamgeUrl);
        }

        _playerMark.SetActive(leaderboard.Count == 6);
        _leaderboadElements[5].SetActive(leaderboard.Count == 6);
    }

    private async Task LoadSrite(Image image, string url)
    {
        var request = new WWW(url);

        while(!request.isDone)
            await Task.Yield();

        image.sprite = Sprite.Create(request.texture, new Rect(0, 0, request.texture.width, request.texture.height), new Vector2(0, 0));
    }
}
