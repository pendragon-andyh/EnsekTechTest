namespace Ensek.Lib.MeterReadingsImport;

using System.Threading;
using System.Threading.Tasks;
using Ensek.Lib.Extensions;
using Microsoft.Extensions.Logging;

public class MeterReadingUploadRequestHandler
{
    private readonly IMeterReadingWriter enrichmentWriter;
    private readonly ILogger<MeterReadingUploadRequestHandler> logger;

    public MeterReadingUploadRequestHandler(IMeterReadingWriter enrichmentWriter, ILogger<MeterReadingUploadRequestHandler> logger)
    {
        this.enrichmentWriter = enrichmentWriter;
        this.logger = logger;
    }

    public async Task<BulkUploadResponse> Handle(
        MeterReadingBulkUploadRequest request,
        TextReader inputStreamReader,
        CancellationToken cancellationToken)
    {
        var commaSeparatedRecordEnumerator = new MeterReadingRecordEnumerator(inputStreamReader.ReadLinesIntoAsyncEnumerable(), request.MaxErrorCount);
        try
        {
            var producerTask = commaSeparatedRecordEnumerator.GetDataRecords(cancellationToken).CreateProducerTask(64, out var consumerEnumerable, cancellationToken);
            var consumerTask = enrichmentWriter.MeterReadingBulkLoad(request, consumerEnumerable, cancellationToken);
            await Task.WhenAll(producerTask, consumerTask);

            return await consumerTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error while importing meter readings ({ex.Message})");
            throw;
        }
    }
}