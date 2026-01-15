using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class OrderStatus
{
    [Key]
    public int OrderStatusID { get; set; }

    [Required]
    [StringLength(50)]
    public string StatusName { get; set; } = null!;
}
