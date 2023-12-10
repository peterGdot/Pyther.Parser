using System;

namespace Pyther.Parser.CSV;

public class CSVReader
{
    private readonly Settings options;
    private readonly StreamParser parser;
    private Headers? headers;

    private long rowId;
    public long RowId => rowId;
    public Headers? Headers => headers;

    public CSVReader(Settings? options = null)
    {
        this.options = options != null ? options.Clone() : new Settings();
        this.parser = new StreamParser(this.options);
    }

    public IEnumerable<List<object>> ReadRow(string path)
    {
        rowId = 0;

        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);

        if (this.options.HasHeader)
        {
            var row = parser.ReadRow(reader, ref rowId) ?? throw new Exception("Header not found!");
            this.headers = new Headers(row, options);
        }
        else
        {
            this.headers = null;
        }

        while (parser.ReadRow(reader, ref rowId) is List<object> row)
        {
            CheckErrors(row);
            if (options.CellTransformMethod != null) {
                ApplyCellTransform(row);
            }
            yield return row;
        }
    }

    public IEnumerable<Record> ReadRecord(string path, RecordFlags flags = RecordFlags.Both, Record? result = null)
    {
        var presenter = new RecordPresenter(flags);
        rowId = 0;

        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);

        if (this.options.HasHeader)
        {
            var row = parser.ReadRow(reader, ref rowId) ?? throw new Exception("Header not found!");
            this.headers = new Headers(row, options);
        } else
        {
            this.headers = null;
        }

        while (parser.ReadRow(reader, ref rowId) is List<object> row)
        {
            CheckErrors(row);
            if (options.CellTransformMethod != null)
            {
                ApplyCellTransform(row);
            }
            yield return (Record)presenter.Convert(row, headers, result);
        }
    }

    public IEnumerable<dynamic> ReadDynamic(string path)
    {
        var presenter = new DynamicPresenter();
        rowId = 0;

        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);

        if (this.options.HasHeader)
        {
            var row = parser.ReadRow(reader, ref rowId) ?? throw new Exception("Header not found!");
            this.headers = new Headers(row, options);
        }
        else
        {
            this.headers = null;
        }

        while (parser.ReadRow(reader, ref rowId) is List<object> row)
        {
            CheckErrors(row);
            if (options.CellTransformMethod != null)
            {
                ApplyCellTransform(row);
            }
            yield return (dynamic)presenter.Convert(row, headers, options);
        }
    }

    public IEnumerable<T> ReadObject<T>(string path, T? obj = null) where T: class
    {
        var presenter = new ObjectPresenter<T>();
        rowId = 0;

        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);

        if (this.options.HasHeader)
        {
            var row = parser.ReadRow(reader, ref rowId) ?? throw new Exception("Header not found!");
            this.headers = new Headers(row, options);
        }
        else
        {
            this.headers = null;
        }

        while (parser.ReadRow(reader, ref rowId) is List<object> row)
        {
            CheckErrors(row);
            if (options.CellTransformMethod != null)
            {
                ApplyCellTransform(row);
            }
            yield return (T)presenter.Convert(row, headers, options, obj);
        }
    }

    public void ApplyCellTransform(List<object> row)
    {
        if (options.CellTransformMethod == null)
        {
            return;
        }
        for (int i = 0; i < row.Count; i++)
        {
            object? cell = row[i];
            row[i] = options.CellTransformMethod(cell, i, headers?[i]);
        }
    }

    private void CheckErrors(List<object> row)
    {
        if (headers != null)
        {
            // to many columns?
            if (row.Count > headers.Count)
            {
                switch (options.ErrorToManyColumns)
                {
                    case ErrorHandling.Throw:
                        throw new ParseException(RowId, $"The row contains to many columns ({row.Count} instead of {headers.Count})!");
                    case ErrorHandling.TryToSolve:
                        row.RemoveRange(headers.Count, row.Count - headers.Count);
                        break;
                    default:
                        break;
                }
            }
            // to few columns?
            else if (row.Count < headers.Count)
            {
                switch (options.ErrorToFewColumns)
                {
                    case ErrorHandling.Throw:
                        throw new ParseException(RowId, $"The row contains to few columns ({row.Count} instead of {headers.Count})!");
                    case ErrorHandling.TryToSolve:
                        for (int i = row.Count; i < headers.Count; i++) row.Add("");
                        break;
                    default:
                        break;
                }
            }
        }
    }

}
