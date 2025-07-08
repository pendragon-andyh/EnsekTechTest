namespace Ensek.Lib.MeterReadingsImport;

public class BulkUploadResponse
{
    public BulkUploadResponse(int successCount, int warningCount, int errorCount, long milliseconds)
    {
        SuccessCount = successCount;
        WarningCount = warningCount;
        ErrorCount = errorCount;
        Milliseconds = milliseconds;
    }

    public int SuccessCount { get; }

    public int WarningCount { get; }

    public int ErrorCount { get; }
    public long Milliseconds { get; }
}