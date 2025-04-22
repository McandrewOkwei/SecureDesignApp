using FinalProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinalProject.Services;
using FinalProject.Services.Util;

namespace FinalProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
	    //Bind port to 5000
	    
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 42)), // Replace with your MySQL version
                            mySqlOptions => mySqlOptions.EnableRetryOnFailure()

                )
            ); 
            
          
            builder.Services.AddScoped<FinalProject.Services.AES.Encryption>(); //Add user encryption handler
            builder.Services.AddScoped<FinalProject.Services.AES.Master>();

            builder.Services.AddScoped<UserService>(); //Add user encryption handler
            builder.Services.AddScoped<CartService>(); //Add user encryption handler
            builder.Services.AddHttpContextAccessor(); //Register httpContext accessor for dependency injection
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                //Configure Json web token use appsettings.json for values.
                //Note* I got frustrated debugging the jwt so its just an option. Default is cookie auth.
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
                // Configure cookie auth
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.Cookie.Name = "AuthToken"; // Match the cookie name you're using
                    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Match your login expiry time
                    options.SlidingExpiration = true; // Expiration window slides for my loyal customers :)
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Cookie security properties always set.
                    options.Cookie.HttpOnly = true; // Cannot access cookie from client side with a script/
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            //Config to add image files, only allow https traffic, use cookies, give access to pages, map pages and run the application.
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }

}
