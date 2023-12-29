using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pyther.Parser.CSV;

namespace Tests
{
    [TestClass]
    public class CSVReader
    {
        [TestMethod]
        public void ReadAsRow()
        {
            var csv = new Pyther.Parser.CSV.CSVReader();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo", "orders.csv");

            string lastname = "";
            string remark = "";

            foreach (List<object> row in csv.ReadRowFromPath(path))
            {
                if (csv.RowId == 7)
                {
                    remark = row[5].ToString() ?? "";
                }
                if (csv.RowId == 10) {
                    lastname = row[2].ToString() ?? "";
                }
            }

            Assert.AreEqual(lastname, "Zimmerman");
            Assert.AreEqual(remark, "no closure \"but\" with escape");
        }

        [TestMethod]
        public void ReadAsRecord()
        {
            var csv = new Pyther.Parser.CSV.CSVReader();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo", "orders.csv");

            string lastname = "";
            string remark = "";

            foreach (Record row in csv.ReadRecordFromPath(path))
            {
                if (csv.RowId == 7)
                {
                    remark = row[5]?.ToString() ?? "";
                }
                if (csv.RowId == 10)
                {
                    lastname = row["customer-lastname"]?.ToString() ?? "";
                }
            }

            Assert.AreEqual(lastname, "Zimmerman");
            Assert.AreEqual(remark, "no closure \"but\" with escape");
        }

        [TestMethod]
        public void ReadAsDynamic()
        {
            var csv = new Pyther.Parser.CSV.CSVReader(new Settings()
            {
                HeaderTransformMethod = TransformMethods.Auto
            });
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo", "orders.csv");

            string lastname = "";
            string remark = "";

            foreach (dynamic obj in csv.ReadDynamicFromPath(path))
            {
                if (csv.RowId == 7)
                {
                    remark = obj.Remark.ToString() ?? "";
                }
                if (csv.RowId == 10)
                {
                    lastname = obj.CustomerLastname.ToString() ?? "";
                }
            }

            Assert.AreEqual(lastname, "Zimmerman");
            Assert.AreEqual(remark, "no closure \"but\" with escape");
        }

        public class Order
        {
            public string? OrderId { get; set; }
            public string? CustomerFirstname { get; set; }
            public string? CustomerLastname { get; set; }
            public string? CustomerPhone { get; set; }
            public DateTime DateOfPurchase { get; set; }
            public string? Remark { get; set; }
        }

        [TestMethod]
        public void ReadAsObject()
        {
            var csv = new Pyther.Parser.CSV.CSVReader(new Settings()
            {
                //HeaderTransformMethod = TransformMethods.KebabCaseToTitleCase
                HeaderTransformMethod = TransformMethods.Auto
            });
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo", "orders.csv");

            string lastname = "";
            string remark = "";

            foreach (var obj in csv.ReadObjectFromPath<Order>(path))
            {
                if (csv.RowId == 7)
                {
                    remark = obj.Remark ?? string.Empty;
                }
                if (csv.RowId == 10)
                {
                    lastname = obj.CustomerLastname ?? string.Empty;
                }
            }

            Assert.AreEqual(lastname, "Zimmerman");
            Assert.AreEqual(remark, "no closure \"but\" with escape");
        }


        private static object MyCellTransform(object data, int columnIndex, string? columnName)
        {
            return columnName switch
            {
                "DateOfPurchase" => DateTime.Parse((string)data).ToUniversalTime(),
                _ => data,
            };
        }

        [TestMethod]
        public void CellTransform()
        {
            var csv = new Pyther.Parser.CSV.CSVReader(new Settings()
            {
                HeaderTransformMethod = TransformMethods.Auto,
                CellTransformMethod = MyCellTransform
            });
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "demo", "orders.csv");

            foreach (var obj in csv.ReadObjectFromPath<Order>(path))
            {
                if (csv.RowId == 4)
                {
                    Assert.AreEqual(obj.DateOfPurchase, DateTime.Parse("2023-08-31T20:26:34+00:00").ToUniversalTime());
                }
            }
        }
    }
}