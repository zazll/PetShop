using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopApp.Models;

public class AppUser
{
    [Key]
    public int UserID { get; set; }

    [Required]
    [StringLength(100)]
    public string UserSurname { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    public string? UserPatronymic { get; set; }

    [Required]
    public string UserLogin { get; set; } = null!;

    [Required]
    public string UserPassword { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }

    public int RoleID { get; set; }
    [ForeignKey("RoleID")]
    public virtual Role Role { get; set; } = null!;
}