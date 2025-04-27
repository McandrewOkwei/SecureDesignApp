using BCrypt.Net;
using FinalProject.Data;
using FinalProject.Services;
using FinalProject.Services.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace FinalProject.Pages.Account
{
    public class LoginModel : PageModel
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Ambiguity between Shared namespace Credential class defintion, so we just take the lazy route and rename the class between create account and login
        [BindProperty]
        public LoginCredential Credential { get; set; } = new LoginCredential();
        public LoginModel(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;

        }


        public void OnGet()
        {

        }

        //Http post method for login is modified to use async await bcuz we async fetch their info from database.
        public async Task<IActionResult> OnPostAsync()
        {
            // Validate the input
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Credential.Username = Credential.Username.Trim();
            Credential.Password = Credential.Password.Trim();
            try
            {
                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == Credential.Username.ToLower());

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found. Please check your username.");
                    return Page();
                }

                // Verify the password
                if (!BCrypt.Net.BCrypt.Verify(Credential.Password, user.Password))
                {
                    ModelState.AddModelError(string.Empty, "Invalid password. Please try again.");
                    return Page();
                }

                var claims = new List<Claim>
                    {//dotnet identity claims are keyvalue pairs that represent ways to track info of an identity.
                    //One person is called a principal in this case. A principal can have many identities and an identity can have many claims or attributes.
                        new Claim(ClaimTypes.Name, user.Username),
                        
                    };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    // Configure authentication properties
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                // Sign in the user using cookies
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirect to Dashboard after successful authentication
                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                //Push The exception to the model and return login page if auth didnt work.
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return Page();
            }
        }


    }
    public class LoginCredential
    {

        [Display(Name = "User Name")]
        [Required]
        [BindProperty]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [BindProperty]
        public string Password { get; set; }

        [Display(Name = "Email Address")]
        public string? Email { get; set; }
    }
}
