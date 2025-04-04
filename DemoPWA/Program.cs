using BitzArt.Blazor.Cookies;
using DemoPWA.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace DemoPWA
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            //builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            builder.Services.AddHttpClient("BFF",x =>
            {
                x.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });

            builder.Services.AddBlazorBootstrap();

            builder.AddBlazorCookies(ServiceLifetime.Singleton);

            builder.Services.AddSingleton<UserService>();
            builder.Services.AddSingleton<INeedInit>(x => x.GetRequiredService<UserService>());

            var host = builder.Build();

            var initTasks = host.Services.GetServices<INeedInit>().Select(x => x.Init());
            await Task.WhenAll(initTasks);

            await host.RunAsync();
        }
    }
}
