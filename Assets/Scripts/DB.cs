using System;

public static class DB
{
    public enum UserAuthTypes { LOGIN, SIGNUP, SAVE_GOOGLE_DATA }
    public const string URL_USER = "http://fruitartist.vertecx.net/api/v1/users";
}


[Serializable]
public class UserError
{
    public string error;
    public string name;
    public string email;
    public string password;
}