namespace Pyther.Parser.CSV;

public class Record
{
    public int Count { get; internal set; }
    internal List<object?>? Fields { get; set; }
    internal Dictionary<string, object?>? Dictionary { get; set; }

    public RecordFlags Flags {
        get
        {
            RecordFlags flags = RecordFlags.Unknown;
            flags |= Fields != null ? RecordFlags.Indexed : 0;
            flags |= Dictionary != null ? RecordFlags.Associative : 0;
            return flags;
        }
    }

    public Record(int count = 0)
    {
        Count = count;
    }

    public object? this[int index]
    {
        get => Fields != null && index >= 0 && index < Fields.Count ? Fields[index] : null;
        set
        {
            if (index < 0) throw new IndexOutOfRangeException();
            Fields ??= Count > 0 ? new(Count) : new();
            if (index >= Fields.Count)
            {
                ResizeList(Fields, index + 1);
            }
            Fields[index] = value;
            Count = Fields.Count;
        }
    }

    public object? this[string key]
    {
        get => Dictionary != null && Dictionary.ContainsKey(key) ? Dictionary[key] : null;
        set
        {
            Dictionary ??= Count > 0 ? new(Count) : new();
            Dictionary[key] = value;
        }
    }

    private static void ResizeList(List<object?> list, int newSize, object? fill = null)
    {
        int oldCount = list.Count;

        if (newSize < oldCount)
        {
            list.RemoveRange(newSize, oldCount - newSize);
        }
        else if (newSize > oldCount)
        {
            if (newSize > list.Capacity)
            {
                list.Capacity = newSize;
            }
            list.AddRange(Enumerable.Repeat(fill, newSize - oldCount));
        }
    }
}
