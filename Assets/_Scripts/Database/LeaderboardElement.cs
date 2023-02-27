public class LeaderboardElement
{
    public int position;

    public string name;

    public int score;

    public string iamgeUrl;

    public bool isYourPosition;

    public LeaderboardElement(int position, string name, int score, string imageUrl, bool isYourPosition = false)
    {
        this.position = position;
        this.name = name;
        this.score = score;
        this.iamgeUrl = imageUrl;
        this.isYourPosition = isYourPosition;
    }
}
