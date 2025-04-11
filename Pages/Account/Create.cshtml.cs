using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FinalProject.Models;
using System.ComponentModel.DataAnnotations;
using FinalProject.Data;
namespace FinalProject.Pages.Account
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;// Insert ApplicationDbContext here to access the database.

        //Required Credential bind here for 2 way data binding between the UI and the backend model.
        [BindProperty]

        //Encapsulate all user data inside a Credential Object
        public Credential Credential { get; set; } = new Credential();

        public bool IsSubmitted { get; set; } = false; // Assume a fresh load of create account the user has not submitted a form yet (don't show text danger until after submission).


        //Constructor gives the CreateModel access to the database context.
        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {

        }

        //When a user hits submit, they will be granted an aes key, and credentials will be saved to the database encrypted


        public IActionResult OnPost()
        {
            IsSubmitted = true;
            // Validate the input
            if (!ModelState.IsValid)
            {
                return Page();
            }
            // Create a new user object
            var newUser = new User
            {
                Username = Credential.Username,
                Password = Credential.Password,
                Email = Credential.Email
            };
            // Save the user to the database (pseudo code)
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return RedirectToPage("/Index");
        }

    }
    public class Credential
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
            if(!Email.Contains("@") || !Email.Contains(".") || string.IsNullOrWhiteSpace(Email))
            {
                return false;
            }
            return true;
        }
    }
}
