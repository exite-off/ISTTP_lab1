using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

public class ResponsiblePersonsController : Controller
{
    private readonly InventoryContext _context;

    public ResponsiblePersonsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.ResponsiblePersons.Include(rp => rp.Department).ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons
            .Include(rp => rp.Department)
            .Include(rp => rp.InventoryItems).ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(rp => rp.Id == id);
        return person == null ? NotFound() : View(person);
    }

    public IActionResult Create()
    {
        PopulateDepartments();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Position,Email,DepartmentId")] ResponsiblePerson person)
    {
        if (!ModelState.IsValid) { PopulateDepartments(); return View(person); }
        _context.Add(person);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons.FindAsync(id);
        if (person == null) return NotFound();
        PopulateDepartments(person.DepartmentId);
        return View(person);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Position,Email,DepartmentId")] ResponsiblePerson person)
    {
        if (id != person.Id) return NotFound();
        if (!ModelState.IsValid) { PopulateDepartments(person.DepartmentId); return View(person); }
        _context.Update(person);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons
            .Include(rp => rp.Department)
            .FirstOrDefaultAsync(rp => rp.Id == id);
        return person == null ? NotFound() : View(person);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Check for assigned items before deletion
        var hasItems = await _context.InventoryItems.AnyAsync(i => i.ResponsiblePersonId == id);
        if (hasItems)
        {
            ModelState.AddModelError("", "Cannot delete: this person has assigned inventory items.");
            var person = await _context.ResponsiblePersons.Include(rp => rp.Department).FirstOrDefaultAsync(rp => rp.Id == id);
            return View(person);
        }
        var p = await _context.ResponsiblePersons.FindAsync(id);
        if (p != null) _context.ResponsiblePersons.Remove(p);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDepartments(int? selected = null) =>
        ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", selected);
}
