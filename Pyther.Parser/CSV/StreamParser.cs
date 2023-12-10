using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pyther.Parser.CSV;

public class StreamParser
{
    private Settings options;
    private bool isInEnclosure = false;
    private bool isInEscape = false;
    private int recordSeparatorMatch = 0;
    private char[] field;
    private int fieldPos = 0;
    private List<object> row = new();

    public StreamParser(Settings? options)
    {
        this.options = options ?? new Settings();
        this.field = new char[1024];
    }

    internal List<object>? ReadRow(StreamReader reader, ref long RowId)
    {
        if (reader.EndOfStream)
        {
            return null;
        }

        // a) this is the optimized path using split, if Enclosure is null and RecordSeparator = NewLine and Escape = null
        if (options.Enclosure == null && options.RecordSeparator == Environment.NewLine && options.Escape == null)
        {
            while (true)
            {
                row.Clear();
                string? line = reader.ReadLine();
                RowId++;
                if (line == null)
                {
                    return null;
                }
                if (options.IgnoreEmptyLines && line.Trim() == "")
                {
                    continue;
                }
                var parts = line.Split(options.Delimeter, options.AutoTrim ? StringSplitOptions.TrimEntries : StringSplitOptions.None);
                foreach (var part in parts)
                {
                    row.Add(part);
                }
                return row;
            }            
        }

        // b) normal path

        // reset
        row.Clear();
        isInEnclosure = false;
        isInEscape = false;
        recordSeparatorMatch = 0;
        fieldPos = 0;

        char? prev;
        char ch = 'Q';
        while (!reader.EndOfStream)
        {
            prev = ch;
            ch = (char)reader.Read();

            // Enclosure?
            if (ch == options.Enclosure)
            {
                if (!options.EscapeByDoubling)
                {
                    if (!isInEscape)
                    {
                        isInEnclosure = !isInEnclosure;
                        continue;
                    }
                }
                else
                {
                    if (prev == options.Enclosure)
                    {
                        AddCharToField(ch);
                    }
                    isInEnclosure = !isInEnclosure;
                    continue;
                }
            }

            // EOR
            if (!isInEnclosure && options.RecordSeparator[recordSeparatorMatch] == ch)
            {
                recordSeparatorMatch++;
                if (recordSeparatorMatch == options.RecordSeparator.Length)
                {
                    RowId++;
                    // EMPTY row?
                    if (options.IgnoreEmptyLines && fieldPos == 0 && row.Count == 0)
                    {
                        recordSeparatorMatch = 0;
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                continue;
            }
            recordSeparatorMatch = 0;

            if (!isInEscape && ch == options.Escape)
            {
                isInEscape = true;
                continue;
            }
            isInEscape = false;

            // Delimeter
            if (!isInEnclosure && ch == options.Delimeter)
            {
                AddFieldToRow(ref row);
                continue;
            }
            AddCharToField(ch);
        }
        if (options.IgnoreEmptyLines && row.Count == 0 && fieldPos == 0)
        {
            return null;
        }
        AddFieldToRow(ref row);
        return row;
    }

    private void AddCharToField(char ch)
    {
        if (fieldPos + 1 == field.Length)
        {
            Array.Resize(ref field, field.Length + 1024);
        }
        field[fieldPos++] = ch;
    }

    private void AddFieldToRow(ref List<object> record)
    {
        var str = new string(field, 0, fieldPos);
        record.Add(options.AutoTrim ? str.Trim() : str);
        fieldPos = 0;
    }

}
