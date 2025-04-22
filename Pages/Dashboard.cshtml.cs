using FinalProject.Models;
using FinalProject.Services.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FinalProject.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly UserService _userService;
        private readonly CartService _cartService;

        public double UserBalance { get; set; }
        public List<CartItem> CartItems { get; set; }
        public decimal CartTotal { get; set; }

        public DashboardModel(UserService userService, CartService cartService)
        {
            _userService = userService;
            _cartService = cartService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            // Get the current username from claims
            var username = User.Identity.Name;

            // Load user data or perform other authenticated operations
            var user = await _userService.GetUserAsync(username);
            UserBalance = user.Balance;

            // Load cart items
            await LoadCartItems(username);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            var username = User.Identity.Name;

            try
            {
                // Get cart items
                var items = await _cartService.GetCartItemsAsync(username);

                if (items == null || !items.Any())
                {
                    TempData["StatusMessage"] = "Your cart is empty.";
                    return RedirectToPage();
                }

                // Calculate total cost
                decimal totalCost = items.Sum(item => item.Price * item.Quantity);

                // Process transaction (deduct from balance)
                bool transactionSuccess = await _userService.ProcessTransactionAsync(username, (double)totalCost);

                if (transactionSuccess)
                {
                    // Clear cart after successful purchase
                    await _cartService.ClearCartAsync(username);
                    TempData["StatusMessage"] = "Purchase completed successfully!";
                }
                else
                {
                    TempData["StatusMessage"] = "Insufficient funds to complete this purchase.";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadCartItems(string username)
        {
            try
            {
                CartItems = await _cartService.GetCartItemsAsync(username);
                CartTotal = CartItems?.Sum(i => i.Price * i.Quantity) ?? 0;
            }
            catch (Exception)
            {
                CartItems = new List<CartItem>();
                CartTotal = 0;
            }
        }
    }
}
