using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Web;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Client.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly AuthStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider,
            ILocalStorageService localStorage)
        {
            _http = http;
            _authStateProvider = (AuthStateProvider)authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<AuthenticatedUser> Login(string userName, string password)
        {
            try
            {
                var res = await _http.GetFromJsonAsync<AuthenticatedUser>($"/api/v1/Account/Login?userName={HttpUtility.UrlEncode(userName)}&password={HttpUtility.UrlEncode(password)}");
                await _localStorage.SetItemAsync("authToken", res.Token);
                _authStateProvider.NotifyUserAuthentication(res.Token);
                _http.DefaultRequestHeaders.Authorization = new("bearer", res.Token);
                return res;
            }
            catch
            {
                return null;
            }
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _authStateProvider.NotifyUserLogout();
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }
}
