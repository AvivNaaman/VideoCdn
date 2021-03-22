using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;
namespace VideoCdn.Web.Client.Auth
{
    public class JwtParser
    {

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            var jsonBytes = ParseBase64WithoutPadding(payload);

            var kvp = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            ExtractRolesFromJwt(claims, kvp);

            claims.AddRange(kvp.Select(k => new Claim(k.Key, k.Value.ToString())));

            return claims;
        }


        private static void ExtractRolesFromJwt(List<Claim> claims, Dictionary<string, object> keyValuePairs)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);
            if (roles is not null)
            {
                var parsedRoles = roles.ToString()
                    .Trim().TrimStart('[').TrimEnd(']').Split(',');
                if (parsedRoles.Length > 1)
                {
                    foreach (var role in parsedRoles)
                    {
                        claims.Add(new(ClaimTypes.Role, role.Trim('"')));
                    }
                }
                else
                {
                    claims.Add(new(ClaimTypes.Role, parsedRoles[0]));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
            }
        }



        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4) // Pad
            {
                case 2:
                    base64 += "==";
                    break;
                case 3:
                    base64 += "=";
                    break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}
