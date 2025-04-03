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
        CookieOptions option = new CookieOptions();
        option.Expires = DateTime.Now.AddMonths(1);
        option.HttpOnly = false;
        option.Secure = true;
        req.HttpContext.Response.Cookies.Append(Constants.USER_INFO_COOKIE, apiData.FullName, option);

        return new OkObjectResult($"Login succesffull");
    }
}
