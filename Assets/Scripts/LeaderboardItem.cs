using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    public Text text_rank;
    public Text text_player;
    public Text text_score;

    public void Set(string rank, string player, string score)
    {
        text_rank.text = rank;
        text_player.text = player;
        text_score.text = score;
    }
}
