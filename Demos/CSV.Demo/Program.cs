using Pyther.Parser.CSV;
using System.Diagnostics;
using System.Dynamic;
using System.IO;

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
        WriteStream();
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
        foreach (var row in csv.ReadRowFromPath(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {row[0]} / {row[1]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as record:");
        foreach (var record in csv.ReadRecordFromPath(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {record[0]} / {record["CustomerLastname"]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as dynamic:");
        foreach (var obj in csv.ReadDynamicFromPath(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {obj.OrderId} / {obj.CustomerPhone}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as object Order:");
        foreach (var order in csv.ReadObjectFromPath<Order>(path).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {order.OrderId} / {order.DateOfPurchase}");
        }

        Order myOrder = new();
        Console.WriteLine($"{Environment.NewLine}Read as object Order:");
        foreach (var _ in csv.ReadObjectFromPath(path, myOrder))
        {
            Console.WriteLine($"{csv.RowId,3} | {myOrder.OrderId} / {myOrder.DateOfPurchase}");
        }

        string text = "\r\norder-id,customer-firstname, customer-lastname,customer-phone,date-of-purchase, remark\r\n\r\n47292fd2-b9b4-4465-8637-48eef12c4b1e, Sherrie, Henry,256-589-3171, 2023-08-31T20:26:34+00:00, some simple text\r\n815c9395-d35b-4fe3-9fa6-7fb35c2daa1b, Ronald, Haley,469-519-5571, 2023-08-31T18:05:45+00:00, \"some text with , and closure\"\r\n188ff3e3-94f5-4e3c-b351-4371d395f878, Martha, Rice,904-237-4794, 2023-08-31T17:33:59+00:00, \"some text with \\\"escaped\\\" data \"\r\na54f79f6-8286-4705-9de0-9f0f3e302bbf, Erica, Attaway,410-930-0663, 2023-08-31T10:21:54+00:00, no closure \\\"but\\\" with escape\r\ndc1b16e7-c8dc-4296-8bbe-30532dcad045, Nanette, Teran, 410-350-3954, 2023-08-31T09:46:12+00:00, \"with closure \\\"and\\\" with escape\"\r\n0ba97355-0ab3-4d85-9d6c-e66d1755dc1e, Hector, Frier,361-484-7220, 2023-08-31T09:55:35+00:00,\r\nf84416d8-e051-43eb-9ac7-9c6edd6c469e, Stephanie, Zimmerman,209-874-1605, 2023-08-30T21:39:14+00:00,\r\nb605f541-135c-49cc-beeb-ac7129f73014, Garrett, Russell,727-360-1839, 2023-08-30T13:20:27+00:00,\r\n7fec6236-8d6c-48df-82d6-22532a325f21, Margaret, Knight,775-418-3551, 2023-08-30T11:04:07+00:00,\r\nd8856748-f898-4a99-828b-735ad614614a, Ryann, Curley,303-785-5140, 2023-08-30T10:17:15+00:00,\r\n\r\n";

        Console.WriteLine($"{Environment.NewLine}Read as row (from string):");
        foreach (var row in csv.ReadRowFromString(text).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {row[0]} / {row[1]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as record (from string):");
        foreach (var record in csv.ReadRecordFromString(text).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {record[0]} / {record["CustomerLastname"]}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as dynamic (from string):");
        foreach (var obj in csv.ReadDynamicFromString(text).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {obj.OrderId} / {obj.CustomerPhone}");
        }

        Console.WriteLine($"{Environment.NewLine}Read as object Order (from string):");
        foreach (var order in csv.ReadObjectFromString<Order>(text).Take(5))
        {
            Console.WriteLine($"{csv.RowId,3} | {order.OrderId} / {order.DateOfPurchase}");
        }

        myOrder = new();
        Console.WriteLine($"{Environment.NewLine}Read as object Order (from string):");
        foreach (var _ in csv.ReadObjectFromString(text, myOrder))
        {
            Console.WriteLine($"{csv.RowId,3} | {myOrder.OrderId} / {myOrder.DateOfPurchase}");
        }
    }

    private static void ErrorDemo()
    {
        Console.WriteLine("\r\n\r\n== CSV Error Demo ========");

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
            foreach (var order in csv.ReadObjectFromPath<Order>(path))
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

    private static void WriteStream()
    {
        Console.WriteLine("== CSV Write Demo ========");

        StringWriter sw = new StringWriter();

        var csv = new CSVWriter(sw, new Settings());
        csv.Headers.Add("Name", "FirstName", "LastName").Add("Height", "Remarks");

        // optional: write headers by hand
        csv.WriteHeader();
        csv.WriteEmptyLine();

        // Write by arguments
        csv.Write("Parker, Peter", "Peter", "Parker", 175.3, "Arguments example");

        // Write Rows
        var row1 = new List<object> { "Parker, Peter", "Peter", "Parker", 175.3, "List of objects example" };
        csv.Write(row1);

        var row2 = new List<string> { "Parker, Peter", "Peter", "Parker", "175.3", "List of string example" };
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

        Console.WriteLine(sw.ToString());
    }


}