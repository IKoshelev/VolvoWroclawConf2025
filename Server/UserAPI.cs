using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Shared.UserAPI;
using System.Text.Json;

namespace Server;

/// <summary>
/// Methods in UserAPI use AuthorizationLevel.Anonymous to allow public calls,
/// and will authenticate user via cookies
/// </summary>
/// <param name="logger"></param>
public class UserAPI(
    ILogger<UserAPI> logger)
{

    [Function("login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var data = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body);

        // todo verify token

        return new JsonResult(new LoginResponse()
        {
            FullName = "Jhon Smith",
            Email = "a@b.c",
            UserId = "12345"
        });
    }
}
