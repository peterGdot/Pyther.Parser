using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Pyther.Parser.CSV;

public class Headers
{
    public List<string> Names { get; set; }
    public Dictionary<string, int> PosByName { get; set; }

    public int Count { get; private set; }

    public Headers()
    {
        Names = new List<string>();
        PosByName = new Dictionary<string, int>();
    }

    public Headers(List<object> row, Settings options)
    {
        Count = row.Count;

        Names = new List<string>(Count);
        PosByName = new Dictionary<string, int>(Count);
        for (int i = 0; i < row.Count; i++)
        {
            string? cell = (row[i].ToString() ?? "").Trim();
            if (options.HeaderTransformMethod != null)
            {
                cell = options.HeaderTransformMethod(cell);
            }
            Names.Add(cell);
            PosByName.Add(cell, i);
        }
    }

    public string? this[int index]
    {
        get => Names != null && index >= 0 && index < Names.Count ? Names[index] : null;
    }

    public int? this[string key]
    {
        get => PosByName != null && PosByName.ContainsKey(key) ? PosByName[key] : null;
    }


    public Headers Add(params string[] names)
    {
        foreach (var name in names)
        {
            this.Names.Add(name);
            PosByName.Add(name, this.Names.Count - 1);
            this.Count++;
        }
        return this;
    }
}
