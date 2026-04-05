using ClosedXML.Excel;
using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

// Expected Excel columns:
// 1: Inventory #   2: Name          3: Entry Date (yyyy-MM-dd)
// 4: Warranty End  5: Price         6: Currency     7: Status
// 8: Category      9: Vendor       10: Department  11: Room #
// 12: Responsible Person
public class InventoryItemImportService : IImportService<InventoryItem>
{
    private const int ColInventoryNumber = 1;
    private const int ColName            = 2;
    private const int ColEntryDate       = 3;
    private const int ColWarrantyEnd     = 4;
    private const int ColPrice           = 5;
    private const int ColCurrency        = 6;
    private const int ColStatus          = 7;
    private const int ColCategory        = 8;
    private const int ColVendor          = 9;
    private const int ColDepartment      = 10;
    private const int ColRoomNumber      = 11;
    private const int ColPerson          = 12;

    private readonly InventoryContext _context;

    public InventoryItemImportService(InventoryContext context) => _context = context;

    public async Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Stream is not readable", nameof(stream));

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null) return;

        foreach (var row in worksheet.RowsUsed().Skip(1)) // skip header
        {
            await AddInventoryItemAsync(row, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AddInventoryItemAsync(IXLRow row, CancellationToken cancellationToken)
    {
        var rowNum = row.RowNumber();
        var invNumber = GetInventoryNumber(row, rowNum);

        // Skip if already exists
        var existing = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.InventoryNumber == invNumber, cancellationToken);
        if (existing is not null) return;

        var category = await GetOrCreateCategoryAsync(row, cancellationToken);
        var vendor   = await GetOrCreateVendorAsync(row, cancellationToken);
        var room     = await GetRoomAsync(row, rowNum, cancellationToken);
        var person   = await GetResponsiblePersonAsync(row, rowNum, cancellationToken);

        var item = new InventoryItem
        {
            InventoryNumber      = invNumber,
            Name                 = GetName(row, rowNum),
            EntryDate            = GetEntryDate(row, rowNum),
            WarrantyEndDate      = GetWarrantyEndDate(row),
            Price                = GetPrice(row, rowNum),
            Currency             = GetCurrency(row),
            Status               = GetStatus(row, rowNum),
            CategoryId           = category.Id,
            VendorId             = vendor.Id,
            RoomId               = room.Id,
            ResponsiblePersonId  = person.Id,
        };

        _context.InventoryItems.Add(item);
    }

    // ── Cell readers ──────────────────────────────────────────────────────────

    private static int GetInventoryNumber(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColInventoryNumber).GetValue<string>().Trim();
        if (!int.TryParse(val, out var n) || n <= 0)
            throw new ImportException(rowNum, $"Invalid inventory number: '{val}'.");
        return n;
    }

    private static string GetName(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColName).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(val))
            throw new ImportException(rowNum, "Name is required.");
        return val;
    }

    private static DateTime GetEntryDate(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColEntryDate).GetValue<string>().Trim();
        if (!DateTime.TryParse(val, out var date))
            throw new ImportException(rowNum, $"Invalid entry date: '{val}'. Use yyyy-MM-dd.");
        return date;
    }

    private static DateTime? GetWarrantyEndDate(IXLRow row)
    {
        var val = row.Cell(ColWarrantyEnd).GetValue<string>().Trim();
        return DateTime.TryParse(val, out var date) ? date : null;
    }

    private static decimal GetPrice(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColPrice).GetValue<string>().Trim();
        if (!decimal.TryParse(val, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var price) || price < 0)
            throw new ImportException(rowNum, $"Invalid price: '{val}'.");
        return price;
    }

    private static string GetCurrency(IXLRow row)
    {
        var val = row.Cell(ColCurrency).GetValue<string>().Trim();
        return string.IsNullOrWhiteSpace(val) ? "UAH" : val.ToUpperInvariant();
    }

    private static string GetStatus(IXLRow row, int rowNum)
    {
        var val = row.Cell(ColStatus).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(val))
            throw new ImportException(rowNum, "Status is required.");
        return val;
    }

    // ── Entity resolvers ─────────────────────────────────────────────────────

    private async Task<Category> GetOrCreateCategoryAsync(IXLRow row, CancellationToken ct)
    {
        var name = row.Cell(ColCategory).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Uncategorized";

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name, ct);
        if (category is null)
        {
            category = new Category { Name = name };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(ct); // flush to get Id
        }
        return category;
    }

    private async Task<Vendor> GetOrCreateVendorAsync(IXLRow row, CancellationToken ct)
    {
        var name = row.Cell(ColVendor).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Unknown";

        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.Name == name, ct);
        if (vendor is null)
        {
            vendor = new Vendor { Name = name, ContactPhone = "—" };
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync(ct);
        }
        return vendor;
    }

    private async Task<Room> GetRoomAsync(IXLRow row, int rowNum, CancellationToken ct)
    {
        var deptName = row.Cell(ColDepartment).GetValue<string>().Trim();
        var roomNumStr = row.Cell(ColRoomNumber).GetValue<string>().Trim();

        if (!int.TryParse(roomNumStr, out var roomNumber))
            throw new ImportException(rowNum, $"Invalid room number: '{roomNumStr}'.");

        var room = await _context.Rooms
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Number == roomNumber &&
                                      r.Department!.Name == deptName, ct);
        if (room is null)
            throw new ImportException(rowNum,
                $"Room {roomNumber} in department '{deptName}' not found.");
        return room;
    }

    private async Task<ResponsiblePerson> GetResponsiblePersonAsync(
        IXLRow row, int rowNum, CancellationToken ct)
    {
        var fullName = row.Cell(ColPerson).GetValue<string>().Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ImportException(rowNum, "Responsible person name is required.");

        var person = await _context.ResponsiblePersons
            .FirstOrDefaultAsync(p => p.FullName == fullName, ct);
        if (person is null)
            throw new ImportException(rowNum,
                $"Responsible person '{fullName}' not found in the database.");
        return person;
    }
}
