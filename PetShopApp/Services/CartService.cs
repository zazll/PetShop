using PetShopApp.Models;

namespace PetShopApp.Services;

public class CartService
{
    private static CartService? _instance;
    public static CartService Instance => _instance ??= new CartService();

    public List<Product> Items { get; private set; } = new();

    private CartService() { }

    public void AddToCart(Product product)
    {
        // Simple implementation: List of products. 
        // For quantity logic, we might need a wrapper class, but for now specific items.
        Items.Add(product);
    }

    public void RemoveFromCart(Product product)
    {
        Items.Remove(product);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public decimal GetTotal()
    {
        return Items.Sum(p => 
            p.ProductDiscountAmount > 0 
            ? p.ProductCost * (1 - p.ProductDiscountAmount.Value / 100m) 
            : p.ProductCost
        );
    }
    
    public int GetCount() => Items.Count;
}
