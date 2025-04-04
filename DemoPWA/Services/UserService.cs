using BitzArt.Blazor.Cookies;
using Microsoft.JSInterop;
using Shared.UserAPI;
using System.Net.Http.Json;

namespace DemoPWA.Services;

public class UserService(
    ICookieService cookieService,
    IJSRuntime jsRuntime,
    IHttpClientFactory httpClientFactory
    ) : NotifyPropertyChangedBase, INeedInit
{
    private string? userName = null;

    public string? UserName
    {
        get => userName;
        set => SetProperty(ref userName, value);
    }
    public bool WasInit { get; private set; }

    public async Task Init()
    {
        UserName = await GetUserNameFromCookie();
        WasInit = true;
    }

    private async Task<string?> GetUserNameFromCookie()
    {
        return (await cookieService.GetAsync(Shared.UserAPI.Constants.USER_INFO_COOKIE))?.Value;
    }

    public async Task SignInWithGoogle()
    {
        var httpClient = httpClientFactory.CreateClient("BFF");

        var googleIdToken = await jsRuntime.InvokeAsync<string>("firebase.signinWithGoogle");

        var bffResponse = await httpClient.PostAsJsonAsync("/api/login", new LoginRequest()
        {
            Token = googleIdToken,
        });

        if (bffResponse.IsSuccessStatusCode)
        {
            UserName = await GetUserNameFromCookie();
        }
    }
}
