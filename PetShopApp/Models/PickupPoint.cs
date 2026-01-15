using System.ComponentModel.DataAnnotations;

namespace PetShopApp.Models;

public class PickupPoint
{
    [Key]
    public int PickupPointID { get; set; }

    [Required]
    [StringLength(10)]
    public string PostalCode { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Street { get; set; } = null!;

    [StringLength(20)]
    public string? HouseNumber { get; set; }
}
