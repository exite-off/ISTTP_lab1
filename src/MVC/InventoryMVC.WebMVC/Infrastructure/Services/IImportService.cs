using InventoryMVC.Domain.Entities;

namespace InventoryMVC.WebMVC.Infrastructure.Services;

public interface IImportService<TEntity> where TEntity : Entity
{
    Task ImportFromStreamAsync(Stream stream, CancellationToken cancellationToken);
}
