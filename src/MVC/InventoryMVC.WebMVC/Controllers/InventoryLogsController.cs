using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Controllers;

// Read-only — logs are created automatically when items change
public class InventoryLogsController : Controller
{
    private readonly InventoryContext _context;

    public InventoryLogsController(InventoryContext context) => _context = context;

    public async Task<IActionResult> Index() =>
        View(await _context.InventoryLogs
            .Include(l => l.InventoryItem)
            .Include(l => l.OldResponsiblePerson)
            .Include(l => l.NewResponsiblePerson)
            .OrderByDescending(l => l.ActionDate)
            .ToListAsync());

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
