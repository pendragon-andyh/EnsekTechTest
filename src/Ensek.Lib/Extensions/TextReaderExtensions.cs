namespace Ensek.Lib.Extensions;

using System.Collections.Generic;
using System.IO;

public static class TextReaderExtensions
{
    public static async IAsyncEnumerable<string> ReadLinesIntoAsyncEnumerable(this TextReader textReader)
    {
        var lineCount = 0;
        while (await textReader.ReadLineAsync() is { } line)
        {
            lineCount++;
            foreach (var ch in line)
            {
                if (ch < ' ')
                {
                    throw new InvalidDataFileFormatException($"Found non-text character on line {lineCount}");
                }
            }

            yield return line;
        }
    }
}