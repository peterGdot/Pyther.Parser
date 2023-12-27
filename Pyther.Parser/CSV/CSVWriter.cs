using System.Reflection;

namespace Pyther.Parser.CSV
{
    public class CSVWriter : IDisposable
    {
        private readonly Settings options;
        private readonly TextWriter writer;
        
        private bool disposed = false;
        private bool needHeaderWritten = true;

        public Headers Headers { get; }
        
        public long RecordCount { get; private set; }

        public CSVWriter(string filename, Settings? options = null) : this(options)
        {
            try
            {
                writer = new StreamWriter(filename, false, this.options.Encoding, this.options.BufferSize);
            } catch (Exception)
            {
                throw;
            }
        }

        public CSVWriter(StringWriter stringWriter, Settings? options = null) : this(options)
        {
            try
            {
                writer = stringWriter;
            }
            catch (Exception)
            {
                throw;
            }
        }

#pragma warning disable CS8618
        private CSVWriter(Settings? options = null)
#pragma warning restore CS8618
        {
            this.options = options != null ? options.Clone() : new Settings();
            Headers = new Headers();
        }

        ~CSVWriter()
        {
            Dispose(false);
        }

        /// <summary>
        /// Write the headers to the file if not already written and the "HasHeader" option is not set to false.
        /// </summary>
        public void WriteHeader()
        {
            if (needHeaderWritten)
            {
                needHeaderWritten = false;
                if (options.HasHeader && Headers.Count > 0)
                {
                    this.Write(this.Headers.Names);
                    RecordCount--;
                }
            }
        }

        /// <summary>
        /// Write a blank line.
        /// </summary>
        public void WriteEmptyLine()
        {
            writer.Write(Environment.NewLine);
        }

        private void WriteCell(string data, int pos)
        {
            if (pos > 0)
            {
                writer.Write(options.Delimeter);
            }

            if (options.AutoTrim)
            {
                data = data.Trim();
            }

            // need Escape
            if (options.EscapeByDoubling)
            {
                data = data.Replace(options.Enclosure.ToString()!, options.Enclosure.ToString() + options.Enclosure);
            }
            else if (options.Escape != null && options.Enclosure != null) {
                data = data.Replace(options.Enclosure.ToString()!, options.Escape.ToString() + options.Enclosure);
            }

            // does we need enclosure?
            bool needEnclosure = options.ForceEnclosure || data.Contains(options.Delimeter);
            if (needEnclosure)
            {
                data = options.Enclosure + data + options.Enclosure;
            }

            writer.Write(data);
        }

        public void Write(params object?[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                object? cell = cells[i];
                if (cell != null)
                {
                    WriteCell(((IConvertible)cell).ToString(options.FormatProvider) ?? "", i);
                } else
                {
                    WriteCell("", i);
                }
            }
            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        /// <summary>
        /// Write `List` of objects.
        /// </summary>
        /// <param name="row"></param>
        public void Write(List<object> row)
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }
            for (int i = 0; i < row.Count; i++)
            {
                WriteCell(((IConvertible)row[i]).ToString(options.FormatProvider) ?? "", i);
            }
            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        /// Write `List` of strings.
        public void Write(List<string> row)
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }
            for (int i = 0; i < row.Count; i++)
            {
                WriteCell(row[i] ?? "", i);                
            }
            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        /// <summary>
        /// Write Record.
        /// </summary>
        /// <param name="record"></param>
        public void Write(Record record)
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }
            if (record.Flags.HasFlag(RecordFlags.Associative))
            {
                if (Headers == null || Headers.Count == 0)
                {
                    throw new Exception("Associative Record requires valid Headers");
                }
                for (int i = 0; i < Headers.Count; i++)
                {
                    string key = Headers[i]!;
                    object? data = record[key] ?? record[i];
                    if (data != null)
                    {
                        WriteCell(((IConvertible)data)?.ToString(options.FormatProvider) ?? "", i);
                    }
                    else
                    {
                        WriteCell("", i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < record.Count; i++)
                {
                    object? data = record[i];
                    if (data != null)
                    {
                        WriteCell(((IConvertible)data)?.ToString(options.FormatProvider) ?? "", i);
                    } else
                    {
                        WriteCell("", i);
                    }
                }
            }
            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        public void WriteDynamic(dynamic obj)
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }
            var dict = (IDictionary<string, object>)obj;
            for (int i = 0; i < Headers.Count; i++)
            {
                string key = Headers[i]!;
                object? data = dict.ContainsKey(key) ? dict[key] : null;
                if (data != null)
                {
                    WriteCell(((IConvertible)data)?.ToString(options.FormatProvider) ?? "", i);
                }
                else
                {
                    WriteCell("", i);
                }
            }

            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        public void Write<T>(T obj) where T : class
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }

            Type type = typeof(T);

            for (int i = 0; i < Headers.Count; i++)
            {
                string key = Headers[i]!;
                PropertyInfo? pi = type.GetProperty(key);
                object? data = pi?.GetValue(obj, null);
                if (data != null)
                {
                    WriteCell(((IConvertible)data)?.ToString(options.FormatProvider) ?? "", i);
                }
                else
                {
                    WriteCell("", i);
                }
            }

            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }

        public void WriteNested<T>(T obj) where T : class
        {
            if (needHeaderWritten)
            {
                WriteHeader();
            }

            Type type = typeof(T);

            for (int i = 0; i < Headers.Count; i++)
            {
                string header = Headers[i]!;

                var keys = header.Split('.');
                object? data = null;
                object o = obj;
                Type t = type;
                for (int j = 0; j < keys.Length; j++)
                {
                    string? key = keys[j];
                    PropertyInfo? pi = t.GetProperty(key);
                    data = pi?.GetValue(o, null);
                    if (data == null) break;
                    if (j + 1 < keys.Length)
                    {
                        o = data;
                        t = data.GetType();
                    }
                }
                if (data != null)
                {
                    WriteCell(((IConvertible)data)?.ToString(options.FormatProvider) ?? "", i);
                }
                else
                {
                    WriteCell("", i);
                }
            }

            writer.Write(options.RecordSeparator);
            writer.Flush();
            RecordCount++;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // dispose mangaged resources
                    // xxx.Dispose();
                }

                // free unmanaged resources
                writer?.Flush();
                writer?.Dispose();

                disposed = true;
            }
        }

    }
}
