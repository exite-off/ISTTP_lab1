using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

// Records every change made to an inventory item
public class InventoryLog : Entity
{
    [Required]
    [Display(Name = "Date")]
    [DataType(DataType.DateTime)]
    public DateTime ActionDate { get; set; }

    // e.g. Transfer, StatusChange, Edit
    [Required]
    [StringLength(50)]
    [Display(Name = "Action")]
    public string ActionType { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Inventory Item")]
    public int InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    // Null when the responsible person didn't change
    [Display(Name = "Old Responsible")]
    public int? OldResponsiblePersonId { get; set; }
    public ResponsiblePerson? OldResponsiblePerson { get; set; }

    [Display(Name = "New Responsible")]
    public int? NewResponsiblePersonId { get; set; }
    public ResponsiblePerson? NewResponsiblePerson { get; set; }
}
