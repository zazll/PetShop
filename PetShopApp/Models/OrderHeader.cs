using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

public class OrderHeader
{
    [Key]
    public int OrderID { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime OrderDeliveryDate { get; set; }

    public int OrderPickupCode { get; set; }

    public int OrderStatusID { get; set; }
    [ForeignKey("OrderStatusID")]
    public virtual OrderStatus OrderStatus { get; set; } = null!;

    public int PickupPointID { get; set; }
    [ForeignKey("PickupPointID")]
    public virtual PickupPoint PickupPoint { get; set; } = null!;

    public int? UserID { get; set; }
    [ForeignKey("UserID")]
    public virtual AppUser? User { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
