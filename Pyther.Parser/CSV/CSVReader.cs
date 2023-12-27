using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

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

    #region Row

    public IEnumerable<List<object>> ReadRowFromString(string text)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        foreach (var r in ReadRow(new StreamReader(stream)))
        {
            yield return r;
        }
    }

    public IEnumerable<List<object>> ReadRowFromPath(string path)
    {
        rowId = 0;
        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);
        foreach (var r in ReadRow(reader))
        {
            yield return r;
        }        
    }

    public IEnumerable<List<object>> ReadRow(StreamReader reader)
    {
        rowId = 0;

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
            yield return row;
        }
    }

    #endregion

    #region Record

    public IEnumerable<Record> ReadRecordFromString(string text, RecordFlags flags = RecordFlags.Both, Record? result = null)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        foreach (var r in ReadRecord(new StreamReader(stream), flags, result))
        {
            yield return r;
        }
    }

    public IEnumerable<Record> ReadRecordFromPath(string path, RecordFlags flags = RecordFlags.Both, Record? result = null)
    {
        rowId = 0;
        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);
        foreach (var r in ReadRecord(reader, flags, result))
        {
            yield return r;
        }
    }

    public IEnumerable<Record> ReadRecord(StreamReader reader, RecordFlags flags = RecordFlags.Both, Record? result = null)
    {
        var presenter = new RecordPresenter(flags);
        rowId = 0;

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

    #endregion

    #region Dynamic

    public IEnumerable<dynamic> ReadDynamicFromString(string text)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        foreach (var r in ReadDynamic(new StreamReader(stream)))
        {
            yield return r;
        }
    }

    public IEnumerable<dynamic> ReadDynamicFromPath(string path)
    {
        rowId = 0;
        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);
        foreach (var r in ReadDynamic(reader))
        {
            yield return r;
        }
    }

    public IEnumerable<dynamic> ReadDynamic(StreamReader reader)
    {
        var presenter = new DynamicPresenter();
        rowId = 0;

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

    #endregion

    #region Object

    public IEnumerable<T> ReadObjectFromString<T>(string text, T? obj = null) where T : class
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream);
        writer.Write(text);
        writer.Flush();
        stream.Position = 0;
        foreach (var r in ReadObject<T>(new StreamReader(stream), obj))
        {
            yield return r;
        }
    }

    public IEnumerable<T> ReadObjectFromPath<T>(string path, T? obj = null) where T : class
    {
        rowId = 0;
        using var fileStream = File.OpenRead(path);
        using var reader = new StreamReader(fileStream, options.Encoding, true, options.BufferSize);
        foreach (var r in ReadObject<T>(reader, obj))
        {
            yield return r;
        }
    }

    public IEnumerable<T> ReadObject<T>(StreamReader reader, T? obj = null) where T: class
    {
        var presenter = new ObjectPresenter<T>();
        rowId = 0;

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

    #endregion

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
