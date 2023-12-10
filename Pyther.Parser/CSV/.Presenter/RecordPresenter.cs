using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Pyther.Parser.CSV;

public class RecordPresenter : BasePresenter
{
    private readonly RecordFlags flags;

    public RecordPresenter(RecordFlags flags)
    {
        this.flags = flags;
    }

    public override object Convert(List<object> row, Headers? headers, Settings options)
    {
        return Convert(row, headers, null);
    }

    public object Convert(List<object> row, Headers? headers, Record? result)
    {
        bool isIndexed = headers == null || flags.HasFlag(RecordFlags.Indexed);
        bool isAssociative = flags.HasFlag(RecordFlags.Associative) && headers != null;

        var record = result ?? new Record(row.Count);

        if (isIndexed)
        {
            record.Fields = row;
        }

        if (isAssociative)
        {
            record.Dictionary = new(record.Count);
            for (int i = 0; i < row.Count; i++)
            {
                object? field = row[i];
                record.Dictionary.Add(headers?[i]!, field);
            }
        }
        return record;
    }
}
