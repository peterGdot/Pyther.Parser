namespace Pyther.Parser.CSV;

[Flags]
public enum RecordFlags : byte
{
    Unknown = 0,

    /// <summary>Allow to access fields by numeric index</summary>
    Indexed = 0b0000_0001,
    /// <summary>Allow to access fields by associative column name</summary>
    Associative = 0b0000_0010,

    Both = Indexed | Associative
}
