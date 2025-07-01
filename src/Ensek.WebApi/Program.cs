namespace Ensek.WebApi;

using Serilog;
using System.Text;
using Ensek.Lib;
using Ensek.Lib.MeterReadingsImport;

public class Program
{
    public static Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog  ... appsettings.json tells it to log to the console in its "compressed json" format.
        // My current company have implemented their own JSON formatter which outputs in a format that is more
        // friendly to DataDog.
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("service", "Ensek.WebApi")
            .CreateLogger();
        Log.Information("Starting WebApi");

        var connectionString = builder.Configuration.GetConnectionString("EnsekDb")
                               ?? throw new ConfigurationException("Missing 'EnsekDb' connection string");

        builder.Services
            .AddLogging(x =>
            {
                // Attach MS logging to Serilog.
                x.ClearProviders();
                x.AddSerilog(Log.Logger);
            })
            .AddAuthorization()
            .AddSingleton<IMeterReadingWriter>(new MeterReadingWriter(connectionString))
            .AddSingleton<MeterReadingUploadRequestHandler>();

        var app = builder.Build();
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapPost("/meter-reading-uploads", async (HttpContext httpContext, MeterReadingUploadRequestHandler handler, bool isValidationOnlyMode = false) =>
        {
            var request = new MeterReadingBulkUploadRequest
                {
                    RequestId = Guid.NewGuid(),
                    IsValidationOnlyMode = isValidationOnlyMode
                };
            using var uploadReader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
            var response = await handler.Handle(request, uploadReader, httpContext.RequestAborted).ConfigureAwait(false);

            return response;
        });

        return app.RunAsync();
    }
}