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
    private readonly string connectionString;

    public MeterReadingWriter(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<BulkUploadResponse> MeterReadingBulkLoad(
        MeterReadingBulkUploadRequest request,
        IEnumerable<SqlDataRecord> records,
        CancellationToken cancellationToken)
    {
        await using var conn = await GetDatabaseConnection(cancellationToken).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.MeterReadingBulkLoad";

        cmd.Parameters.AddWithValue("RequestId", request.RequestId);

        var tvpParam = cmd.Parameters.AddWithValue("@Data", records);
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
}