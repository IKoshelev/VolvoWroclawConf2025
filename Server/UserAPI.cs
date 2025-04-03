using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        //// Configure the cookie
        CookieOptions option = new CookieOptions();
        option.Expires = DateTime.Now.AddMonths(1);
        //option.Domain = "volvo-wroclaw-conf-2025-api.azurewebsites.net";
        //option.Path = "/";
        option.HttpOnly = true;
        option.Secure = true;
        //option.SameSite = SameSiteMode.None;

        //// A little non logical way to actually get the HttpResponse (from the HttpRequest and its HttpContext)
        req.HttpContext.Response.Cookies.Append("ggggggggggggggg", "ddddddddddddddddd", option);


        logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}
