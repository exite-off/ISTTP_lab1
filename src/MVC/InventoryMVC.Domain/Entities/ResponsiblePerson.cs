namespace InventoryMVC.Domain.Entities;

// Can't be deleted while assigned to any inventory item
public class ResponsiblePerson : Entity
{
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
