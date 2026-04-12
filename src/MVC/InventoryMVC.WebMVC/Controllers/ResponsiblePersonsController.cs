using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Extensions;
using InventoryMVC.WebMVC.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

[Authorize]
public class ResponsiblePersonsController : Controller
{
    private readonly InventoryContext _context;
    private readonly IDataPortServiceFactory<ResponsiblePerson> _dataPortFactory;

    public ResponsiblePersonsController(
        InventoryContext context,
        IDataPortServiceFactory<ResponsiblePerson> dataPortFactory)
    {
        _context = context;
        _dataPortFactory = dataPortFactory;
    }

    public async Task<IActionResult> Index(string? q, int? departmentId, string? groupBy)
    {
        var query = _context.ResponsiblePersons.Include(rp => rp.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.FullName.ToLower().Contains(q.Trim().ToLower()));
        if (departmentId.HasValue)
            query = query.Where(p => p.DepartmentId == departmentId.Value);

        ViewBag.Q            = q;
        ViewBag.DepartmentId = departmentId;
        ViewBag.GroupBy      = groupBy;
        ViewBag.DepartmentList = new SelectList(
            await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId);

        return View(await query.OrderBy(p => p.FullName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons
            .Include(rp => rp.Department)
            .Include(rp => rp.InventoryItems).ThenInclude(i => i.Category)
            .FirstOrDefaultAsync(rp => rp.Id == id);
        return person == null ? NotFound() : View(person);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult Create()
    {
        PopulateDepartments();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([Bind("FullName,Position,Email,DepartmentId")] ResponsiblePerson person)
    {
        if (!ModelState.IsValid) { PopulateDepartments(); return View(person); }
        _context.Add(person);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons.FindAsync(id);
        if (person == null) return NotFound();
        PopulateDepartments(person.DepartmentId);
        return View(person);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Position,Email,DepartmentId")] ResponsiblePerson person)
    {
        if (id != person.Id) return NotFound();
        if (!ModelState.IsValid) { PopulateDepartments(person.DepartmentId); return View(person); }
        _context.Update(person);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var person = await _context.ResponsiblePersons
            .Include(rp => rp.Department)
            .FirstOrDefaultAsync(rp => rp.Id == id);
        return person == null ? NotFound() : View(person);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
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

    // ── Import / Export ───────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult Import() => View();

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Import(
        IFormFile personsFile, CancellationToken cancellationToken)
    {
        if (personsFile == null || personsFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file.");
            return View();
        }

        try
        {
            var importService = _dataPortFactory.GetImportService(personsFile.ContentType);
            using var stream = personsFile.OpenReadStream();
            await importService.ImportFromStreamAsync(stream, cancellationToken);
        }
        catch (ImportException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
        catch (NotSupportedException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Export(
        [FromQuery] string contentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        CancellationToken cancellationToken = default)
    {
        var exportService = _dataPortFactory.GetExportService(contentType);
        var memoryStream = new MemoryStream();
        await exportService.WriteToAsync(memoryStream, cancellationToken);
        await memoryStream.FlushAsync(cancellationToken);
        memoryStream.Position = 0;

        return new FileStreamResult(memoryStream, contentType)
        {
            FileDownloadName = $"responsible_persons_{DateTime.UtcNow:yyyy-MM-dd}.xlsx"
        };
    }

    private void PopulateDepartments(int? selected = null) =>
        ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Name), "Id", "Name", selected);
}
