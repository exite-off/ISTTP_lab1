using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

public class Room : Entity
{
    [Required(ErrorMessage = "Room number is required")]
    [Display(Name = "Room Number")]
    public int Number { get; set; }

    [Required(ErrorMessage = "Floor is required")]
    public int Floor { get; set; }

    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
}
