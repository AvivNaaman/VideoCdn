using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VideoCdn.Web.Server.Data;
using VideoCdn.Web.Server.Models;
using VideoCdn.Web.Server.Options;

namespace VideoCdn.Web.Server
{
    public class StartupSetup
    {
        private readonly ILogger<StartupSetup> logger;
        private readonly VideoServerOptions options;
        private readonly VideoCdnDbContext dbContext;
        private readonly UserManager<VideoCdnUser> userManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;
        private readonly AdminOptions adminOptions;

        public StartupSetup(ILogger<StartupSetup> logger, IOptions<VideoServerOptions> options, VideoCdnDbContext dbContext,
            UserManager<VideoCdnUser> userManager, RoleManager<IdentityRole<int>> roleManager, IOptions<AdminOptions> adminOptions)
        {
            this.logger = logger;
            this.options = options.Value;
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.adminOptions = adminOptions.Value;
        }

        public async Task Run()
        {
            if (!Directory.Exists(options.TempFilePath))
            {
                logger.LogInformation("Path '{0}' for temp data does not exist. Creating it.", options.TempFilePath);
                Directory.CreateDirectory(options.TempFilePath);
            }
            if (!Directory.Exists(options.DataPath))
            {
                logger.LogInformation("Path '{0}' for video data does not exist. Creating it.", options.DataPath);
                Directory.CreateDirectory(options.DataPath);
            }

            // Migrate DB To latest version
            var migrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (migrations.Any())
            {
                logger.LogInformation("There are {0} unapplied migrations. Applying now...", migrations.Count());
                await dbContext.Database.MigrateAsync();
            }

            // Add roles, ensure super admin existence & etc.
            var sa = await userManager.FindByNameAsync(adminOptions.UserName);
            if (sa is null)
            {
                logger.LogInformation("Super admin {0} ({1}) does not exist. Creating.", adminOptions.UserName, adminOptions.Email);
                sa = new() { UserName = adminOptions.UserName, Email = adminOptions.Email };
                await userManager.CreateAsync(sa, adminOptions.Password);
            }
            else if (adminOptions.ForceSetPassword)
            {
                logger.LogInformation("Super admin is forced to reset password.");
                await userManager.ResetPasswordAsync(sa,
                    await userManager.GeneratePasswordResetTokenAsync(sa),
                    adminOptions.Password);
            }

            // add to role (create if role does not exist)
            var role = await roleManager.FindByNameAsync("Admin");
            bool roleExistedBefore = role is not null;
            if (role is null)
            {
                role = new IdentityRole<int>("Admin");
                logger.LogInformation("The Admin role does not exist. Creating.");
                await roleManager.CreateAsync(role);
            }

            // add user if not in role (check if role wasn't just created
            bool isInRole = false;
            if (roleExistedBefore)
            {
                isInRole = await userManager.IsInRoleAsync(sa, "Admin");
            }

            if (!isInRole)
            {
                logger.LogInformation("Adding the new Super Admin to the Admin role.");
                await userManager.AddToRoleAsync(sa, "Admin");
            }
        }
    }
}
