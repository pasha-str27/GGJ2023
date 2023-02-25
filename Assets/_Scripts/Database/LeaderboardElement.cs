using UnityEngine;

public class LeaderboardElement
{
    public int position;

    public string name;

    public int score;

    public Sprite iconSprite;

    public LeaderboardElement(int position, string name, int score, Sprite iconSprite)
    {
        this.position = position;
        this.name = name;
        this.score = score;
        this.iconSprite = iconSprite;
    }
}
