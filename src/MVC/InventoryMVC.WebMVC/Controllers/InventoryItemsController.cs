using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

public class InventoryItemsController : Controller
{
    private readonly InventoryContext _context;

    private static readonly string[] Statuses = ["Active", "Under Repair", "Written Off", "In Storage"];

    public InventoryItemsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Room).ThenInclude(r => r!.Department)
            .Include(i => i.ResponsiblePerson)
            .ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Vendor)
            .Include(i => i.Room).ThenInclude(r => r!.Department)
            .Include(i => i.ResponsiblePerson).ThenInclude(rp => rp!.Department)
            .Include(i => i.Logs)
                .ThenInclude(l => l.OldResponsiblePerson)
            .Include(i => i.Logs)
                .ThenInclude(l => l.NewResponsiblePerson)
            .FirstOrDefaultAsync(i => i.Id == id);
        return item == null ? NotFound() : View(item);
    }

    public IActionResult Create()
    {
        PopulateDropdowns();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("InventoryNumber,Name,EntryDate,WarrantyEndDate,Price,Status,ResponsiblePersonId,CategoryId,RoomId,VendorId")]
        InventoryItem item)
    {
        if (!ModelState.IsValid) { PopulateDropdowns(); return View(item); }
        _context.Add(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null) return NotFound();
        PopulateDropdowns(item);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,InventoryNumber,Name,EntryDate,WarrantyEndDate,Price,Status,ResponsiblePersonId,CategoryId,RoomId,VendorId")]
        InventoryItem item)
    {
        if (id != item.Id) return NotFound();
        if (!ModelState.IsValid) { PopulateDropdowns(item); return View(item); }

        var existing = await _context.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        if (existing == null) return NotFound();

        // Auto-log when responsible person changes
        if (existing.ResponsiblePersonId != item.ResponsiblePersonId)
        {
            _context.InventoryLogs.Add(new InventoryLog
            {
                ActionDate = DateTime.Now,
                ActionType = "Transfer",
                Description = "Responsible person changed",
                InventoryItemId = item.Id,
                OldResponsiblePersonId = existing.ResponsiblePersonId,
                NewResponsiblePersonId = item.ResponsiblePersonId
            });
        }

        _context.Update(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.ResponsiblePerson)
            .Include(i => i.Room)
            .FirstOrDefaultAsync(i => i.Id == id);
        return item == null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item != null) _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropdowns(InventoryItem? item = null)
    {
        ViewBag.ResponsiblePersonId = new SelectList(
            _context.ResponsiblePersons.OrderBy(rp => rp.FullName), "Id", "FullName", item?.ResponsiblePersonId);
        ViewBag.CategoryId = new SelectList(
            _context.Categories.OrderBy(c => c.Name), "Id", "Name", item?.CategoryId);
        ViewBag.RoomId = new SelectList(
            _context.Rooms.Include(r => r.Department).OrderBy(r => r.Number)
                .Select(r => new { r.Id, Display = $"Room {r.Number}, Floor {r.Floor} ({r.Department!.Name})" }),
            "Id", "Display", item?.RoomId);
        ViewBag.VendorId = new SelectList(
            _context.Vendors.OrderBy(v => v.Name), "Id", "Name", item?.VendorId);
        ViewBag.Statuses = new SelectList(Statuses, item?.Status);
    }
}
