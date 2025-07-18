using CsvHelper;
using System.Globalization;

namespace CPS.ComplexCases.Common.Helpers;

public static class CsvGeneratorHelper
{
    /// <summary>
    /// Generates CSV content from a list of objects using reflection
    /// </summary>
    /// <typeparam name="T">Type of objects to export</typeparam>
    /// <param name="records">List of objects to export</param>
    /// <returns>CSV content as string</returns>
    public static string GenerateCsv<T>(IEnumerable<T> records)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(records);
        return writer.ToString();
    }
}