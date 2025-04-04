using BitzArt.Blazor.Cookies;
using BlazorBootstrap;
using Microsoft.JSInterop;
using Shared.UserAPI;
using System.Net.Http.Json;
using System.Web;

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
        var cookie = await cookieService.GetAsync(Shared.UserAPI.Constants.USER_INFO_COOKIE);
        
        if (cookie == null)
        {
            return null;
        }

        return HttpUtility.UrlDecode(cookie.Value);
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

    public async Task Signout()
    {
        var httpClient = httpClientFactory.CreateClient("BFF");

        await httpClient.PostAsync("/api/logout", null);

        UserName = await GetUserNameFromCookie();  
    }

    public async Task<String> GetNote()
    {
        var httpClient = httpClientFactory.CreateClient("BFF");

        var note = await httpClient.GetFromJsonAsync<string>("/api/get-note");

        return note;
    }

    public async Task<bool> SetNote(string note)
    {
        var httpClient = httpClientFactory.CreateClient("BFF");

        var resp = await httpClient.PostAsync("/api/set-note", new StringContent(note));

        return resp.IsSuccessStatusCode;
    }
}
