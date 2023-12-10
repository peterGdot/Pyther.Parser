using Pyther.Parser.CSV;
using System.Diagnostics;
using System.Dynamic;

internal class Program
{
    class Order
    {
        public string? OrderId { get; set; }
        public string? CustomerFirstname { get; set; }
        public string? CustomerLastname { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime DateOfPurchase { get; set; }
        public string? Remark { get; set; }
    }

    class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name => LastName + ", " + FirstName;
        public double Height { get; set; }
        public string? Remarks { get; set; }
    }

    private static void Main()
    {
        ReaderDemo();        
        ErrorDemo();
        WriteDemo();
    }

    private static object MyCellTransform(object data, int columnIndex, string? columnName)
    {
        return columnName switch
        {
            "DateOfPurchase" => DateTime.Parse((string)data).ToUniversalTime(),
            _ => data,
        };
    }

    private static void ReaderDemo()
    {
        Console.WriteLine("== CSV Reader Demo ========");

        var csv = new CSVReader(new Settings()
        {
            Delimeter = ',',
            Enclosure = '"',
            HeaderTransformMethod = TransformMethods.Auto,
        });
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "orders.csv");


        Console.WriteLine($"{Environment.NewLine}Read as row:");
        foreach (var row in csv.ReadRow(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {row[0]} / {row[1]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as record:");
        foreach (var record in csv.ReadRecord(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {record[0]} / {record["CustomerLastname"]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as dynamic:");
        foreach (var obj in csv.ReadDynamic(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {obj.OrderId} / {obj.CustomerPhone}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as object Order:");
        foreach (var order in csv.ReadObject<Order>(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {order.OrderId} / {order.DateOfPurchase}");
        }

        Order myOrder = new();
        Console.WriteLine($"{Environment.NewLine}Read as object Order:");
        foreach (var _ in csv.ReadObject(path, myOrder))
        {
            Console.WriteLine($"{csv.RowId,3} | {myOrder.OrderId} / {myOrder.DateOfPurchase}");
        }
    }

    private static void ErrorDemo()
    {
        Console.WriteLine("== CSV Error Demo ========");

        try
        {
            var csv = new CSVReader(new Settings()
            {
                Delimeter = ',',
                Enclosure = '"',
                HeaderTransformMethod = TransformMethods.KebabCaseToTitleCase,
                CellTransformMethod = MyCellTransform
            });
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "errors.csv");

            Console.WriteLine($"{Environment.NewLine}Read as object Order:");
            foreach (var order in csv.ReadObject<Order>(path))
            {
                Console.WriteLine($"{csv.RowId,3} | {order.OrderId} / {order.DateOfPurchase}");
            }
        } catch (ParseException ex)
        {
            Console.WriteLine($"CSV Parse error on row #{ex.RowId}: {ex.Message}");
        }
    }

    private static void WriteDemo()
    {
        Console.WriteLine("== CSV Write Demo ========");

        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "written.csv");

        var csv = new CSVWriter(path, new Settings());

        csv.Headers.Add("Name", "FirstName", "LastName").Add("Height", "Remarks");

        // optional: write headers by hand
        csv.WriteHeader();
        csv.WriteEmptyLine();

        // Write by arguments
        csv.Write("Parker, Peter", "Peter", "Parker", 175.3, "Arguments example");

        // Write Rows
        var row1 = new List<object>{"Parker, Peter", "Peter", "Parker", 175.3, "List of objects example" };
        csv.Write(row1);

        var row2 = new List<string>{"Parker, Peter", "Peter", "Parker", "175.3", "List of string example" };
        csv.Write(row2);

        // Write Records
        var rec1 = new Record();
        rec1[0] = "Parker, Peter";
        rec1[1] = "Peter";
        rec1[2] = "Parker";
        rec1[3] = 175.3;
        rec1[4] = "List of indexed Record example";
        csv.Write(rec1);

        var rec2 = new Record();
        rec2["Name"] = "Parker, Peter";
        rec2[1] = "Peter";
        rec2["Height"] = 175.3;
        rec2["LastName"] = "Parker";
        rec2["Remarks"] = "List of mixed Record example";
        csv.Write(rec2);

        // Write dynamics
        dynamic obj = new ExpandoObject();
        obj.Name = "Parker, Peter";
        obj.FirstName = "Peter";
        obj.LastName = "Parker";
        obj.Height = 175.3;
        obj.Remarks = "dynamic object example";
        csv.WriteDynamic(obj);

        // Write Object
        var person = new Person()
        {
            FirstName = "Peter",
            LastName = "Parker",
            Height = 175.3,
            Remarks = "custom object example"
        };
        csv.Write(person);

        csv.Dispose();
    }


}