using BCrypt.Net;
using FinalProject.Data;
using FinalProject.Models;
using FinalProject.Services.AES;
using FinalProject.Services.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace FinalProject.Pages.Account
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;// Insert ApplicationDbContext here to access the database.
        private readonly IConfiguration _config;
        private readonly UserService _userService; // Added UserService for balance encryption

        //Required Credential bind here for 2 way data binding between the UI and the backend model.
        [BindProperty]

        //Encapsulate all user data inside a Credential Object
        public CreateCredential Credential { get; set; } = new CreateCredential();

        public bool IsSubmitted { get; set; } = false; // Assume a fresh load of create account the user has not submitted a form yet (don't show text danger until after submission).


        //Constructor gives the CreateModel access to the database context.
        public CreateModel(ApplicationDbContext context, IConfiguration config, UserService userService)
        {
            _context = context;
            _config = config;
            _userService = userService;
        }
        public void OnGet()
        {

        }

        //When a user hits submit, they will be granted an aes key, and credentials will be saved to the database encrypted


        public async Task<IActionResult> OnPostAsync()
        {
            IsSubmitted = true;

            if (!ModelState.IsValid)
            {
                return Page();
            }
            try
            {
                Console.WriteLine("Starting account creation process...");

                //If duplicate user, redirect to create an account
                if (_context.Users.Any(u => u.Username == Credential.Username))
                {
                    ModelState.AddModelError(Credential.Username, "Username already taken");
                    return Page();
                }

                Console.WriteLine("Creating new user object...");
                // Create a new user object
                var newUser = new User
                {
                    Username = Credential.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(Credential.Password),
                    Email = Credential.Email,
                    Balance = 900000
                };
                newUser.EncBalance = _userService.InitializeUserBalanceAsync(newUser);
                Console.WriteLine("Adding user to database...");
                // First add to database to get valid ID
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                Console.WriteLine("User saved to database with ID: " + newUser.Username);

                Console.WriteLine("Initializing user balance...");
                // Then initialize user balance - this will set up the encryption key
                
                Console.WriteLine("User balance initialized");

                Console.WriteLine("Setting up authentication...");
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, newUser.Username),
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                // Sign in the user using cookies
                Console.WriteLine("Signing in user...");
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                Console.WriteLine("Redirecting to Index page...");
                // Use explicit URL generation
                return RedirectToPage("/Dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR creating account: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                ModelState.AddModelError(string.Empty, $"Error creating account: {ex.Message}");
                return Page();
            }
        }
    }
        public class CreateCredential
    {

        [Display(Name = "User Name")]
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }

        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        public bool Validate()
        {
            if (!Email.Contains("@") || !Email.Contains(".") || string.IsNullOrWhiteSpace(Email))
            {
                return false;
            }
            return true;
        }
    }
}
