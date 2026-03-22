using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

public class DepartmentsController : Controller
{
    private readonly InventoryContext _context;

    public DepartmentsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.Departments.ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments
            .Include(d => d.ResponsiblePersons)
            .Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == id);
        return dept == null ? NotFound() : View(dept);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Address,Phone")] Department department)
    {
        if (!ModelState.IsValid) return View(department);
        _context.Add(department);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments.FindAsync(id);
        return dept == null ? NotFound() : View(dept);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone")] Department department)
    {
        if (id != department.Id) return NotFound();
        if (!ModelState.IsValid) return View(department);
        _context.Update(department);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        return dept == null ? NotFound() : View(dept);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept != null) _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
