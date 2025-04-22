using FinalProject.Models;
using FinalProject.Services.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Xml.Linq;

namespace FinalProject.Pages
{
    public class BrowseModel : PageModel
    {
        public List<IceCream> IceCreams { get; set; } = new();
        public List<CartItem> CartItems { get; private set; }
        public decimal CartTotal { get; private set; }

        private readonly CartService _cartService;
        public BrowseModel(CartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<IActionResult> OnPostAsync(string cartItemsJson)
        {
            if (string.IsNullOrEmpty(cartItemsJson))
            {
                return Page();
            }

            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                var user = User.Identity.Name;
                Console.WriteLine($"Received cart JSON: {cartItemsJson}");
                var newCartItems = JsonSerializer.Deserialize<List<CartItemDto>>(cartItemsJson);

                // Redirect back to the same page if the cart's empty
                if (newCartItems == null || newCartItems.Count == 0)
                {
                    return Page();
                }

                // IMPORTANT: First clear the existing cart - this prevents duplicates
                await _cartService.ClearCartAsync(user);

                // Then add all items from the form submission
                foreach (var item in newCartItems)
                {
                    if (item.quantity > 0) // Only add items with positive quantities
                    {
                        await _cartService.AddToCartAsync(user, item.name, item.quantity, item.price);
                        Console.WriteLine($"Added to cart: {item.name}, Qty: {item.quantity}, Price: {item.price}");
                    }
                }

                return RedirectToPage("/Browse");
            }
            catch (Exception x)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request: " + x.Message);
                await OnGetAsync();
                return Page();
            }
        }




        // Data transfer object class for java deserialization (grab data from client pushing buttons and not submitting a regular form)
        public class CartItemDto
        {
            public string name { get; set; }
            public decimal price { get; set; }
            public int quantity { get; set; }
        }

        
            public async Task OnGetAsync()
        {
            // Path to the images folder
            var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "browse");
            var imageFiles = Directory.GetFiles(imagesPath);

            // Populate the IceCreams list
            foreach (var imageFile in imageFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(imageFile);
                IceCreams.Add(new IceCream
                {
                    Label = Label(fileName), // Replace with actual label if needed
                    ImagePath = $"/images/browse/{Path.GetFileName(imageFile)}",
                    Price = GetPrice(fileName)
                });
            }

            // Load cart items for authenticated users
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    CartItems = await _cartService.GetCartItemsAsync(User.Identity.Name);

                    // Calculate the total of all cart items
                    CartTotal = 0;
                    foreach (var item in CartItems)
                    {
                        CartTotal += item.Price *  item.Quantity;
                    }

                    Console.WriteLine($"Found {CartItems.Count} cart items with total: ${CartTotal}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading cart items: {ex.Message}");
                }
            }
        }

        public double GetPrice(string name)
        {
            if (name.Contains("Oreo"))
            {
                return 5000.2;
            }
            else if (name.Contains("StrawBerryBowl"))
            {
                return 6000.3;
            }
            else if (name.Contains("VanillaStraw"))
            {
                return 7000.4;
            }
            else if (name.Contains("FriedIceCream"))
            {
                return 8000.5;
            }
            else
            {
                return 9000.1;
            }

        }
        public string Label(string name)
        {
            if (name.Contains("Oreo"))
            {
                return "Vanilla";
            }
            else if (name.Contains("StrawBerryBowl"))
            {
                return "Strawberry Bowl";
            }
            else if (name.Contains("VanillaStraw"))
            {
                return "Vanilla Strawberry mix";
            }
            else if (name.Contains("FriedIceCream"))
            {
                return "Fried Ice Cream";
            }
            else
            {
                return "Chocolate";
            }
        }
    }
}
