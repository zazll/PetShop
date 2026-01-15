using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class Manufacturer
{
    [Key]
    public int ManufacturerID { get; set; }

    [Required]
    [StringLength(100)]
    public string ManufacturerName { get; set; } = null!;
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
