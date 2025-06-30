namespace Ensek.Lib.MeterReadingsImport;

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient.Server;

/// <summary>
/// Low-memory way to stream comma-separated records into a MeterReading-based TVP parameter.
/// </summary>
public class MeterReadingRecordEnumerator
{
    private static readonly SqlMetaData[] MetaData =
    {
        new("RowId", SqlDbType.Int),
        new("AccountId", SqlDbType.Int),
        new("MeterReadingDateTime", SqlDbType.DateTime),
        new("MeterReadValue", SqlDbType.Int)
    };

    private readonly IAsyncEnumerable<string> commaSeparatedRecords;
    private readonly int maxErrorCount;

    public List<string> Errors { get; } = new();
    public int ValidatedCount { get; private set; }
    public int RejectedCount { get; private set; }

    public MeterReadingRecordEnumerator(IAsyncEnumerable<string> commaSeparatedRecords, int maxErrorCount)
    {
        this.commaSeparatedRecords = commaSeparatedRecords;
        this.maxErrorCount = maxErrorCount;
    }

    public async IAsyncEnumerable<SqlDataRecord> GetDataRecords([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rowId = 0;
        await foreach (var commaSeparatedRecord in commaSeparatedRecords.WithCancellation(cancellationToken))
        {
            var columnData = commaSeparatedRecord.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            rowId++;

            if (rowId == 1 && !ValidateHeaders(rowId, columnData))
            {
                AddError(rowId, "Upload terminated");
                throw new InvalidDataFileFormatException("Invalid file format for Meter Reading upload data file");
            }

            if (rowId > 1 && columnData.Length > 0)
            {
                var meterReadingLine = new MeterReadingLine(rowId, commaSeparatedRecord, columnData);
                var sqlDataRecord = meterReadingLine.Parse();
                if (sqlDataRecord != null)
                {
                    ValidatedCount++;
                    yield return sqlDataRecord;
                }
                else if (RejectedCount > maxErrorCount)
                {
                    throw new InvalidDataFileFormatException("Too many errors - terminating");
                }
                else
                {
                    AddError(rowId, meterReadingLine.Reason ?? "Unknown");
                    RejectedCount++;
                }
            }
        }
    }

    private bool ValidateHeaders(int rowId, string[] columns)
    {
        var isOk = true;
        if (columns.Length != MetaData.Length - 1)
        {
            isOk = AddError(rowId, $"Expected {MetaData.Length - 1} columns, found {columns.Length}");
        }

        for (var i = 1; i < MetaData.Length; i++)
        {
            var expectedColumnName = MetaData[i].Name;
            if (i <= columns.Length && !expectedColumnName.Equals(columns[i - 1], StringComparison.Ordinal))
            {
                isOk = AddError(rowId, $"Expected column {i} to have heading '{expectedColumnName}'");
            }
        }

        return isOk;
    }

    private bool AddError(int rowId, string? errorText)
    {
        if (Errors.Count <= maxErrorCount || rowId == 1)
        {
            Errors.Add($"Row {rowId} - {errorText}");
        }

        return false;
    }
}