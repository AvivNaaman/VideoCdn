using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using VideoCdn.Web.Server.Options;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using VideoCdn.Web.Server.Services;
using VideoCdn.Web.Server.Data;
using Microsoft.EntityFrameworkCore;
using VideoCdn.Web.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VideoCdn.Web.Shared;
using VideoCdn.Web.Server.Middlewares;

namespace VideoCdn.Web.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions<VideoServerOptions>()
                .Bind(Configuration.GetSection("VideoCdn"));
            services.AddOptions<JwtOptions>()
                .Bind(Configuration.GetSection("Jwt"));
            services.AddOptions<AdminOptions>()
                .Bind(Configuration.GetSection("Admin"));

            services.AddSettingsFile<VideoCdnSettings>("VideoCdnSettings.json");

            services.AddSingleton<IVideoTokenService, VideoTokenService>();
            services.AddSingleton<IVideoEncodingQueue, VideoEncodingQueue>();
            services.AddSingleton<ChunkedUploadsCollection>();

            services.AddScoped<IVideoEncodingService, VideoEncodingService>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IWatchCounterService, WatchCounterService>();

            services.AddTransient<StartupSetup>();

            services.AddDbContext<VideoCdnDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            services.AddIdentity<VideoCdnUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<VideoCdnDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new()
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Key"))),
                    ClockSkew = TimeSpan.FromMinutes(5),
                };
            });

            services.Configure<FormOptions>(o => {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = int.MaxValue;
                o.MemoryBufferThreshold = int.MaxValue;
            });
            services.AddSwaggerGen();
            services.AddControllersWithViews().AddJsonOptions(json =>
            {
                // because sharing models!
                json.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
            });
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<Startup> logger, IOptions<VideoServerOptions> options, StartupSetup setup)
        {
            setup.Run().Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();

            app.UseMiddleware<VideoMiddleware>();

            try
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(options.Value.DataPath),
                    RequestPath = new PathString(options.Value.VideoServeUrl),
                    ServeUnknownFileTypes = true
                });
            }
            catch
            {
                logger.LogError("Failed to setup static file serve directory for videos.");
                throw;
            }


            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 API");
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
