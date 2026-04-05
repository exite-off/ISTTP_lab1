using System.Text.Json;
using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using InventoryMVC.WebMVC.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InventoryMVC.WebMVC.Controllers;

public class InventoryItemsController : Controller
{
    private readonly InventoryContext _context;
    private readonly IDataPortServiceFactory<InventoryItem> _dataPortFactory;

    private static readonly string[] Statuses = ["Active", "Under Repair", "Written Off", "In Storage"];
    private static readonly string[] Currencies = ["UAH", "USD", "EUR", "GBP", "PLN"];

    public InventoryItemsController(
        InventoryContext context,
        IDataPortServiceFactory<InventoryItem> dataPortFactory)
    {
        _context = context;
        _dataPortFactory = dataPortFactory;
    }

    private const int PageSize = 15;

    public async Task<IActionResult> Index(
        string? q, string? status, int? categoryId, int? departmentId,
        string? groupBy, int page = 1)
    {
        var query = _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Room).ThenInclude(r => r!.Department)
            .Include(i => i.ResponsiblePerson)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qTrim = q.Trim();
            bool isNum = int.TryParse(qTrim, out var invNum);
            query = query.Where(i =>
                i.Name.ToLower().Contains(qTrim.ToLower()) ||
                (isNum && i.InventoryNumber == invNum));
        }
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(i => i.Status == status);
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);
        if (departmentId.HasValue)
            query = query.Where(i => i.Room!.DepartmentId == departmentId.Value);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        page = Math.Clamp(page, 1, Math.Max(1, totalPages));

        ViewBag.Q            = q;
        ViewBag.Status       = status;
        ViewBag.CategoryId   = categoryId;
        ViewBag.DepartmentId = departmentId;
        ViewBag.GroupBy      = groupBy;
        ViewBag.CurrentPage  = page;
        ViewBag.TotalPages   = totalPages;
        ViewBag.TotalCount   = totalCount;
        ViewBag.StatusList   = new SelectList(Statuses, status);
        ViewBag.CategoryList = new SelectList(
            await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);
        ViewBag.DepartmentList = new SelectList(
            await _context.Departments.OrderBy(d => d.Name).ToListAsync(), "Id", "Name", departmentId);

        var items = await query
            .OrderBy(i => i.InventoryNumber)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        return View(items);
    }

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
        [Bind("InventoryNumber,Name,EntryDate,WarrantyEndDate,Price,Currency,Status,ResponsiblePersonId,CategoryId,RoomId,VendorId")]
        InventoryItem item)
    {
        if (!ModelState.IsValid) { PopulateDropdowns(); return View(item); }

        if (!await DepartmentsMatch(item.RoomId, item.ResponsiblePersonId))
        {
            ModelState.AddModelError(nameof(item.ResponsiblePersonId),
                "The responsible person must belong to the same department as the selected room.");
            PopulateDropdowns();
            return View(item);
        }

        try
        {
            _context.Add(item);
            await _context.SaveChangesAsync();

            // Log the initial registration / first assignment
            _context.InventoryLogs.Add(new InventoryLog
            {
                ActionDate = DateTime.Now,
                ActionType = "Registration",
                Description = "Item registered and assigned to responsible person",
                InventoryItemId = item.Id,
                OldResponsiblePersonId = null,
                NewResponsiblePersonId = item.ResponsiblePersonId
            });
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
        {
            ModelState.AddModelError(nameof(item.InventoryNumber),
                $"Inventory number {item.InventoryNumber} is already in use. Please choose a different number.");
            PopulateDropdowns();
            return View(item);
        }

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
        [Bind("Id,InventoryNumber,Name,EntryDate,WarrantyEndDate,Price,Currency,Status,ResponsiblePersonId,CategoryId,RoomId,VendorId")]
        InventoryItem item)
    {
        if (id != item.Id) return NotFound();
        if (!ModelState.IsValid) { PopulateDropdowns(item); return View(item); }

        if (!await DepartmentsMatch(item.RoomId, item.ResponsiblePersonId))
        {
            ModelState.AddModelError(nameof(item.ResponsiblePersonId),
                "The responsible person must belong to the same department as the selected room.");
            PopulateDropdowns(item);
            return View(item);
        }

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

        try
        {
            _context.Update(item);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
        {
            ModelState.AddModelError(nameof(item.InventoryNumber),
                $"Inventory number {item.InventoryNumber} is already in use. Please choose a different number.");
            PopulateDropdowns(item);
            return View(item);
        }

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

    // ── Import / Export ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Import() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(
        IFormFile itemsFile, CancellationToken cancellationToken)
    {
        if (itemsFile == null || itemsFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file.");
            return View();
        }

        try
        {
            var importService = _dataPortFactory.GetImportService(itemsFile.ContentType);
            using var stream = itemsFile.OpenReadStream();
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
            FileDownloadName = $"inventory_{DateTime.UtcNow:yyyy-MM-dd}.xlsx"
        };
    }

    // Returns false if the responsible person belongs to a different department than the room
    private async Task<bool> DepartmentsMatch(int roomId, int personId)
    {
        var room = await _context.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roomId);
        var person = await _context.ResponsiblePersons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == personId);
        if (room == null || person == null) return true; // let other validators handle missing refs
        return room.DepartmentId == person.DepartmentId;
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
        ViewBag.Currencies = new SelectList(Currencies, item?.Currency ?? "UAH");

        // JSON maps for client-side department filtering (camelCase for JS)
        var camel = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        ViewBag.RoomDeptMapJson = JsonSerializer.Serialize(
            _context.Rooms.Select(r => new { r.Id, r.DepartmentId }).ToList()
                .ToDictionary(r => r.Id, r => r.DepartmentId), camel);
        ViewBag.PersonsJson = JsonSerializer.Serialize(
            _context.ResponsiblePersons.OrderBy(p => p.FullName)
                .Select(p => new { p.Id, p.FullName, p.DepartmentId }).ToList(), camel);
    }
}
