namespace Ensek.Lib.MeterReadingsImport;

public class BulkUploadResponse
{
    public BulkUploadResponse(int successCount, int warningCount, int errorCount)
    {
        SuccessCount = successCount;
        WarningCount = warningCount;
        ErrorCount = errorCount;
    }

    public int SuccessCount { get; }

    public int WarningCount { get; }

    public int ErrorCount { get; }
}