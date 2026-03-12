namespace InventoryMVC.Domain.Entities;

public class Vendor : Entity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
