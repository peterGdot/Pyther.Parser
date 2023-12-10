using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyther.Parser.CSV;

public abstract class BasePresenter
{
    public abstract object Convert(List<object> row, Headers? headers, Settings options);
}
