using CsvHelper;
using System.Globalization;

namespace CPS.ComplexCases.Common.Helpers;

public static class CsvGeneratorHelper
{
    /// <summary>
    /// Generates CSV content from a list of dictionaries
    /// </summary>
    /// <param name="records">List of records where each record is a dictionary of column name to value</param>
    /// <returns>CSV content as string</returns>
    public static string GenerateCsv(IEnumerable<Dictionary<string, object?>> records)
    {
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        var recordList = records.ToList();
        if (!recordList.Any())
            return string.Empty;

        var allColumns = recordList.SelectMany(r => r.Keys).Distinct().ToList();

        foreach (var column in allColumns)
        {
            csv.WriteField(column);
        }
        csv.NextRecord();

        foreach (var record in recordList)
        {
            foreach (var column in allColumns)
            {
                var value = record.ContainsKey(column) ? record[column]?.ToString() : string.Empty;
                csv.WriteField(value);
            }
            csv.NextRecord();
        }

        return writer.ToString();
    }

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