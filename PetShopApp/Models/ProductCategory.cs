using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class ProductCategory
{
    [Key]
    public int CategoryID { get; set; }

    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
