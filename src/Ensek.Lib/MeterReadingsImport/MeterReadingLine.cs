namespace Ensek.Lib.MeterReadingsImport;

using System.Globalization;

public class MeterReadingLine
{
    public const int ExpectedColumnCount = 3;

    public int RowId { get; }
    public string RawData { get; }
    public int? AccountId { get; }
    public DateTime? MeterReadingDateTime { get; }
    public int? MeterReadValue { get; }
    public MeterReadingLineStatus Status { get; private set; }
    public string? Reason { get; private set; }

    public MeterReadingLine(int rowId, string rawData, string[] columnData)
    {
        RowId = rowId;
        RawData = rawData;

        if (columnData.Length < ExpectedColumnCount)
        {
            // If too-few columns then bail out immediately to protect against index out of range errors.
            AddError($"Expected {ExpectedColumnCount} columns, found {columnData.Length}");
            return;
        }

        if (columnData.Length > ExpectedColumnCount)
        {
            // Test data contains 'x' in 4th column ... treat as warning for now.
            AddWarning($"Expected {ExpectedColumnCount} columns, found {columnData.Length}");
        }

        var accountIdText = columnData[0];
        if (string.IsNullOrEmpty(accountIdText)
            || !int.TryParse(accountIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var accountId)
            || accountId < 0)
        {
            AddError($"Could not convert '{accountIdText}' into an AccountId value");
        }
        else
        {
            AccountId = accountId;
        }

        var readingDateText = columnData[1];
        if (string.IsNullOrEmpty(readingDateText)
            || !DateTime.TryParseExact(readingDateText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var readingDate)
            || readingDate.Year < 2000
            || readingDate.Year > 2200)
        {
            AddError($"Could not convert '{readingDateText}' into a Meter Reading Date");
        }
        else
        {
            MeterReadingDateTime = readingDate;
        }

        var readingValueText = columnData[2];
        if (string.IsNullOrEmpty(readingValueText)
            || !int.TryParse(readingValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var readingValue)
            || readingValue < 0
            || readingValue > 99999)
        {
            AddError($"Could not convert '{readingValueText}' into a Meter Read value");
        }
        else
        {
            MeterReadValue = readingValue;
        }

        //if (readingValueText.Length != 5)
        //{
        //    // Original brief said reading-values SHOULD have format of 'NNNNN' - so allow for now.
        //    AddWarning($"Meter read value '{readingValueText}' should have format 'NNNNN'");
        //}
    }

    private void AddError(string reasonText)
    {
        if (Status < MeterReadingLineStatus.Error)
        {
            Status = MeterReadingLineStatus.Error;
            Reason = reasonText;
        }
    }

    private void AddWarning(string reasonText)
    {
        if (Status < MeterReadingLineStatus.Warning)
        {
            Status = MeterReadingLineStatus.Warning;
            Reason = reasonText;
        }
    }
}