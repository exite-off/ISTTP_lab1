using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

// Read-only — logs are created automatically when items change
public class InventoryLogsController : Controller
{
    private readonly InventoryContext _context;

    public InventoryLogsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index(
        string? q, string? actionType, DateTime? dateFrom, DateTime? dateTo, string? groupBy)
    {
        var query = _context.InventoryLogs
            .Include(l => l.InventoryItem)
            .Include(l => l.OldResponsiblePerson)
            .Include(l => l.NewResponsiblePerson)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.Trim().ToLower();
            query = query.Where(l =>
                l.InventoryItem!.Name.ToLower().Contains(qLower) ||
                l.Description!.ToLower().Contains(qLower));
        }
        if (!string.IsNullOrWhiteSpace(actionType))
            query = query.Where(l => l.ActionType == actionType);
        if (dateFrom.HasValue)
            query = query.Where(l => l.ActionDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(l => l.ActionDate < dateTo.Value.AddDays(1));

        ViewBag.Q          = q;
        ViewBag.ActionType = actionType;
        ViewBag.DateFrom   = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo     = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.GroupBy    = groupBy;
        ViewBag.ActionTypes = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            await _context.InventoryLogs
                .Select(l => l.ActionType).Distinct().OrderBy(a => a).ToListAsync(),
            actionType);

        return View(await query.OrderByDescending(l => l.ActionDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var log = await _context.InventoryLogs
            .Include(l => l.InventoryItem)
            .Include(l => l.OldResponsiblePerson)
            .Include(l => l.NewResponsiblePerson)
            .FirstOrDefaultAsync(l => l.Id == id);
        return log == null ? NotFound() : View(log);
    }
}
