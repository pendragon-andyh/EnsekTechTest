namespace Ensek.Lib.MeterReadingsImport;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Low-memory way to stream comma-separated records into a MeterReading-based TVP parameter.
/// </summary>
public class MeterReadingRecordEnumerator
{
    private readonly IAsyncEnumerable<string> commaSeparatedRecords;

    public MeterReadingRecordEnumerator(IAsyncEnumerable<string> commaSeparatedRecords)
    {
        this.commaSeparatedRecords = commaSeparatedRecords;
    }

    public async IAsyncEnumerable<MeterReadingLine> GetDataRecords([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rowId = 0;
        await foreach (var commaSeparatedRecord in commaSeparatedRecords.WithCancellation(cancellationToken))
        {
            var columnData = commaSeparatedRecord.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            rowId++;

            if (rowId == 1)
            {
                ValidateHeaders(columnData);
            }

            if (rowId > 1 && columnData.Length > 0)
            {
                yield return new MeterReadingLine(rowId, commaSeparatedRecord, columnData);
            }
        }
    }

    private static void ValidateHeaders(string[] columns)
    {
        if (columns.Length != MeterReadingLine.ExpectedColumnCount)
        {
            throw new InvalidDataFileFormatException($"Expected {MeterReadingLine.ExpectedColumnCount} column headings, found {columns.Length}");
        }

        // TODO - validate the column heading names.
    }
}