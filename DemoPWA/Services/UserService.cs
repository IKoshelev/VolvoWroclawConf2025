//using BitzArt.Blazor.Cookies;

namespace DemoPWA.Services
{
    public class UserService(
        //ICookieService cookieService
        ): NotifyPropertyChangedBase, INeedInit
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
            //UserName = (await cookieService.GetAsync(Shared.UserAPI.Constants.USER_INFO_COOKIE))?.Value;
            WasInit = true;
        }

        public async Task ChangeUser()
        {
            if (UserName != null)
            {
                UserName = null;
            } 
            else
            {
                //await cookieService.SetAsync(new Cookie(
                //    Shared.UserAPI.Constants.USER_INFO_COOKIE,
                //    "bbbbbbbbbbbbbbbbbb",
                //    DateTimeOffset.Now.AddDays(30)));

                UserName = "bbbbbbbbbbbbbbbb";
            }


        }

    }
}
