using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

public class OrderProduct
{
    public int OrderID { get; set; }
    [ForeignKey("OrderID")]
    public virtual OrderHeader Order { get; set; } = null!;

    public int ProductID { get; set; }
    [ForeignKey("ProductID")]
    public virtual Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}
