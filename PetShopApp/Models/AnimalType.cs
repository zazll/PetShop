using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class AnimalType
{
    [Key]
    public int AnimalTypeID { get; set; }

    [Required]
    [StringLength(50)]
    public string AnimalTypeName { get; set; } = null!;
    
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
