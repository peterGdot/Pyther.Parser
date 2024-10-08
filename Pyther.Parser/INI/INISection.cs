using System.Collections;
using System.Collections.Specialized;

namespace Pyther.Parser.INI
{
    public class INISection
    {
        /// <summary>The INI File this section belongs to</summary>
        public INIFile INIFile { get; }
        private OrderedDictionary data;
        private int commentId = 0;
        internal List<string> lines = new List<string>();

        public string Name { get; private set; }
        public List<string> Lines => lines;

        public INISection(INIFile iniFile, string name)
        {
            this.INIFile = iniFile;
            this.Name = name;
            data = iniFile.config.UseCaseSensitive ? new OrderedDictionary() : new OrderedDictionary(StringComparer.OrdinalIgnoreCase);
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public void Set(string key, string value)
        {
            data[key] = value;
        }

        public void AddComment(string raw)
        {
            string key = ".raw{" + commentId + "}";
            // Set(key, INIFile.config.DefaultComment + raw);
            Set(key, raw);
            commentId++;
        }

        public String? Get(string key)
        {
            return this.data.Contains(key) ? this.data[key]?.ToString() : null;
        }

        public IEnumerable<KeyValuePair<string, string>> Entries(bool includeComments = false)
        {
            foreach (string key in this.data.Keys)
            {
                if (!key.StartsWith(".raw{") || includeComments)
                {
                    yield return new KeyValuePair<string, string>(key, this.data[key]?.ToString() ?? "");
                }
            }
        }

        public IEnumerable<string> Keys(bool includeComments = false)
        {
            foreach (string key in this.data.Keys)
            {
                if (!key.StartsWith(".raw{") || includeComments)
                {
                    yield return key;
                }
            }
        }

        public IEnumerable<string> Values(bool includeComments = false)
        {
            foreach (string key in this.data.Keys)
            {
                if (!key.StartsWith(".raw{") || includeComments)
                {
                    yield return this.data[key]?.ToString() ?? "";
                }
            }
        }

        public void Save(StreamWriter sw)
        {
            if (this.Name != ".intro")
            {
                sw.WriteLine("[" + this.Name + "]");
            }
            foreach (DictionaryEntry row in data)
            {
                string key = row.Key.ToString() ?? "";
                if (key.StartsWith(".raw{"))
                {
                    string value = row.Value?.ToString() ?? "";
                    value = value[(value.IndexOf('}') + 1)..];
                    sw.WriteLine(value);
                }
                else
                {
                    sw.WriteLine(key + INIFile.config.KeyValueWriteDelimiter + row.Value);
                }
            }
            sw.WriteLine("");
        }


    }
}
