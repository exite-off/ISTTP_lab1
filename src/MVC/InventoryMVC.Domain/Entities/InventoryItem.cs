namespace InventoryMVC.Domain.Entities;

// Core aggregate — tracks ownership history via InventoryLog
public class InventoryItem : Entity, IAggregateRoot
{
    public int InventoryNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public decimal Price { get; set; }

    // e.g. Active, WrittenOff, UnderRepair
    public string Status { get; set; } = string.Empty;

    public int ResponsiblePersonId { get; set; }
    public ResponsiblePerson? ResponsiblePerson { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<InventoryLog> Logs { get; set; } = new List<InventoryLog>();
}
