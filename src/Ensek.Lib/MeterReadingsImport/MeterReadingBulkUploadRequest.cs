namespace Ensek.Lib.MeterReadingsImport;

public class MeterReadingBulkUploadRequest
{
    /// <summary>
    /// If this is true then we are only validating for errors. Nothing is stored to the database.
    /// </summary>
    public bool IsValidationOnlyMode { get; set; } = false;

    public Guid RequestId { get; set; }
    public int MaxErrorCount { get; set; } = 100;
}