using InventoryMVC.Domain.Entities;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public interface IExportService<TEntity> where TEntity : Entity
{
    Task WriteToAsync(Stream stream, CancellationToken cancellationToken);
}
