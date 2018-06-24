using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    public Text text_rank;
    public Text text_username;
    public Text text_score;

    public void Set(string rank, string username, string score)
    {
        text_rank.text = rank;
        text_username.text = username;
        text_score.text = score;
    }
}
