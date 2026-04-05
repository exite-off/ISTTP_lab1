using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public class InventoryItemDataPortServiceFactory : IDataPortServiceFactory<InventoryItem>
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly InventoryContext _context;

    public InventoryItemDataPortServiceFactory(InventoryContext context) => _context = context;

    public IImportService<InventoryItem> GetImportService(string contentType)
    {
        if (contentType == ExcelContentType)
            return new InventoryItemImportService(_context);

        throw new NotSupportedException(
            $"No import service for content type '{contentType}'.");
    }

    public IExportService<InventoryItem> GetExportService(string contentType)
    {
        if (contentType == ExcelContentType)
            return new InventoryItemExportService(_context);

        throw new NotSupportedException(
            $"No export service for content type '{contentType}'.");
    }
}
