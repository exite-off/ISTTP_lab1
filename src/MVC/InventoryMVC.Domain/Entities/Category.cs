namespace InventoryMVC.Domain.Entities;

public class Category : Entity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
