using System.ComponentModel.DataAnnotations;

namespace InventoryMVC.Domain.Entities;

// Core aggregate — tracks ownership history via InventoryLog
public class InventoryItem : Entity, IAggregateRoot
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Inventory number must be a positive integer.")]
    [Display(Name = "Inventory #")]
    public int InventoryNumber { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Entry Date")]
    [DataType(DataType.Date)]
    public DateTime EntryDate { get; set; }

    [Display(Name = "Warranty Ends")]
    [DataType(DataType.Date)]
    public DateTime? WarrantyEndDate { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    [Required]
    [StringLength(10)]
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "UAH";

    // e.g. Active, WrittenOff, UnderRepair
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Responsible Person")]
    public int ResponsiblePersonId { get; set; }
    public ResponsiblePerson? ResponsiblePerson { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    [Display(Name = "Room")]
    public int RoomId { get; set; }
    public Room? Room { get; set; }

    [Required]
    [Display(Name = "Vendor")]
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public ICollection<InventoryLog> Logs { get; set; } = new List<InventoryLog>();
}
