namespace Ensek.Lib.MeterReadingsImport;

public class MeterReadingBulkUploadRequest
{
    /// <summary>
    /// If this is true then we are only validating for errors. Nothing is stored to the database.
    /// TODO - not used yet. Thinking of adding as parameter to the sproc - so we get AccountId and Duplicate validation, but no inserts.
    /// </summary>
    public bool IsValidationOnlyMode { get; set; } = false;

    public Guid RequestId { get; set; }
}