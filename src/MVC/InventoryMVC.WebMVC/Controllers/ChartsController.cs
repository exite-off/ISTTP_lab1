using InventoryMVC.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryMVC.WebMVC.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChartsController : ControllerBase
{
    private readonly InventoryContext _context;
    private static readonly JsonSerializerOptions CamelCase =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ChartsController(InventoryContext context) => _context = context;

    // Items grouped by category name
    private record CategoryCountItem(string Category, int Count);

    // Items grouped by status
    private record StatusCountItem(string Status, int Count);

    [HttpGet("countByCategory")]
    public async Task<IActionResult> GetCountByCategoryAsync(CancellationToken cancellationToken)
    {
        var items = await _context.Categories
            .Select(c => new CategoryCountItem(c.Name, c.InventoryItems.Count()))
            .ToListAsync(cancellationToken);

        return new JsonResult(items, CamelCase);
    }

    [HttpGet("countByStatus")]
    public async Task<IActionResult> GetCountByStatusAsync(CancellationToken cancellationToken)
    {
        var items = await _context.InventoryItems
            .GroupBy(i => i.Status)
            .Select(g => new StatusCountItem(g.Key, g.Count()))
            .ToListAsync(cancellationToken);

        return new JsonResult(items, CamelCase);
    }
}
