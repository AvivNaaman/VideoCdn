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
            EnsureOptionsValid();
            await EnsureDbValid();
            await EnsureAdminWithRights();
        }

        private void EnsureOptionsValid()
        {
            try
            {
                if (!Directory.Exists(options.TempFilePath))
                {
                    logger.LogInformation("Path '{0}' for temp data does not exist. Creating it.", options.TempFilePath);
                    Directory.CreateDirectory(options.TempFilePath);
                }
            }
            catch (IOException ioe)
            {
                logger.LogError("An exception was thrown while trying to access the temp folder {0}: {1}", options.TempFilePath, ioe);
            }

            try
            {
                if (!Directory.Exists(options.DataPath))
                {
                    logger.LogInformation("Path '{0}' for video data does not exist. Creating it.", options.DataPath);
                    Directory.CreateDirectory(options.DataPath);
                }
            }
            catch (IOException ioe)
            {
                logger.LogError("An exception was thrown while trying to access the data folder {0}: {1}", options.DataPath, ioe);
            }

            // Check FFMpeg thread/process limitation and notify if values are not right
            if (options.MaxRunningProcesses <= 0)
            {
                logger.LogWarning("Max running encoding processes is set to {0}, Which is invalid." +
                    " If you'd like to disable the encoding for the current instance," +
                    " Please turn encoding off for all resolutions via the portal," +
                    "under admin/settings.", options.MaxRunningProcesses);
            }
            if (options.ThreadsPerStream < 0)
            {
                logger.LogWarning("Threads per process is set to {0}, Which is invalid." +
                    "The value should 0 (which means - no limit) or any other integer (which will set the limit explicitly)" +
                    " If you'd like to disable the encoding for the current instance," +
                    " Please turn encoding off for all resolutions via the portal," +
                    "under admin/settings.", options.ThreadsPerStream);
            }
        }

        private async Task EnsureDbValid()
        {
            try
            {
                // Migrate DB To latest version
                var migrations = await dbContext.Database.GetPendingMigrationsAsync();
                if (migrations.Any())
                {
                    logger.LogInformation("There are {0} unapplied migrations. Applying now...", migrations.Count());
                    await dbContext.Database.MigrateAsync();
                }
            }
            catch (Exception e)
            {
                logger.LogError("An exception was thrown while migrating the database to the latest version: {0}", e);
            }
        }

        private async Task EnsureAdminWithRights()
        {
            try
            {
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
            catch (Exception ex)
            {
                logger.LogError("An exception was thrown while ensurign that the admin user exists," +
                    " with correct password and in the Admin role: {0}", ex);
            }
        }
    }
}
