using ClosedXML.Excel;
using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public class ResponsiblePersonExportService : IExportService<ResponsiblePerson>
{
    private const string WorksheetName = "Responsible Persons";

    private static readonly IReadOnlyList<string> Headers =
        ["Full Name", "Position", "Email", "Department"];

    private readonly InventoryContext _context;

    public ResponsiblePersonExportService(InventoryContext context) => _context = context;

    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanWrite)
            throw new ArgumentException("Stream is not writable", nameof(stream));

        var persons = await _context.ResponsiblePersons
            .Include(p => p.Department)
            .OrderBy(p => p.FullName)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(WorksheetName);

        WriteHeader(ws);

        int rowIndex = 2;
        foreach (var person in persons)
            WritePerson(ws, person, rowIndex++);

        ws.Columns().AdjustToContents();
        workbook.SaveAs(stream);
    }

    private static void WriteHeader(IXLWorksheet ws)
    {
        for (int col = 0; col < Headers.Count; col++)
            ws.Cell(1, col + 1).Value = Headers[col];

        ws.Row(1).Style.Font.Bold = true;
        ws.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
    }

    private static void WritePerson(IXLWorksheet ws, ResponsiblePerson person, int rowIndex)
    {
        ws.Cell(rowIndex, 1).Value = person.FullName;
        ws.Cell(rowIndex, 2).Value = person.Position;
        ws.Cell(rowIndex, 3).Value = person.Email;
        ws.Cell(rowIndex, 4).Value = person.Department?.Name ?? string.Empty;
    }
}
