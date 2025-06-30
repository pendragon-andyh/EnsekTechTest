namespace Ensek.Lib.MeterReadingsImport;

using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient.Server;

public class MeterReadingLine
{
    private const int ExpectedColumnCount = 3;

    private static readonly SqlMetaData[] MetaData =
    {
        new("RowId", SqlDbType.Int),
        new("AccountId", SqlDbType.Int),
        new("MeterReadingDateTime", SqlDbType.DateTime),
        new("MeterReadValue", SqlDbType.Int),
        new("ReadingStatus", SqlDbType.Int),
        new("StatusReason", SqlDbType.VarChar, 80)
    };

    private readonly string[] columnData;

    public MeterReadingLine(int rowId, string rawData, string[] columnData)
    {
        this.columnData = columnData;
        RowId = rowId;
        RawData = rawData;
    }

    public int RowId { get; }
    public string RawData { get; }

    public MeterReadingLineStatus Status { get; private set; }

    public string? Reason { get; private set; }

    public SqlDataRecord? Parse()
    {
        if (columnData.Length < ExpectedColumnCount)
        {
            // If too-few columns then bail out immediately to protect against index out of range errors.
            AddError($"Expected {ExpectedColumnCount} columns, found {columnData.Length}");
            return null;
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
            return null;
        }

        var readingDateText = columnData[1];
        if (string.IsNullOrEmpty(readingDateText)
            || !DateTime.TryParseExact(readingDateText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var readingDate)
            || readingDate.Year < 2000
            || readingDate.Year > 2200)
        {
            AddError($"Could not convert '{readingDateText}' into a Meter Reading Date");
            return null;
        }

        var readingValueText = columnData[2];
        if (string.IsNullOrEmpty(readingValueText)
            || !int.TryParse(readingValueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var readingValue)
            || readingValue < 0
            || readingValue > 99999)
        {
            AddError($"Could not convert '{readingValueText}' into a Meter Read value");
            return null;
        }

        //if (readingValueText.Length != 5)
        //{
        //    // Original brief said reading-values SHOULD have format of 'NNNNN' - so allow for now.
        //    AddWarning($"Meter read value '{readingValueText}' should have format 'NNNNN'");
        //}

        var record = new SqlDataRecord(MetaData);

        record.SetInt32(0, RowId);
        record.SetInt32(1, accountId);
        record.SetDateTime(2, readingDate);
        record.SetInt32(3, readingValue);
        record.SetInt32(4, (int)Status);

        if (string.IsNullOrEmpty(Reason))
        {
            record.SetDBNull(5);
        }
        else
        {
            record.SetString(5, Reason);
        }

        return record;
    }

    public void AddError(string reasonText)
    {
        if (Status < MeterReadingLineStatus.Error)
        {
            Status = MeterReadingLineStatus.Error;
            Reason = reasonText;
        }
    }

    public void AddWarning(string reasonText)
    {
        if (Status < MeterReadingLineStatus.Warning)
        {
            Status = MeterReadingLineStatus.Warning;
            Reason = reasonText;
        }
    }
}