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

    public async Task<IActionResult> Index(string? q, int? departmentId, int? floor, string? groupBy)
    {
        var query = _context.Rooms.Include(r => r.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q) && int.TryParse(q.Trim(), out var roomNum))
            query = query.Where(r => r.Number == roomNum);
        if (departmentId.HasValue)
            query = query.Where(r => r.DepartmentId == departmentId.Value);
        if (floor.HasValue)
            query = query.Where(r => r.Floor == floor.Value);

        ViewBag.Q            = q;
        ViewBag.DepartmentId = departmentId;
        ViewBag.Floor        = floor;
        ViewBag.GroupBy      = groupBy;
        ViewBag.DepartmentList = new SelectList(
            await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId);
        ViewBag.FloorList = new SelectList(
            await _context.Rooms.Select(r => r.Floor).Distinct().OrderBy(f => f).ToListAsync(), floor);

        return View(await query.OrderBy(r => r.Number).ToListAsync());
    }

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
