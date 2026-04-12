using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

[Authorize]
public class VendorsController : Controller
{
    private readonly InventoryContext _context;

    public VendorsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index(string? q)
    {
        ViewBag.Q = q;
        var query = _context.Vendors.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(v => v.Name.ToLower().Contains(q.Trim().ToLower()));
        return View(await query.OrderBy(v => v.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var vendor = await _context.Vendors
            .Include(v => v.InventoryItems)
            .FirstOrDefaultAsync(v => v.Id == id);
        return vendor == null ? NotFound() : View(vendor);
    }

    [Authorize(Roles = RoleNames.Admin)]
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Create([Bind("Name,Address,ContactPhone,Email")] Vendor vendor)
    {
        if (!ModelState.IsValid) return View(vendor);
        _context.Add(vendor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var vendor = await _context.Vendors.FindAsync(id);
        return vendor == null ? NotFound() : View(vendor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,ContactPhone,Email")] Vendor vendor)
    {
        if (id != vendor.Id) return NotFound();
        if (!ModelState.IsValid) return View(vendor);
        _context.Update(vendor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
        return vendor == null ? NotFound() : View(vendor);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor != null) _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
