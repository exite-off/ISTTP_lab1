using InventoryMVC.Domain.Entities;
using InventoryMVC.Infrastructure;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public class ResponsiblePersonDataPortServiceFactory : IDataPortServiceFactory<ResponsiblePerson>
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly InventoryContext _context;

    public ResponsiblePersonDataPortServiceFactory(InventoryContext context) => _context = context;

    public IImportService<ResponsiblePerson> GetImportService(string contentType)
    {
        if (contentType == ExcelContentType)
            return new ResponsiblePersonImportService(_context);

        throw new NotSupportedException(
            $"No import service for content type '{contentType}'.");
    }

    public IExportService<ResponsiblePerson> GetExportService(string contentType)
    {
        if (contentType == ExcelContentType)
            return new ResponsiblePersonExportService(_context);

        throw new NotSupportedException(
            $"No export service for content type '{contentType}'.");
    }
}
