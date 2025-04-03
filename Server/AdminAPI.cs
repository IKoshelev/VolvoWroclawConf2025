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
}
