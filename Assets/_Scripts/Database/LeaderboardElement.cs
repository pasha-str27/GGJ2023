using UnityEngine;

public class LeaderboardElement
{
    public int position;

    public string name;

    public int score;

    public string iamgeUrl;

    public LeaderboardElement(int position, string name, int score, string imageUrl)
    {
        this.position = position;
        this.name = name;
        this.score = score;
        this.iamgeUrl = imageUrl;
    }
}
