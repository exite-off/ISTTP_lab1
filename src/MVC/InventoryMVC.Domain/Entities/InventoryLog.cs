namespace InventoryMVC.Domain.Entities;

// Records every change made to an inventory item
public class InventoryLog : Entity
{
    public DateTime ActionDate { get; set; }

    // e.g. Transfer, StatusChange, Edit
    public string ActionType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int InventoryItemId { get; set; }
    public InventoryItem? InventoryItem { get; set; }

    // Null when the responsible person didn't change
    public int? OldResponsiblePersonId { get; set; }
    public ResponsiblePerson? OldResponsiblePerson { get; set; }

    public int? NewResponsiblePersonId { get; set; }
    public ResponsiblePerson? NewResponsiblePerson { get; set; }
}
