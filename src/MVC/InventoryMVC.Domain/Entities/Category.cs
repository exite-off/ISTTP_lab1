using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

public class Category : Entity, IAggregateRoot
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
