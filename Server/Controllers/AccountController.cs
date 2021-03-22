using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VideoCdn.Web.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using VideoCdn.Web.Server.Options;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Shared;
using System.Text;
using System.Linq;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<VideoCdnUser> _userManager;
        private readonly JwtOptions _jwtOptions;

        public AccountController(UserManager<VideoCdnUser> userManager, IOptions<JwtOptions> jwtOptions)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        [HttpGet]
        [Route("Login")]
        public async Task<ActionResult<AuthenticatedUser>> Login(string userName, string password)
        {
            if ((userName is null or "") || (password is null or "")) return BadRequest();
            var u = await _userManager.FindByNameAsync(userName);
            if (u is null) return BadRequest();

            if (await _userManager.CheckPasswordAsync(u, password))
            {
                var roles = await _userManager.GetRolesAsync(u);
                return new AuthenticatedUser
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    Token = GenerateJwtToken(u, roles)
                };
            }
            else return BadRequest();
        }

        private string GenerateJwtToken(VideoCdnUser u, IList<string> Roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new(ClaimTypes.Name, u.UserName),
                new(ClaimTypes.Email, u.Email),
            };
            Roles.ToList().ForEach(r => claims.Add(new(ClaimTypes.Role, r))); // add roles
            var descriptor = new JwtSecurityToken(null, null, claims,
            expires: DateTime.Now.AddYears(1),
            signingCredentials: new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)), SecurityAlgorithms.HmacSha256Signature));

            return new JwtSecurityTokenHandler().WriteToken(descriptor);
        }

        [HttpGet]
        [Route("CreateAccountFast")]
        public async Task<ActionResult<IdentityResult>> CreateAccountFast(string userName, string password, string email)
        {
            return Ok(await _userManager.CreateAsync(new() { UserName = userName, Email = email }, password));
        }
    }
}
