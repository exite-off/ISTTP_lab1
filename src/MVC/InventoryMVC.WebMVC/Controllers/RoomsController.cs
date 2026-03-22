using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

public class RoomsController : Controller
{
    private readonly InventoryContext _context;

    public RoomsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.Rooms.Include(r => r.Department).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var room = await _context.Rooms
            .Include(r => r.Department)
            .Include(r => r.InventoryItems).ThenInclude(i => i.ResponsiblePerson)
            .FirstOrDefaultAsync(r => r.Id == id);
        return room == null ? NotFound() : View(room);
    }

    public IActionResult Create()
    {
        PopulateDepartments();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Number,Floor,DepartmentId")] Room room)
    {
        if (!ModelState.IsValid) { PopulateDepartments(); return View(room); }
        _context.Add(room);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var room = await _context.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        PopulateDepartments(room.DepartmentId);
        return View(room);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Number,Floor,DepartmentId")] Room room)
    {
        if (id != room.Id) return NotFound();
        if (!ModelState.IsValid) { PopulateDepartments(room.DepartmentId); return View(room); }
        _context.Update(room);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var room = await _context.Rooms.Include(r => r.Department).FirstOrDefaultAsync(r => r.Id == id);
        return room == null ? NotFound() : View(room);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room != null) _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDepartments(int? selected = null) =>
        ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", selected);
}
