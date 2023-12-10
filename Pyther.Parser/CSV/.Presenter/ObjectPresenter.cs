using System.Reflection;

namespace Pyther.Parser.CSV;

public class ObjectPresenter<T> : BasePresenter where T : class
{
    private List<PropertyInfo?>? propInfoCache;

    public override object Convert(List<object> row, Headers? headers, Settings options)
    {
        return Convert(row, headers, options, null);
    }

    public object Convert(List<object> row, Headers? headers, Settings options, T? result)
    {
        if (propInfoCache == null)
        {
            BuildPropInfoCache(row, headers);
        }

        T obj = result ?? Activator.CreateInstance(typeof(T)) as T ?? throw new Exception("Can't create object");

        for (int i = 0; i < row.Count; i++)
        {
            PropertyInfo? prop = propInfoCache![i];
            if (prop != null)
            {
                var val = System.Convert.ChangeType(row[i], prop.PropertyType);
                prop.SetValue(obj, val, null);
            } else
            {
                switch (options.ErrorInvalidClassProperty)
                {
                    case ErrorHandling.Throw:
                        throw new Exception($"Class \"{typeof(T).FullName}\" doesn't contain \"{headers?[i]}\" property!");
                    default: 
                        break;
                }
            }
        }
        return obj;
    }


    public void BuildPropInfoCache(List<object> row, Headers? headers)
    {
        propInfoCache = new(row.Count);
        for (int i = 0; i < row.Count; i++)
        {
            string key = (headers?[i]) ?? "Column" + i;
            PropertyInfo? prop = typeof(T).GetProperty(key, BindingFlags.Public | BindingFlags.Instance);
            propInfoCache.Add(prop != null && prop.CanWrite ? prop : null);
        }
    }

}