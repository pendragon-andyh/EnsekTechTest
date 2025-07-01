namespace Ensek.Lib.MeterReadingsImport;

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

[ExcludeFromCodeCoverage]
public class MeterReadingWriter : IMeterReadingWriter
{
    private static readonly SqlMetaData[] MetaData =
    {
        new("RowId", SqlDbType.Int),
        new("AccountId", SqlDbType.Int),
        new("MeterReadingDateTime", SqlDbType.DateTime),
        new("MeterReadValue", SqlDbType.Int),
        new("ReadingStatus", SqlDbType.Int),
        new("StatusReason", SqlDbType.VarChar, 80)
    };

    private readonly string connectionString;

    public MeterReadingWriter(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<BulkUploadResponse> MeterReadingBulkLoad(
        MeterReadingBulkUploadRequest request,
        IEnumerable<MeterReadingLine> meterReadings,
        CancellationToken cancellationToken)
    {
        await using var conn = await GetDatabaseConnection(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.MeterReadingBulkLoad";

        cmd.Parameters.AddWithValue("RequestId", request.RequestId);

        var tvpParam = cmd.Parameters.AddWithValue("@Data", ToSqlDataRecords(meterReadings));
        tvpParam.SqlDbType = SqlDbType.Structured;
        tvpParam.TypeName = "dbo.MeterReadingTVP";

        await using var resultSet = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await resultSet.ReadAsync(cancellationToken))
        {
            return new BulkUploadResponse(resultSet.GetInt32(0), resultSet.GetInt32(1), resultSet.GetInt32(2));
        }

        throw new InvalidDataFileFormatException("Invalid response from database");
    }

    private async Task<SqlConnection> GetDatabaseConnection(CancellationToken cancellationToken)
    {
        var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        return conn;
    }

    private static IEnumerable<SqlDataRecord> ToSqlDataRecords(IEnumerable<MeterReadingLine> lines)
    {
        foreach (var line in lines)
        {
            var record = new SqlDataRecord(MetaData);

            record.SetInt32(0, line.RowId);

            // AccountId, MeterReadingDateTime, and MeterReadValue can be null if there was an error.
            // We will pass them all to the sproc (to get accurate metrics), but errors will not
            // be inserted into the final table.
            record.SetInt32(1, line.AccountId.GetValueOrDefault());
            record.SetDateTime(2, line.MeterReadingDateTime.GetValueOrDefault());
            record.SetInt32(3, line.MeterReadValue.GetValueOrDefault());

            record.SetInt32(4, (int)line.Status);

            if (string.IsNullOrEmpty(line.Reason))
            {
                record.SetDBNull(5);
            }
            else
            {
                record.SetString(5, line.Reason);
            }

            yield return record;
        }
    }
}