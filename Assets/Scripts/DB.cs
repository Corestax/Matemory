using System;

public static class DB
{
    public enum UserAuthTypes { LOGIN, SIGNUP, SAVE_GOOGLE_DATA, RESET_PASSWORD, RESEND_EMAIL, GET_LEADERBOARD, GET_HIGHSCORE, SAVE_HIGHSCORE }
    public const string URL_USER = "https://fruitartist.vertecx.net/api/v1/users";
}


[Serializable]
public class DBResponseMessage
{
    public string message;
}


[Serializable]
public class DBResponseUserData
{
    public string name;
    public string email;
}


[Serializable]
public class DBResponseUserError
{
    public string error;
    public string name;
    public string email;
    public string password;
}
