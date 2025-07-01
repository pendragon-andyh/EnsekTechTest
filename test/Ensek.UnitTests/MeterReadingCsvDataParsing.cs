namespace Ensek.UnitTests;

using System.Globalization;
using Ensek.Lib.MeterReadingsImport;

public class MeterReadingCsvDataParsing
{
    [Theory]
    [InlineData("", null, null, null, MeterReadingLineStatus.Error)]
    [InlineData("1,30/10/2019 10:00,123", 1, "2019-10-30 10:00", 123, MeterReadingLineStatus.Ok)]
    [InlineData("1,30/10/2019 10:00,", null, null, null, MeterReadingLineStatus.Error)]
    [InlineData("321,30/10/2019 10:00,123, x", 321, "2019-10-30 10:00", 123, MeterReadingLineStatus.Warning)]
    [InlineData("1,10/30/2019 10:00,123", 1, null, 123, MeterReadingLineStatus.Error)]
    [InlineData("1,30/10/2019 10:00,-123", 1, "2019-10-30 10:00", null, MeterReadingLineStatus.Error)]
    [InlineData("1,30/10/2019 10:00,123456", 1, "2019-10-30 10:00", null, MeterReadingLineStatus.Error)]
    public void MeterReadingLineConstructorTheory(string rawData, int? accountId, string? readingIsoDateText, int? readingValue, MeterReadingLineStatus expectedStatus)
    {
        var columnData = rawData.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var line = new MeterReadingLine(1, rawData, columnData);

        var readingDate = default(DateTime?);
        if (!string.IsNullOrEmpty(readingIsoDateText))
        {
            readingDate = DateTime.Parse(readingIsoDateText, CultureInfo.InvariantCulture);
        }

        Assert.Equal(accountId, line.AccountId);
        Assert.Equal(readingDate, line.MeterReadingDateTime);
        Assert.Equal(readingValue, line.MeterReadValue);
        Assert.Equal(expectedStatus, line.Status);
    }
}