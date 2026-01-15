using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class Supplier
{
    [Key]
    public int SupplierID { get; set; }

    [Required]
    [StringLength(100)]
    public string SupplierName { get; set; } = null!;

    [StringLength(20)]
    public string? ContactPhone { get; set; }
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
