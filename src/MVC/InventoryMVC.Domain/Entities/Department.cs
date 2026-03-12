namespace InventoryMVC.Domain.Entities;

// Owns rooms and responsible persons
public class Department : Entity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public ICollection<ResponsiblePerson> ResponsiblePersons { get; set; } = new List<ResponsiblePerson>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
