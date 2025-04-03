using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Shared.UserAPI;
using System.Net.Http.Json;
using System.Text.Json;

namespace DemoPWA.API;

public class BffFunctions(
    ILogger<BffFunctions> logger,
    IHttpClientFactory httpClientFactory)
{

    [Function("login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        using var httpClient = httpClientFactory.CreateClient("API");

        var data = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body);

        var apiResponse = await httpClient.PostAsJsonAsync("login", data);

        if (!apiResponse.IsSuccessStatusCode)
        {
            return new UnauthorizedObjectResult("");
        }

        var apiData = await apiResponse.Content.ReadFromJsonAsync<LoginResponse>();

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
}
