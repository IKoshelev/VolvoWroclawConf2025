﻿namespace Shared.UserAPI
{
    public static class Constants
    {
        public const string USER_INFO_COOKIE = "USER_INFO_COOKIE";
        public const string USER_LOGIN_COOKIE = "USER_LOGIN_COOKIE";
    }

    public class LoginRequest
    {
        public string Token { get; set; }
    };

    public class LoginResponse
    {
        public string UserIdEncrypted { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    public class GetNoreResponse
    {
        public string Note { get; set; }
    }

    public class UserNotificationRequest
    {
        public string FcmToken { get; set; }
        public string Text { get; set; }
    }
}