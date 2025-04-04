using FirebaseAdmin.Auth;
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
    ILogger<UserAPI> logger,
    JsonSerializerOptions jsonOptions)
{

    [Function("login")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var data = await JsonSerializer.DeserializeAsync<LoginRequest>(req.Body, jsonOptions);

        var firebase = await FirebaseUtil.GetFirebaseAuth(cancellationToken);

        FirebaseToken decodedToken = await firebase.VerifyIdTokenAsync(data.Token);
        string uid = decodedToken.Uid;

        // todo make sure user exists in db

        return new JsonResult(new LoginResponse()
        {
            FullName = decodedToken.Claims.GetValueOrDefault("name")?.ToString(),
            Email = decodedToken.Claims.GetValueOrDefault("email")?.ToString(),
            UserIdEncrypted = EncryptionHelper.Encrypt(uid)
        });
    }
}
