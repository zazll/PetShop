using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

public class Review
{
    [Key]
    public int ReviewID { get; set; }

    public int ProductID { get; set; }
    [ForeignKey("ProductID")]
    public virtual Product Product { get; set; } = null!;

    public int UserID { get; set; }
    [ForeignKey("UserID")]
    public virtual AppUser User { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewDate { get; set; } = DateTime.Now;
}
