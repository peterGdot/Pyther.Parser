using System.Globalization;
using System.Text;

namespace Pyther.Parser.CSV;

public class Settings
{
    // IO
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public int BufferSize { get; set; } = 1024 * 1024;

    // Parser
    public string RecordSeparator { get; set; } = Environment.NewLine;
    public char Delimeter { get; set; } = ',';
    public char? Enclosure { get; set; } = '"';
    public char? Escape { get; set; } = '\\';
    public bool EscapeByDoubling { get; set; } = false;
    public bool ForceEnclosure { get; set; } = false;


    // Content
    public bool HasHeader { get; set; } = true;
    public Func<string, string>? HeaderTransformMethod { get; set; }
    public Func<object, int, string?, object>? CellTransformMethod { get; set; }   // (cell data, column index, column name) -> object
    public bool AutoTrim { get; set; } = true;
    public bool IgnoreEmptyLines { get; set; } = true;
    public IFormatProvider? FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    // Error Handling
    public ErrorHandling ErrorToManyColumns { get; set; } = ErrorHandling.TryToSolve; // more columns than header
    public ErrorHandling ErrorToFewColumns { get; set; } = ErrorHandling.TryToSolve; // less columns than header
    public ErrorHandling ErrorInvalidClassProperty { get; set; } = ErrorHandling.TryToSolve; // object doesn't contain the given property
 
    internal Settings Clone()
    {
        return (Settings)this.MemberwiseClone();
    }
}
