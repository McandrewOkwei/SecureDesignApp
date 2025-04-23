using FinalProject.Data;
using FinalProject.Services;
using FinalProject.Services.Util;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text;

namespace FinalProject
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = args,
                // This disables the default URL configuration 
                // so only your Kestrel configuration will be used
                ApplicationName = typeof(Program).Assembly.FullName,
                ContentRootPath = Directory.GetCurrentDirectory()
            });
                


            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
               
                // In Docker, use the container certificate path
                builder.WebHost.ConfigureKestrel(options =>
                {
                options.Listen(IPAddress.Any, 80);

                // Configure HTTPS with explicit certificate in container
                options.Listen(IPAddress.Any, 443, listenOptions =>
                {
                    var certPath = "/app/Cert/drewslab.selfip.com.pfx";
                    var certPassword = "1q2w3e4r";


                    if (File.Exists(certPath))
                    {
                        try
                        {
                            listenOptions.UseHttps(certPath, certPassword);
                            Console.WriteLine($"Using certificate from: {certPath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading certificate: {ex.Message}");
                            // Fall back to development certificate
                            listenOptions.UseHttps();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Certificate file not found: {certPath}");
                        // Fall back to development certificate
                        listenOptions.UseHttps();
                    }
                });
            });
        }
            else
            {
                // For local development, use development certificates
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 80);
                    options.Listen(IPAddress.Any, 443, listenOptions =>
                    {
                        listenOptions.UseHttps();
                    });
                });
            }
            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 42)), // MySQL version
                mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);
            // Your existing services
            builder.Services.AddScoped<FinalProject.Services.AES.Encryption>();
            builder.Services.AddScoped<FinalProject.Services.AES.Master>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.Cookie.Name = "AuthToken";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
            });
            builder.Services.AddRazorPages();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
            {
                app.UseHttpsRedirection();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
