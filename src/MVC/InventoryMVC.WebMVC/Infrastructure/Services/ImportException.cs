namespace InventoryMVC.WebMVC.Infrastructure.Services;

// Thrown when an Excel row cannot be parsed or required references are missing
public class ImportException : Exception
{
    public int RowNumber { get; }

    public ImportException(int rowNumber, string message)
        : base($"Row {rowNumber}: {message}")
    {
        RowNumber = rowNumber;
    }
}
