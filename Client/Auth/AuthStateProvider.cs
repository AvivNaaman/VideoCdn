using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace VideoCdn.Web.Client.Auth
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState anonymus;

        public AuthStateProvider(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
            anonymus = new(new());
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (token is null or "") return anonymus;

            _http.DefaultRequestHeaders.Authorization = new("bearer", token);

            return new(new(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType")));
        }

        public void NotifyUserAuthentication(string token)
        {
            ClaimsPrincipal authenticatedUser = new(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(anonymus));
        }
    }
}
