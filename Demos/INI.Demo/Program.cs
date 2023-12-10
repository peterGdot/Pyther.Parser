using Pyther.Parser.INI;

internal class Program
{
    private static void Main()
    {
        INIFile ini = new(new INIConfig()
        {
            UseCaseSensitive = false,
            IgnoreEmptyLines = false
        });

        ini.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "example.ini"));

        Console.WriteLine("AppName = " + ini.Get("", "AppName"));
        Console.WriteLine("[database].user = " + ini.Get("database", "user"));
        Console.WriteLine("[DataBase].User = " + ini.Get("DataBase", "User"));

        ini.Set("app", "counter", (ini.GetInt("app", "counter") ?? 0) + 1);

        if (ini.HasSection("database"))
        {
            foreach (bool includeComments in new[] { false, true })
            {
                Console.WriteLine("ini.GetSection(\"database\")!.Entries({0}):", includeComments);
                foreach (KeyValuePair<string, string> x in ini.GetSection("database")!.Entries(includeComments))
                {
                    Console.WriteLine(" - " + x.Key + " = " + x.Value);
                }
                Console.WriteLine("ini.GetSection(\"database\")!.Keys({0}):", includeComments);
                foreach (string x in ini.GetSection("database")!.Keys(includeComments))
                {
                    Console.WriteLine(" - " + x);
                }
                Console.WriteLine("ini.GetSection(\"database\")!.Values({0}):", includeComments);
                foreach (string x in ini.GetSection("database")!.Values(includeComments))
                {
                    Console.WriteLine(" - " + x);
                }
            }
        }


        ini.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "example2.ini"));
    }
}

