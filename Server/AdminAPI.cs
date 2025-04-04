using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Server.DB;

namespace Server;

/// <summary>
/// Methods in AdminAPI use AuthorizationLevel.Function,
/// you will have to run them from Azure Portal or use Azure Portal to get Function Keys.
/// Function keys may only be given to trusted actors.
/// </summary>
public class AdminAPI(
    ILogger<AdminAPI> logger)
{

    [Function("ensure-db-created")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        var res = await new CosmosDBContext().Database.EnsureCreatedAsync();

        return new OkObjectResult($"Ran OK, result: {res}");
    }

    [Function("create-mock-users")]
    public async Task<IActionResult> CreateMockUsers(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        using var db = new CosmosDBContext();

        var seed = Random.Shared.Next();

        for (int i = 1; i < 10; i++)
        {
            var user = new User()
            {
                UserId = Guid.NewGuid().ToString(),
                FullName = $"User {seed + i}",
                Email = $"User{seed + i}@fakeemai72346346463.com"
            };
            db.Users.Add(user);
        }

        await db.SaveChangesAsync();

        return new OkObjectResult($"Ran OK, result");
    }

#if DEBUG
    [Function("encrypt-user-id")]
    public async Task<IActionResult> EncryptUserId(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        using var stream = new StreamReader(req.Body);
        var note = await stream.ReadToEndAsync();
        return new OkObjectResult(EncryptionHelper.Encrypt(note));
    }
#endif

}
