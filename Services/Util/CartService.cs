using FinalProject.Data;
using FinalProject.Models;
using FinalProject.Services;
using FinalProject.Services.AES;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FinalProject.Services.Util
{
    public class CartService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;
        private readonly Encryption _encryption;
        private readonly Master _master;

        public CartService(ApplicationDbContext context, UserService userService, Encryption encryption, Master master)
        {
            _context = context;
            _userService = userService;
            _encryption = encryption;
            _master = master;
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string username)
        {
            try
            {
                // Get user key for decryption
                var userKey = await _userService.GetUserKeyAsync(username);

                // Get cart items
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Username == username);

                if (cart == null || cart.Items == null)
                    return new List<CartItem>();

                // Decrypt the data as needed
                foreach (var item in cart.Items)
                {
                    try
                    {
                        if (item.IV != null && item.IV.Length > 0 && !string.IsNullOrEmpty(item.EncryptedData))
                        {
                            // Decrypt the encrypted JSON data
                            var encryptedBytes = Convert.FromBase64String(item.EncryptedData);
                            var decryptedBytes = _encryption.DecryptData(encryptedBytes, userKey, item.IV);
                            var jsonData = Encoding.UTF8.GetString(decryptedBytes);

                            // Deserialize JSON to extract properties
                            using (var jsonDoc = JsonDocument.Parse(jsonData))
                            {
                                var root = jsonDoc.RootElement;

                                // Extract the properties
                                if (root.TryGetProperty("ProductName", out var productNameElement))
                                    item.ProductName = productNameElement.GetString() ?? string.Empty;

                                if (root.TryGetProperty("Quantity", out var quantityElement))
                                    item.Quantity = quantityElement.GetInt32();

                                if (root.TryGetProperty("Price", out var priceElement))
                                    item.Price = priceElement.GetDecimal();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error decrypting cart item {item.Id}: {ex.Message}");
                    }
                }

                return cart.Items.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart items: {ex.Message}");
                return new List<CartItem>();
            }
        }

        public async Task AddToCartAsync(string username, string productName, int quantity = 1, decimal price = 0)
        {
            try
            {
                Console.WriteLine($"Adding to cart: {username}, {productName}, {quantity}, {price}");

                // Get user key for encryption
                var userKey = await _userService.GetUserKeyAsync(username);

                // Get or create cart
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.Username == username);

                if (cart == null)
                {
                    cart = new Cart { Username = username };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Generate IV for encryption
                using var aes = System.Security.Cryptography.Aes.Create();
                aes.GenerateIV();
                var iv = aes.IV;

                // Create data object for serialization
                var itemData = new
                {
                    ProductName = productName,
                    Quantity = quantity,
                    Price = price
                };

                // Serialize to JSON
                var json = JsonSerializer.Serialize(itemData);
                Console.WriteLine($"Serialized JSON: {json}");

                // Encrypt the JSON data
                var bytes = Encoding.UTF8.GetBytes(json);
                var encryptedData = _encryption.EncryptData(bytes, userKey, iv);
                var encryptedBase64 = Convert.ToBase64String(encryptedData);

                // Create cart item entity
                var item = new CartItem
                {
                    CartId = cart.Id,
                    EncryptedData = encryptedBase64,
                    IV = iv,
                };

                // Add to database and save changes
                _context.CartItems.Add(item);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Successfully added cart item with ID: {item.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR adding to cart: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to inform the caller
            }
        }

        public async Task RemoveFromCartAsync(string username, int itemId)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Username == username);

                if (cart != null)
                {
                    var item = cart.Items.FirstOrDefault(i => i.Id == itemId);
                    if (item != null)
                    {
                        _context.CartItems.Remove(item);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Successfully removed cart item with ID: {itemId}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR removing from cart: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCartAsync(string username)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.Username == username);

                if (cart != null && cart.Items.Any())
                {
                    _context.CartItems.RemoveRange(cart.Items);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Successfully cleared cart for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR clearing cart: {ex.Message}");
                throw;
            }
        }

        private bool IsEncrypted(string data)
        {
            // Basic check to see if a string looks like it might be encrypted data in Base64
            if (string.IsNullOrEmpty(data)) return false;

            try
            {
                var bytes = Convert.FromBase64String(data);
                return bytes.Length > 16; // Encrypted data typically has some minimum length
            }
            catch
            {
                return false;
            }
        }
    }
}
