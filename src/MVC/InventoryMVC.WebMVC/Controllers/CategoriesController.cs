using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

public class CategoriesController : Controller
{
    private readonly InventoryContext _context;

    public CategoriesController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = _context.Categories.Include(c => c.InventoryItems).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.ToLower().Contains(q.Trim().ToLower()));
        return View(await query.OrderBy(c => c.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories
            .Include(c => c.InventoryItems).ThenInclude(i => i.ResponsiblePerson)
            .FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? NotFound() : View(category);
    }

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
    {
        if (!ModelState.IsValid) return View(category);
        _context.Add(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FindAsync(id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Category category)
    {
        if (id != category.Id) return NotFound();
        if (!ModelState.IsValid) return View(category);
        _context.Update(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        return category == null ? NotFound() : View(category);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null) _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
