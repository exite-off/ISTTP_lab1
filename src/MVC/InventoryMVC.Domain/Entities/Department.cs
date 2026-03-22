using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

// Owns rooms and responsible persons
public class Department : Entity, IAggregateRoot
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required")]
    [StringLength(20)]
    [Display(Name = "Phone")]
    public string Phone { get; set; } = string.Empty;

    public ICollection<ResponsiblePerson> ResponsiblePersons { get; set; } = new List<ResponsiblePerson>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
