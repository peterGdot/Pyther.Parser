using System.Globalization;

namespace Pyther.Parser.INI
{
    /// <summary>
    /// An INI File reader and writer.
    /// Features:
    ///  - Global properties (properties without section)
    ///  - Case (in)sensitive (both supported)
    ///  - Comments (starting with ";")
    ///  - Keep comments between load and save
    ///  - Keep ordering between load and save
    /// @TODO:
    ///  - handle new line "\EOL" as line break
    /// </summary>
    public class INIFile
    {
        internal INIConfig config;

        /// <summary>Get the current ini file path.</summary>
        public string? Path { get; private set; }
        // list of all sections
        private List<INISection> sectionList = new();
        // dictionary for all sections
        private Dictionary<string, INISection> sectionDict;

        public INIFile(INIConfig? config)
        {
            this.config = config ?? new INIConfig();
            sectionDict = this.config.UseCaseSensitive ? new Dictionary<string, INISection>() : new Dictionary<string, INISection>(StringComparer.OrdinalIgnoreCase);
        }

        public INIFile() : this(null)
        {
        }

        /// <summary>
        /// Load an ini file from given path.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Exception"></exception>
        public void Load(string path)
        {
            Path = path;
            if (!File.Exists(path))
            {
                throw new Exception("INI File '" + Path + "' not found!");
            }

            INISection section = AddSection(".intro");
            string[] lines = File.ReadAllLines(Path);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (String.IsNullOrWhiteSpace(line))
                {
                    if (config.IgnoreEmptyLines)
                    {
                        continue;
                    }
                    section.AddComment(line);
                }
                else if (Array.IndexOf(config.CommentIndication, line[0]) != -1)
                {
                    section.AddComment(line);
                }
                // is section ?
                else if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string name = line.Substring(1, line.Length - 2);
                    section = AddSection(name);
                }
                // key = value pairs
                else
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        section.lines.Add(line);
                    }
                    string[] kv = line.Split(new char[] { config.KeyValueReadDelimiter }, 2);
                    section.Set(kv[0].Trim(), kv.Length > 1 ? kv[1].Trim() : "");                    
                }
            }
        }

        /// <summary>
        /// Save the current ini file.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Exception"></exception>
        public void Save(string? path = null)
        {
            path = String.IsNullOrWhiteSpace(path) ? Path : path;
            if (path == null)
            {
                throw new Exception("No path to save given!");
            }

            using (StreamWriter sw = File.CreateText(path))
            {
                foreach (var section in sectionList)
                {
                    section.Save(sw);
                }
            }
        }

        #region Section

        /// <summary>
        /// Check if a section of given name already exists.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasSection(string name)
        {
            return this.sectionDict.ContainsKey(name);
        }

        /// <summary>
        /// Add a new section to the ini file. If a section of this name already exists, return it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The new or existing section.</returns>
        public INISection AddSection(string name)
        {
            if (this.sectionDict.ContainsKey(name))
            {
                return this.sectionDict[name];
            }
            INISection section = new INISection(this, name);
            this.sectionList.Add(section);
            this.sectionDict.Add(name, section);
            return section;
        }

        /// <summary>
        /// Get a section and optional create it, if it doesn't exists.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="createIfNotExists"></param>
        /// <returns></returns>
        public INISection? GetSection(string sectionName, bool createIfNotExists = true)
        {
            if (!this.sectionDict.ContainsKey(sectionName))
            {
                return createIfNotExists ? AddSection(sectionName) : null;
            }
            return this.sectionDict[sectionName];
        }

        /// <summary>
        /// Clear a section by removing all Key/Value pairs and comments.
        /// </summary>
        /// <param name="sectionName"></param>
        public void ClearSection(string sectionName)
        {
            if (this.sectionDict.ContainsKey(sectionName))
            {
                this.sectionDict[sectionName].Clear();
            }
        }

        #endregion

        #region Entries

        /// <summary>
        /// Returns true, if the ini file has the given section and key.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Has(string? sectionName, string key)
        {
            sectionName ??= ".intro";
            return this.sectionDict.ContainsKey(sectionName) && this.sectionDict[sectionName].Get(key) != null;
        }
        
        /// <summary>
        /// Returns true, if the ini file has the given section and key and the value is not an empty string.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasValue(string? sectionName, string key)
        {
            sectionName ??= ".intro";
            return this.sectionDict.ContainsKey(sectionName) && !string.IsNullOrWhiteSpace(this.sectionDict[sectionName].Get(key) ?? null);
        }

        /// <summary>
        /// Get a section string value by section and key.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <returns></returns>
        public string? Get(string? sectionName, string key)
        {
            sectionName ??= ".intro";
            return this.sectionDict.ContainsKey(sectionName) ? this.sectionDict[sectionName].Get(key) : null;
        }

        /// <summary>
        /// Get a section int value by section and key.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <returns></returns>
        public int? GetInt(string? sectionName, string key)
        {
            string? val = this.Get(sectionName, key);
            return val != null && int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out int result) ? result : null;
        }

        public long? GetLong(string? sectionName, string key)
        {
            string? val = this.Get(sectionName, key);
            return val != null && long.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out long result) ? result : null;
        }

        public float? GetFloat(string? sectionName, string key)
        {
            string? val = this.Get(sectionName, key);
            return val != null && float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : null;
        }
        public double? GetDouble(string? sectionName, string key)
        {
            string? val = this.Get(sectionName, key);
            return val != null && double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : null;
        }

        /// <summary>
        /// Get a section bool value by section and key.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <param name="fallback">Opional fallback value.</param>
        /// <returns></returns>
        public bool? GetBool(string? sectionName, string key)
        {
            var str = this.Get(sectionName, key)?.ToLower();
            return str switch
            {
                "true" or "on" or "1" => true,
                null => null,
                _ => false
            };
        }

        /// <summary>
        /// Set an entry value.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <param name="value">The entries value to set.</param>
        public void Set(string? sectionName, string key, string value)
        {
            if (sectionName == null)
            {
                sectionName = ".intro";
            }
            INISection section = this.sectionDict.ContainsKey(sectionName) ? this.sectionDict[sectionName] : AddSection(sectionName);
            section.Set(key, value);
        }

        /// <summary>
        /// Set an entry value.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <param name="value">The entries value to set.</param>
        public void Set(string? sectionName, string key, int value)
        {
            if (sectionName == null)
            {
                sectionName = ".intro";
            }
            this.Set(sectionName, key, value.ToString());
        }

        /// <summary>
        /// Set an entry value.
        /// </summary>
        /// <param name="sectionName">The name of the section or null for global values.</param>
        /// <param name="key">The entries key.</param>
        /// <param name="value">The entries value to set.</param>
        public void Set(string? sectionName, string key, bool value)
        {
            if (sectionName == null)
            {
                sectionName = ".intro";
            }
            this.Set(sectionName, key, value ? "true" : "false");
        }

        #endregion

    }
}
