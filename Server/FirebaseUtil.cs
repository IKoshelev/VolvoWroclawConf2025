using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Server;

internal static class FirebaseUtil
{
    private static readonly SemaphoreSlim applicationInitLock = new(1);

    public async static Task<FirebaseAuth> GetFirebaseAuth(CancellationToken cancellationToken)
    {
        await GetFirebaseApp(cancellationToken);
        return FirebaseAuth.DefaultInstance;
    }

    public async static Task<FirebaseApp> GetFirebaseApp(
        CancellationToken cancellationToken)
    {
        if (FirebaseApp.DefaultInstance != null)
        {
            return FirebaseApp.DefaultInstance;
        }

        await applicationInitLock.WaitAsync();

        try
        {
            // Authenticate Firebase
            if (FirebaseApp.DefaultInstance == null)
            {
                // You have to avoid 
                var credential =
                    await GoogleCredential.FromFileAsync(
                        "./keys/volvowroclawconf2025-firebase-adminsdk-fbsvc-2f3f10f5a2.json", 
                        cancellationToken);
                
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential
                });
            }
        }
        finally
        {
            applicationInitLock.Release();
        }
        
    
        return FirebaseApp.DefaultInstance;
    }
}
