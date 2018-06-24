using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : Singleton<LeaderboardController>
{
    public List<Leaderboard> Leaderboard;

    void Start()
    {
        Leaderboard = new List<Leaderboard>();
    }

    public void GetLeaderboard(string userName, int level, Action callback = null)
    {
        StartCoroutine(GetLeaderboardCR(userName, level, callback));
    }

    private IEnumerator GetLeaderboardCR(string userName, int level, Action callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.ScoreAuthTypes.GET_LEADERBOARD);
        form.AddField("username", userName);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_SCORE, form))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                Leaderboard.Clear();

                // Parse JSON
                Leaderboard = JsonHelper.FromJson<Leaderboard>(www.text).ToList();

                if (callback != null)
                    callback();
            }
            else
            {
                Debug.LogWarning(www.error);
            }
        }
    }
}

[Serializable]
public class Leaderboard
{
    public string rank;
    public string username;
    public string score;
}