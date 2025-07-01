namespace Ensek.Lib.MeterReadingsImport;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IMeterReadingWriter
{
    Task<BulkUploadResponse> MeterReadingBulkLoad(
        MeterReadingBulkUploadRequest request,
        IEnumerable<MeterReadingLine> meterReadings,
        CancellationToken cancellationToken);
}