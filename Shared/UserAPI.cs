namespace Shared.UserAPI;

public static class Constants
{
    public const string USER_INFO_COOKIE = "USER_INFO_COOKIE";
    public const string USER_LOGIN_COOKIE = "USER_LOGIN_COOKIE";
}


public record LoginRequest(string token, string? name, string? email);
