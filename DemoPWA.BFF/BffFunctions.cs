using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.UserAPI;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DemoPWA.API;

public class BffFunctions(
    ILogger<BffFunctions> logger,
    IHttpClientFactory httpClientFactory,
    JsonSerializerOptions jsonOptions)
{

    [Function("login")]
    public async Task<IActionResult> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        using var httpClient = httpClientFactory.CreateClient("API");

        var data = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body, jsonOptions);

        var apiResponse = await httpClient.PostAsJsonAsync("login", data);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return new UnauthorizedObjectResult("");
        }

        var apiData = await apiResponse.Content.ReadFromJsonAsync<LoginResponse>(jsonOptions);

        /// public cookie for information
        req.HttpContext.Response.Cookies.Append(
            Constants.USER_INFO_COOKIE, 
            apiData.FullName, 
            new CookieOptions()
            {
                Expires = DateTime.Now.AddMonths(1),
                HttpOnly = false,
                Secure = true,
            });

        req.HttpContext.Response.Cookies.Append(
            Constants.USER_LOGIN_COOKIE, 
            apiData.UserIdEncrypted, 
            new CookieOptions()
            {
                Expires = DateTime.Now.AddMonths(1),
                HttpOnly = true,
                Secure = true,
            });

        return new OkObjectResult($"Login succesffull");
    }

    [Function("logout")]
    public async Task<IActionResult> Logout(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        /// public cookie for information
        req.HttpContext.Response.Cookies.Append(
            Constants.USER_INFO_COOKIE,
            "",
            new CookieOptions()
            {
                Expires = DateTime.Now.AddMinutes(5), // expired cookies stripped out of response?
                HttpOnly = false,
                Secure = true,
            });

        req.HttpContext.Response.Cookies.Append(
            Constants.USER_LOGIN_COOKIE,
            "",
            new CookieOptions()
            {
                Expires = DateTime.Now.AddMinutes(5),
                HttpOnly = true,
                Secure = true,
            });

        return new OkObjectResult($"Logout succesffull");
    }

    [Function("get-note")]
    public async Task<IActionResult> GetNote(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        using var httpClient = httpClientFactory.CreateClient("API");

        var message = new HttpRequestMessage(HttpMethod.Get, "get-note");

        SetAuthHeaderFromCookie(message, req);

        var apiResponse = await httpClient.SendAsync(message);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return new UnauthorizedObjectResult("");
        }

        var apiData = await apiResponse.Content.ReadAsStringAsync();

        return new OkObjectResult(apiData);
    }

    [Function("set-note")]
    public async Task<IActionResult> SetNote(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        using var stream = new StreamReader(req.Body);
        var note = await stream.ReadToEndAsync();

        using var httpClient = httpClientFactory.CreateClient("API");

        var message = new HttpRequestMessage(HttpMethod.Post, "set-note");

        SetAuthHeaderFromCookie(message, req);

        message.Content = new StringContent(note);

        var apiResponse = await httpClient.SendAsync(message);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return new UnauthorizedObjectResult("");
        }

        return new OkObjectResult("");
    }

    [Function("request-push-notification")]
    public async Task<IActionResult> RequestPushNotification(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var data = await JsonSerializer.DeserializeAsync<UserNotificationRequest>(req.Body, jsonOptions);

        using var httpClient = httpClientFactory.CreateClient("API");

        var message = new HttpRequestMessage(HttpMethod.Post, "request-push-notification");

        SetAuthHeaderFromCookie(message, req);

        message.Content = new StringContent(
            JsonSerializer.Serialize(data),
            Encoding.UTF8,
            "application/json");

        var apiResponse = await httpClient.SendAsync(message);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return new UnauthorizedObjectResult("");
        }

        return new OkObjectResult("");
    }

    public static void SetAuthHeaderFromCookie(HttpRequestMessage message, HttpRequest originalReq)
    {
        var cookieValue = originalReq.Cookies[Constants.USER_LOGIN_COOKIE];

#if DEBUG
        // to ease local testing
        cookieValue = "UUavlJ2tjRERTmntPJv3Nh6/xOev0FtZkE8QQ/yIer8=";
#endif

        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            throw new ArgumentException();
        }

        message.Headers.Add("x-user-id-encoded", cookieValue);
    }
}
