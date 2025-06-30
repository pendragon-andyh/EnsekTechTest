namespace Ensek.Lib.MeterReadingsImport;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient.Server;

public interface IMeterReadingWriter
{
    Task<BulkUploadResponse> MeterReadingBulkLoad(
        MeterReadingBulkUploadRequest request,
        IEnumerable<SqlDataRecord> records,
        CancellationToken cancellationToken);
}