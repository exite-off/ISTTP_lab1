using ClosedXML.Excel;
using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public class InventoryItemExportService : IExportService<InventoryItem>
{
    private const string WorksheetName = "Inventory Items";

    private static readonly IReadOnlyList<string> Headers =
    [
        "Inventory #", "Name", "Entry Date", "Warranty End Date",
        "Price", "Currency", "Status",
        "Category", "Vendor", "Department", "Room #", "Responsible Person"
    ];

    private readonly InventoryContext _context;

    public InventoryItemExportService(InventoryContext context) => _context = context;

    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
            throw new ArgumentException("Stream is not writable", nameof(stream));

        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Vendor)
            .Include(i => i.Room).ThenInclude(r => r!.Department)
            .Include(i => i.ResponsiblePerson)
            .OrderBy(i => i.InventoryNumber)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(WorksheetName);

        WriteHeader(ws);

        int rowIndex = 2;
        foreach (var item in items)
        {
            WriteItem(ws, item, rowIndex++);
        }

        ws.Columns().AdjustToContents();
        workbook.SaveAs(stream);
    }

    private static void WriteHeader(IXLWorksheet ws)
    {
        for (int col = 0; col < Headers.Count; col++)
        {
            ws.Cell(1, col + 1).Value = Headers[col];
        }
        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private static void WriteItem(IXLWorksheet ws, InventoryItem item, int rowIndex)
    {
        int col = 1;
        ws.Cell(rowIndex, col++).Value = item.InventoryNumber;
        ws.Cell(rowIndex, col++).Value = item.Name;
        ws.Cell(rowIndex, col++).Value = item.EntryDate.ToString("yyyy-MM-dd");
        ws.Cell(rowIndex, col++).Value = item.WarrantyEndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        ws.Cell(rowIndex, col++).Value = item.Price;
        ws.Cell(rowIndex, col++).Value = item.Currency;
        ws.Cell(rowIndex, col++).Value = item.Status;
        ws.Cell(rowIndex, col++).Value = item.Category?.Name ?? string.Empty;
        ws.Cell(rowIndex, col++).Value = item.Vendor?.Name ?? string.Empty;
        ws.Cell(rowIndex, col++).Value = item.Room?.Department?.Name ?? string.Empty;
        ws.Cell(rowIndex, col++).Value = item.Room?.Number.ToString() ?? string.Empty;
        ws.Cell(rowIndex, col++).Value = item.ResponsiblePerson?.FullName ?? string.Empty;
    }
}
