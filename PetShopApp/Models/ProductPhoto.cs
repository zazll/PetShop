using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

[Table("ProductPhotoTable")]
public class ProductPhoto
{
    [Key]
    public int PhotoID { get; set; }

    public int ProductID { get; set; }
    [ForeignKey("ProductID")]
    public virtual Product Product { get; set; } = null!;

    [Required]
    public string PhotoPath { get; set; } = null!;

    public bool IsMain { get; set; }
}
