using Serilog;
using DataService;
using AuthService;
using System.Text;
using UserService;
using RolesService;
using ModelService;
using EmailService;
using LogginService;
using CookieService;
using FiltersService;
using BackendService;
using CountryService;
using ActivityService;
using DashboardService;
using FunctionalService;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Angular_with_Dotnet.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Antiforgery;

namespace Angular_with_Dotnet;

public class Program
{
    [Obsolete]
    public static void Main(string[] args)
    {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // ConfigurationManager configuration = builder.Configuration;

        builder.Services.AddSpaStaticFiles(configuration => configuration.RootPath = "ClientApp/dist");

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      DB Connection Options                                         */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString: builder.Configuration.GetConnectionString("CmsCoreNg_DEV"),
            x => x.MigrationsAssembly("Angular_with_Dotnet")));

        builder.Services.AddDbContext<DataProtectionKeysContext>(options =>
            options.UseSqlServer(connectionString: builder.Configuration.GetConnectionString("DataProtectionKeysContext"),
            x => x.MigrationsAssembly("Angular_with_Dotnet")));

        /*----------------------------------------------------------------------------------------------------*/
        /*                                          Functional SERVICE                                        */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IFunctionalSvc, FunctionalSvc>();
        builder.Services.Configure<AdminUserOptions>(builder.Configuration.GetSection("AdminUserOptions"));
        builder.Services.Configure<AppUserOptions>(builder.Configuration.GetSection("AppUserOptions"));

        /*----------------------------------------------------------------------------------------------------*/
        /*                                           Writable SERVICE                                         */
        /*----------------------------------------------------------------------------------------------------*/
        var siteWideSettingsSection = builder.Configuration.GetSection("SiteWideSettings");
        var smtpOptionsSection = builder.Configuration.GetSection("SmtpOptions");
        var sendGridOptionsSection = builder.Configuration.GetSection("SendGridOptions");
        builder.Services.ConfigureWritable<SendGridOptions>(sendGridOptionsSection, "appsettings.json");
        builder.Services.ConfigureWritable<SmtpOptions>(smtpOptionsSection, "appsettings.json");
        builder.Services.ConfigureWritable<SiteWideSettings>(siteWideSettingsSection, "appsettings.json");

        /*----------------------------------------------------------------------------------------------------*/
        /*                                         DEFAULT IDENTITY OPTIONS                                   */
        /*----------------------------------------------------------------------------------------------------*/
        var identityDefaultOptionsConfiguration = builder.Configuration.GetSection("IdentityDefaultOptions");
        builder.Services.Configure<IdentityDefaultOptions>(identityDefaultOptionsConfiguration);
        var identityDefaultOptions = identityDefaultOptionsConfiguration.Get<IdentityDefaultOptions>();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password Settings
            options.Password.RequireDigit = identityDefaultOptions.PasswordRequireDigit;
            options.Password.RequiredLength = identityDefaultOptions.PasswordRequiredLength;
            options.Password.RequireNonAlphanumeric = identityDefaultOptions.PasswordRequireNonAlphanumeric;
            options.Password.RequireUppercase = identityDefaultOptions.PasswordRequireUppercase;
            options.Password.RequireLowercase = identityDefaultOptions.PasswordRequireLowercase;
            options.Password.RequiredUniqueChars = identityDefaultOptions.PasswordRequireUniqueChars;

            // Lockout Settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
            options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
            options.Lockout.AllowedForNewUsers = identityDefaultOptions.LockoutAllowedForNewUsers;

            // Users Setting
            options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

            // Email Confirmation require
            options.SignIn.RequireConfirmedEmail = identityDefaultOptions.SignInRequireConfirmedEmail;

        }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      AppSettings SERVICE                                           */
        /*----------------------------------------------------------------------------------------------------*/
        var appSettingsSection = builder.Configuration.GetSection("AppSettings");
        builder.Services.Configure<AppSettings>(appSettingsSection);

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      JWT Authentication SERVICE                                    */
        /*----------------------------------------------------------------------------------------------------*/

        var appSettings = appSettingsSection.Get<AppSettings>();
        byte[] key = Encoding.ASCII.GetBytes(s: appSettings.Secret);

        builder.Services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;

        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = appSettings.ValidateIssuerSigninKey,
                ValidateIssuer = appSettings.ValidateIssuer,
                ValidateAudience = appSettings.ValidateAudience,
                ValidIssuer = appSettings.Site,
                ValidAudience = appSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };
        });


        /*----------------------------------------------------------------------------------------------------*/
        /*                                     DATA PROTECTION SERVICE                                        */
        /*----------------------------------------------------------------------------------------------------*/
        var dataProtectionSection = builder.Configuration.GetSection("DataProtectionKeys");
        builder.Services.Configure<DataProtectionKeys>(dataProtectionSection);
        builder.Services.AddDataProtection().PersistKeysToDbContext<DataProtectionKeysContext>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                           Email SERVICE                                            */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGridOptions"));
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("SmtpOptions"));
        builder.Services.AddTransient<IEmailSvc, EmailSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                           Auth SERVICE                                             */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IAuthSvc, AuthSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                           Admin SERVICE                                            */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IAdminSvc, AdminSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                       Activity SERVICE                                             */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IActivitySvc, ActivityService.CountrySvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      Cookie Helper SERVICE                                         */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddTransient<CookieOptions>();
        builder.Services.AddTransient<ICookieSvc, CookieSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                       Country SERVICE                                             */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<ICountrySvc, CountriesSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                       Dashboard SERVICE                                            */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IDashboardSvc, DashboardSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                       User Helper SERVICE                                             */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IUserSvc, UserSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                       User Roles SERVICE                                           */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddTransient<IRoleSvc, RoleSvc>();

        /*----------------------------------------------------------------------------------------------------*/
        /*                                  AuthenticationSchemes SERVICE                                     */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddAuthentication("Administrator").AddScheme<AdminAuthenticationOptions, AdminAuthenticationHandler>("Admin", null);
        builder.Services.AddAuthentication("User").AddScheme<UserAuthenticationOptions, UserAuthenticationHandler>("User", null);

        /*----------------------------------------------------------------------------------------------------*/
        /*                                          Enable CORS                                               */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("EnableCORS", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().Build();
            });
        });

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      Enable API Versioning                                         */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        /*----------------------------------------------------------------------------------------------------*/
        /*                                      Razor Pages Runtime SERVICE                                   */
        /*       Add Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation NuGet Package to the Projects          */
        /*----------------------------------------------------------------------------------------------------*/
        builder.Services.AddMvc().AddControllersAsServices().AddRazorRuntimeCompilation()
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

        /*-------------------------------------------------------------------------------------------------------*/
        /*                              Anti Forgery Token Validation SERVICE                                    */
        /*      We use option pattern to configure Antiforgery feature through AntiForgeryOptions class          */
        /*The HeaderName Property used to specify name of header through which antiforgery token will be accepted*/
        /*-------------------------------------------------------------------------------------------------------*/
        builder.Services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-XSRF-TOKEN";
        });

        builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Angular_with_Dotnet_App")
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("CurrentManagedThreadID", Environment.CurrentManagedThreadId)
            .Enrich.WithProperty("OSVersion", Environment.OSVersion)
            .Enrich.WithProperty("Version", Environment.Version)
            .Enrich.WithProperty("Username", Environment.UserName)
            .Enrich.WithProperty("ProcessID", Process.GetCurrentProcess().Id)
            .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
            .WriteTo.Console(theme: CustomConsoleTheme.VisualStudioMacLight)
            .WriteTo.File(formatter: new CustomTextFormatted(),
                path: Path.Combine(hostingContext.HostingEnvironment.ContentRootPath +
                $"{Path.DirectorySeparatorChar} " +
                $"Logs {Path.DirectorySeparatorChar}",
                $"load_error_{DateTime.Now:yyyyMMdd}.text"))
            .ReadFrom.Configuration(hostingContext.Configuration)
        );

        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                DataProtectionKeysContext dpcontext = services.GetRequiredService<DataProtectionKeysContext>();
                ApplicationDbContext context = services.GetRequiredService<ApplicationDbContext>();
                IFunctionalSvc functionSvc = services.GetRequiredService<IFunctionalSvc>();
                ICountrySvc countrySvc = services.GetRequiredService<ICountrySvc>();

                DbContextInitializer.Initialize(dpcontext, context, functionSvc, countrySvc).Wait();
            }
            catch (DivideByZeroException ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
            }
        }

        var antiforgery = app.Services.GetRequiredService<IAntiforgery>();


        app.Use((context, nextDelegate) =>
        {
            var path = context.Request.Path.Value!.ToLower();
            string[] directUrls = { "/admin", "/store", "/cart", "/checkout", "/login" };
            if (path.StartsWith("/swagger") || path.StartsWith("/api") || string.Equals("/", path) || directUrls.Any(url => path.StartsWith(url)))
            {
                var tokens = antiforgery.GetAndStoreTokens(context);
                context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions()
                {
                    HttpOnly = false,
                    Secure = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Strict

                });
            }
            return nextDelegate(context);
        });


        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();


        app.MapControllerRoute(
            name: "default",
            pattern: "{controller}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


        app.MapFallbackToFile("index.html");

        app.Run();

    }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}

