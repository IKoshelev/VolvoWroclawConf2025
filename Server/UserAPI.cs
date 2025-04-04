using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Server.DB;
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
        var userName = decodedToken.Claims.GetValueOrDefault("name")?.ToString();
        var userEmail = decodedToken.Claims.GetValueOrDefault("email")?.ToString();

        using var db = new CosmosDBContext();

        var user = db.Users.Where(x => x.UserId == uid).SingleOrDefault();

        if (user == null)
        {
            user = new User()
            {
                UserId = uid,
                FullName = userName,
                Email = userEmail
            };
            db.Users.Add(user);
        }

        user.LastSignIn = DateOnly.FromDateTime(DateTime.Now);

        await db.SaveChangesAsync();

        return new JsonResult(new LoginResponse()
        {
            FullName = decodedToken.Claims.GetValueOrDefault("name")?.ToString(),
            Email = decodedToken.Claims.GetValueOrDefault("email")?.ToString(),
            UserIdEncrypted = EncryptionHelper.Encrypt(uid)
        });
    }

    [Function("get-note")]
    public async Task<IActionResult> GetNote(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
    CancellationToken cancellationToken)
    {
        string userId = GetUserIdFromHeader(req);

        using var db = new CosmosDBContext();

        var user = db.Users.Where(x => x.UserId == userId).Single();

        return new OkObjectResult(user.Note ?? "");
    }


    [Function("set-note")]
    public async Task<IActionResult> SetNote(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
    CancellationToken cancellationToken)
    {
        string userId = GetUserIdFromHeader(req);

        using var stream = new StreamReader(req.Body);
        var note = await stream.ReadToEndAsync();

        using var db = new CosmosDBContext();

        var user = db.Users.Where(x => x.UserId == userId).Single();

        user.Note = note;

        await db.SaveChangesAsync();

        return new OkObjectResult("");
    }

    private static string GetUserIdFromHeader(HttpRequest req)
    {
        var header = req.Headers["x-user-id-encoded"];
        var user = EncryptionHelper.Decrypt(header);

        if (string.IsNullOrWhiteSpace(user))
        {
            throw new ArgumentException();
        }

        return user;
    }
}
