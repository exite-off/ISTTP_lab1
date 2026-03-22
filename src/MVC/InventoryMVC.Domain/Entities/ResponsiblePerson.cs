using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

// Can't be deleted while assigned to any inventory item
public class ResponsiblePerson : Entity
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
