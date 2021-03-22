using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Shared;

namespace VideoCdn.Web.Server.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    [Authorize(/*Roles = "Admin"*/)]
    public class AdminController : ControllerBase
    {
        private readonly ISettingsService<VideoCdnSettings> _settingsService;
        private readonly UserManager<VideoCdnUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(ISettingsService<VideoCdnSettings> settingsService, UserManager<VideoCdnUser> userManager
            , RoleManager<IdentityRole<int>> roleManager)
        {
            _settingsService = settingsService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("[action]")]
        public ActionResult<VideoCdnSettings> Settings()
        {
            return _settingsService.Settings;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ActionResult> Settings(VideoCdnSettings settings)
        {
            _settingsService.Settings = settings;
            await _settingsService.Save();
            return Ok();
        }

        const int MaxPageSize = 20;

        [HttpGet]
        [Route("Users/NumberOfPages")]
        public async Task<ActionResult<int>> UsersNumberOfPages()
        {
            double maxPageSizeAsDouble = MaxPageSize;
            return (int)Math.Ceiling((await _userManager.Users.CountAsync()) / maxPageSizeAsDouble);
        }

        // TODO: Make it more efficient!
        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<UserModel>>> Users([FromQuery] UserSearchModel model)
        {
            var users = _userManager.Users;

            if (model.Text is not null or "")
            {
                users = users.Where(u => u.Email.Contains(model.Text) ||
                    u.UserName.Contains(model.Text));
            }

            if (model.Page > 1)
            {
                users = users.Skip((model.Page - 1) * MaxPageSize);
            }
            // pagination
            var usersList = await users
                .Take(MaxPageSize)
                .ToListAsync();

            // query everything and return it
            var final = new List<UserModel>();
            foreach (var user in usersList)
            {
                final.Add(new()
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList()
                });
            }
            return final;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<List<string>>> Roles()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<UserModel>> GetUserById(int id)
        {
            var u = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);
            var roles = await _userManager.GetRolesAsync(u);
            return new UserModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Roles = roles.ToList()
            };
        }


        [HttpGet]
        [Route("[action]")]
        public async Task<ActionResult<UserModel>> UpdateUser(UserModel updatedUser)
        {
            var u = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == updatedUser.Id);
            if (u is null) return NotFound();

            u.Email = updatedUser.Email;
            u.UserName = updatedUser.UserName;
            await _userManager.UpdateAsync(u);
            return Ok();
        }
    }
}
