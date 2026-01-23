using PetShopApp.Models;

namespace PetShopApp.Services;

public class CartService
{
    // Nested class to represent an item in the cart with its quantity
    public class CartItem
    {
        public Product Product { get; }
        public int Quantity { get; set; }
        
        public string ProductName => Product.ProductName;
        public decimal ProductCost => Product.ProductCost;
        public byte? ProductDiscountAmount => Product.ProductDiscountAmount;
        public int ProductID => Product.ProductID;

        public CartItem(Product product, int quantity = 1)
        {
            Product = product;
            Quantity = quantity;
        }

        public decimal GetCurrentPrice()
        {
            return Product.ProductDiscountAmount > 0
                ? Product.ProductCost * (1 - Product.ProductDiscountAmount.Value / 100m)
                : Product.ProductCost;
        }
    }

    private static CartService? _instance;
    public static CartService Instance => _instance ??= new CartService();

    public List<CartItem> Items { get; private set; } = new();

    private CartService() { }

    public void AddToCart(Product product)
    {
        var existingItem = Items.FirstOrDefault(item => item.Product.ProductID == product.ProductID);
        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Items.Add(new CartItem(product));
        }
    }

    public void RemoveFromCart(CartItem itemToRemove)
    {
        Items.Remove(itemToRemove);
    }
    
    public void IncreaseQuantity(CartItem item)
    {
        item.Quantity++;
    }

    public void DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            Items.Remove(item); // Remove if quantity drops to 0
        }
    }

    public void Clear()
    {
        Items.Clear();
    }

    public decimal GetTotal()
    {
        return Items.Sum(item => item.GetCurrentPrice() * item.Quantity);
    }
    
    public int GetCount() => Items.Sum(item => item.Quantity);
}
