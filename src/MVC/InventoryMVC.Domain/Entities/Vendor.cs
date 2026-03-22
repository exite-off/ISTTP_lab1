using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

public class Vendor : Entity, IAggregateRoot
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    [Required(ErrorMessage = "Contact phone is required")]
    [StringLength(20)]
    [Display(Name = "Contact Phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
