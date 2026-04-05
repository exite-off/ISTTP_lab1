using ClosedXML.Excel;
using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

// Expected Excel columns:
// 1: Full Name   2: Position   3: Email   4: Department (name, must exist)
public class ResponsiblePersonImportService : IImportService<ResponsiblePerson>
{
    private const int ColFullName   = 1;
    private const int ColPosition   = 2;
    private const int ColEmail      = 3;
    private const int ColDepartment = 4;

    private readonly InventoryContext _context;

    public ResponsiblePersonImportService(InventoryContext context) => _context = context;

    public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Stream is not readable", nameof(stream));

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null) return;

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            await AddPersonAsync(row, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AddPersonAsync(IXLRow row, CancellationToken cancellationToken)
    {
        var rowNum = row.RowNumber();
        var email = GetEmail(row, rowNum);

        // Skip if a person with this email already exists
        var existing = await _context.ResponsiblePersons
            .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
        if (existing is not null) return;

        var department = await GetDepartmentAsync(row, rowNum, cancellationToken);

        var person = new ResponsiblePerson
        {
            FullName     = GetFullName(row, rowNum),
            Position     = GetPosition(row, rowNum),
            Email        = email,
            DepartmentId = department.Id,
        };

        _context.ResponsiblePersons.Add(person);
    }

    // ── Cell readers ──────────────────────────────────────────────────────────

    private static string GetFullName(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColFullName).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(val))
            throw new ImportException(rowNum, "Full name is required.");
        return val;
    }

    private static string GetPosition(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColPosition).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(val))
            throw new ImportException(rowNum, "Position is required.");
        return val;
    }

    private static string GetEmail(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColEmail).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(val))
            throw new ImportException(rowNum, "Email is required.");
        return val;
    }

    // ── Entity resolver ───────────────────────────────────────────────────────

    private async Task<Department> GetDepartmentAsync(
        IXLRow row, int rowNum, CancellationToken cancellationToken)
    {
        var name = row.Cell(ColDepartment).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ImportException(rowNum, "Department name is required.");

        var dept = await _context.Departments
            .FirstOrDefaultAsync(d => d.Name == name, cancellationToken);
        if (dept is null)
            throw new ImportException(rowNum,
                $"Department '{name}' not found in the database.");
        return dept;
    }
}
