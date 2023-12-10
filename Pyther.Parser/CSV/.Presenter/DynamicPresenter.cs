using System.Dynamic;

namespace Pyther.Parser.CSV
{
    public class DynamicPresenter : BasePresenter
    {
        public override object Convert(List<object> row, Headers? headers, Settings options)
        {
            dynamic obj = new ExpandoObject();
            var dict = (IDictionary<string, object>)obj;
            obj.Count = row.Count;
            for (int i = 0; i < row.Count; i++)
            {
                object? field = row[i];
                string key = (headers?[i]) ?? "Column" + i;
                dict[key] = field;
            }
            return obj;
        }
    }
}
