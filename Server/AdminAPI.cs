using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Server.DB;
using Microsoft.Azure.Functions.Worker;
using FirebaseAdmin.Messaging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading.Tasks;

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

    [Function("send-delayed-notification-ontimer-manual")]
    public async Task<IActionResult> SendDelayedNotificationManual(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        await SendOutNotifications(default);

        return new OkObjectResult("");
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

    [Function("send-delayed-notification-ontimer")]
    [FixedDelayRetry(5, "00:00:10")]
    public async Task SendOutDelayedNotificationsOnTimer(
        [TimerTrigger("*/15 * * * * *")] TimerInfo myTimer,
        FunctionContext context)
    {
        await SendOutNotifications(default);
    }

    private async Task SendOutNotifications(CancellationToken cancellation)
    {
        using var db = new CosmosDBContext();

        var now = DateTime.UtcNow;

        var notifications = db.DelayedNotificayions.Where(x => x.TimeUTC <= now).ToArray();

        var firebaseMessaging = await FirebaseUtil.GetFirebaseMessaging(cancellation);

        foreach (var notification in notifications.Where(x => !string.IsNullOrEmpty(x.UserID)))
        {
            try
            {
                var user = db.Users.Where(x => x.UserId == notification.UserID).Single();
                foreach (var fcmToken in user.FCMTokens ?? [])
                {
                    // Construct message
                    var message = new Message
                    {
                        Token = fcmToken,
                        Notification = new Notification
                        {
                            Title = "Volvo Wroclaw Conf 2025",
                            Body = notification.Text
                        }
                    };

                    // Send push notification
                    string response = await firebaseMessaging.SendAsync(message);
                }

                db.DelayedNotificayions.Remove(notification);
            }
            catch (Exception ex)
            {
            }
        }

        await db.SaveChangesAsync();
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
