using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

public class Product
{
    [Key]
    public int ProductID { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductArticleNumber { get; set; } = null!;

    [Required]
    public string ProductName { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string UnitDescription { get; set; } = null!;

    [Column(TypeName = "decimal(19,4)")]
    public decimal ProductCost { get; set; }

    public byte? ProductDiscountAmount { get; set; }

    public int ProductQuantityInStock { get; set; }

    public string? ProductPhoto { get; set; }

    public string? ProductDescription { get; set; }

    public int CategoryID { get; set; }
    [ForeignKey("CategoryID")]
    public virtual ProductCategory Category { get; set; } = null!;

    public int? AnimalTypeID { get; set; }
    [ForeignKey("AnimalTypeID")]
    public virtual AnimalType? AnimalType { get; set; }

    public int ManufacturerID { get; set; }
    [ForeignKey("ManufacturerID")]
    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public int SupplierID { get; set; }
    [ForeignKey("SupplierID")]
    public virtual Supplier Supplier { get; set; } = null!;
}
