namespace Pyther.Parser.CSV;

public class ParseException : Exception
{
    public long RowId { get; set; }

    public ParseException(long rowId)
    {
        RowId = rowId;
    }

    public ParseException(long rowId, string message) : base(message)
    {
        RowId = rowId;
    }

    public ParseException(long rowId, string message, Exception inner) : base(message, inner)
    {
        RowId = rowId;
    }
}
