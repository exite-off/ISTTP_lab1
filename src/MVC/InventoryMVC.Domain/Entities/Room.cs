namespace InventoryMVC.Domain.Entities;

public class Room : Entity
{
    public int Number { get; set; }
    public int Floor { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
