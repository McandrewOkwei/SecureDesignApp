using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages.Account
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }
        public IActionResult OnPost()
        {
            // Validate the input
            if (!ModelState.IsValid)
            {
                return Page();
            }
            return RedirectToPage("/Dashboard");
        }
        }
}
