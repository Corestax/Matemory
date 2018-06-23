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
        print(userName);
        print(level);
        StartCoroutine(GetLeaderboardCR(userName, level, callback));
    }

    IEnumerator GetLeaderboardCR(string userName, int level, Action callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("auth_type", (int)DB.UserAuthTypes.GET_LEADERBOARD);
        form.AddField("username", userName);
        form.AddField("level", level);

        using (WWW www = new WWW(DB.URL_USER, form))
        {
            yield return www;

            if (!string.IsNullOrEmpty(www.error))
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
    public string Rank;
    public string Player;
    public string Score;
}