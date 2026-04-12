using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

[Authorize]
public class DepartmentsController : Controller
{
    private readonly InventoryContext _context;

    public DepartmentsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = _context.Departments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(d => d.Name.ToLower().Contains(q.Trim().ToLower()));
        return View(await query.OrderBy(d => d.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments
            .Include(d => d.ResponsiblePersons)
            .Include(d => d.Rooms)
            .FirstOrDefaultAsync(d => d.Id == id);
        return dept == null ? NotFound() : View(dept);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([Bind("Name,Address,Phone")] Department department)
    {
        if (!ModelState.IsValid) return View(department);
        _context.Add(department);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments.FindAsync(id);
        return dept == null ? NotFound() : View(dept);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone")] Department department)
    {
        if (id != department.Id) return NotFound();
        if (!ModelState.IsValid) return View(department);
        _context.Update(department);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var dept = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        return dept == null ? NotFound() : View(dept);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept != null) _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
