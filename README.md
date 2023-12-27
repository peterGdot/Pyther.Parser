# CSV File Reader/Writer
* namespace `Pyther.Parser.CSV;` 

## Features
* easy to use
* extreme fast (take a look at the end of this document)
* lightweight code
* can handle csv files of any size with minimal memory footprint
* can transform rows into associative records, dynamic objects or objects of any class
* lots of optional settings

## Quickstart

### Read CSV Files
One of **many** ways to read CSV files:
```cs
using Pyther.Parser.CSV;

var csv = new CSVReader();

foreach (var record in csv.ReadRecordFromPath(@"C:\orders.csv"))
{
    Console.WriteLine(record["customer-lastname"].ToString());
}
```

### Write CSV Files
One of **many** ways to write CSV files:
```cs
using Pyther.Parser.CSV;

var csv = new CSVWriter(@"C:\heroes.csv");

csv.Headers.Add("Name", "FirstName", "LastName", "Height", "Remarks");
csv.Write("Parker, Peter", "Peter", "Parker", 175.3, "Arguments example");
```

## CSVReader

### Example Data
Lets take a csv like this as an example:

![](data/assets/projects/pyther/core/orders-csv.png)
[order.csv](data/assets/projects/pyther/core/orders.csv?download)

We will discover 4 different ways to parse this file.

### 1st Way - Read as Row
* returns a `System.Collections.Generic.List` of `object` per entry
* access by index

```cs
var csv = new CSVReader();

foreach (List<object> row in csv.ReadRowFromPath(@"C:\orders.csv"))
{
    string remark = row[5].ToString() ?? "";
    string lastname = row[2].ToString() ?? "";
}
```

### 2nd Way - Read as Record
* returns a `Pyther.Parser.CSV.Record` per entry
* access by index or column header name

```cs
var csv = new CSVReader();

foreach (Record row in csv.ReadRecordFromPath(@"C:\orders.csv"))
{
    string remark = row[5].ToString() ?? "";
    string lastname = row["customer-lastname"].ToString() ?? "";
}
```

* if you only need to access the record by header name or index, you can use one of these values as the second paramater:
    * `RecordFlags.Indexed` ... allow access by index only
    * `RecordFlags.Associative` ... allow access by name only
    * `RecordFlags.Both` ... allow access by index and name (default)
   
### 3rd Way - Read as Dynamic Object
* returns a `dynamic` object
* HeaderTransformMethod  defines how the column name should be transformed
    * TransformMethods.KebabCaseToTitleCase: `customer-lastname` => `CustomerName`
* access by object property (named from colum header)

```cs
var csv = new CSVReader(new Settings()
{
    HeaderTransformMethod = TransformMethods.Auto
});

foreach (dynamic obj in csv.ReadDynamicFromPath(@"C:\orders.csv"))
{
    string remark = obj.Remark.ToString() ?? "";
    string lastname = obj.CustomerLastname.ToString() ?? "";
}
```
* Hint: *This is the slowest way*

### 4th Way - Read as Object

lets say we have an object like

```cs
public class Order
{
    public string? OrderId { get; set; }
    public string? CustomerFirstname { get; set; }
    public string? CustomerLastname { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime DateOfPurchase { get; set; }
    public string? Remark { get; set; }
}
```

we can read each line transformed to this object:

```cs
var csv = new CSVReader(new Settings()
{
    HeaderTransformMethod = TransformMethods.Auto
});

foreach (var obj in csv.ReadObjectFromPath<Order>(@"C:\orders.csv"))
{
    string remark = obj.Remark;
    string lastname = obj.CustomerLastname;
}
```

* You can also populate an existing object by using it as second argument. This way you can also recycle an object to improve performance.

```cs
Order myOrder = new Order();
foreach (var _ in csv.ReadObjectFromPath(@"C:\orders.csv", myOrder))
{
    Console.WriteLine($"{csv.RowId,3} | {myOrder.OrderId} / {myOrder.DateOfPurchase}");
}
```

## CSVWriter
* no matter what way you choose. After initialization you don't have to care about enclosures, delimters, escapes, aso.

### Create the csv object
```cs
var csv = new CSVWriter(@"C:\heroes.csv");
```
##### Defining the Column Headers
* by default headers are optional, but are required if you want to write (dynamic) objects
* let create the following headers: `Name`, `FirstName`, `LastName`, `Height` and `Remarks`:

```cs
csv.Headers.Add("Name", "FirstName", "LastName", "Height", "Remarks");
// or
csv.Headers.
    Add("Name").
    Add("FirstName").
    Add("LastName").
    Add("Height").
    Add("Remarks");
// or
csv.Headers.
    Add("Name", "FirstName").
    Add("LastName").
    Add("Height", "Remarks");
```

after that you *can* write the headers to the file. If you skip this, they will be written at the time the first record was written.
```cs
csv.WriteHeader();
```

### 1st Way - Write from Arguments
* all parameters are `object`

```cs
csv.Write("Parker, Peter", "Peter", "Parker", 175.3, "Arguments example");
```

### 2nd Way - Write from Lists
```cs
var row = new List<object>{"Parker, Peter", "Peter", "Parker", 175.3, "List of objects example" };
csv.Write(row);
```
or 
```cs
var row = new List<string>{"Parker, Peter", "Peter", "Parker", "175.3", "List of string example" };
csv.Write(row);
```

### 3rd Way - Write from Records
* Hint: on all cases, the ordering doesn't matter

a) from associative records
```cs
var rec = new Record();
rec["Name"] = "Parker, Peter";
rec["FirstName"] = "Peter";
rec["Height"] = 175.3;
rec["LastName"] = "Parker";
rec["Remarks"] = "List of mixed Record example";

csv.Write(rec);
```

b) from indexed records
```cs
var rec = new Record();
rec[0] = "Parker, Peter";
rec[1] = "Peter";
rec[2] = "Parker";
rec[3] = 175.3;
rec[4] = "List of indexed Record example";

csv.Write(rec);
```

c) or mixed
```cs
var rec = new Record();
rec["Name"] = "Parker, Peter";
rec["FirstName"] = "Peter";
rec[2] = "Parker";
rec["Height"] = 175.3;
rec["Remarks"] = "List of mixed Record example";

csv.Write(rec);
```

*Performance Hint*: if you know the amount of columns upfront (what is almost always the case), you should give this information as the first constructor argument:
```
var rec = new Record(5);
...
```

### 4th Way - Write from Dynamic Objects
You can also write dynamic objects
```cs
dynamic obj = new ExpandoObject();
obj.Name = "Parker, Peter";
obj.FirstName = "Peter";
obj.LastName = "Parker";
obj.Height = 175.3;
obj.Remarks = "dynamic object example";

csv.WriteDynamic(obj);
```

### 5th Way - Write from Custom Objects
Lets say we have the following `Person` class
```cs
class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Name => LastName + ", " + FirstName;
    public double Height { get; set; }
    public string? Remarks { get; set; }
}
```

with the following example data
```cs
var person = new Person()
{
    FirstName = "Peter",
    LastName = "Parker",
    Height = 175.3,
    Remarks = "custom object example"
};
```

we can simply write it the following way
```cs
csv.Write(person);
```

### 6th Way - Write Custom Objects using nested paths

Lets say we have two model class:
```cs
class Order
{
    public string? Id { get; set; }
    public Address? Billing { get; set; }
    public Address? Shipping { get; set; }
}

class Address
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
}
```

with the following example data
```cs
Order order = new()
{
    Id = "123",
    Shipping = new Address()
    {
        FirstName = "Peter",
        LastName = "Parker",
        Company = "Marvel"
    }
};
```

and we have the following csv headers:
```cs
csv.Headers
    .Add("Id")
    .Add("Billing.FirstName", "Billing.LastName", "Billing.Company")
    .Add("Shipping.FirstName", "Shipping.LastName", "Shipping.Company");
```

we can simply write it the following way
```cs
csv.WriteNested(order);
```

and we get the follow result (remember `order.Billing` was not set)
```
Id,Billing.FirstName,Billing.LastName,Billing.Company,Shipping.FirstName,Shipping.LastName,Shipping.Company

123,,,,Peter,Parker,Marvel
```

## Settings

You can affect the way how the csv file will be parsed using a `Pyther.Parser.CSV.Settings` object as a constrructor parameter:

```css
var csv = new CSVReader(new Settings()
{
    ...
});
```

### Encoding
* Defines the file encoding.
* type: `Encoding`
* default: `Encoding.UTF8`

### BufferSize
* The buffer size used to read the file.
* type: `int`
* default: `1MB` 

### RecordSeparator
* Defines how the records are separated.
* type: `string`
* default: `Environment.NewLine` 

### Delimeter
* Defines how the columns/cells are separated.
* type: `string`
* default: `,`

### Enclosure
* Defines the field enclosure.
* type: `string`
* default: `"`

### ForceEnclosure
* Should values always be enclosed?
* type: `bool`
* default: `false` 

### Escape
* An optional escape symbol.
* type: `string`
* default: `\`

### EscapeByDoubling
* Enable/Disable escaping using the enclosure symbol twice.
* type: `bool`
* default: `false`

### HasHeaders
* Does the CSV contain headers?
* type: `bool`
* default: `true`

### HeaderTransformMethod
* Callback method to transform column header names.
* type: `Func<string, string>?` (string) -> string
* default: `null`

### CellTransformMethod
* Callback method to transform cell content.
* type: `Func<object, int, string?, object>?` (cell data, column index, column name) -> object
* default: `null`

### AutoTrim
* Auto trim cell values?
* type: `bool`
* default: `true`

### IgnoreEmptyLines
* Ignore empty lines?
* type: `bool`
* default: `true`

### FormatProvider
* Format provider used when writing data (`null` means current culture)
* type `IFormatProvider`
* default: `CultureInfo.InvariantCulture`

### ErrorToManyColumns
* How to handle the error, if there are more column headers than record cells.
* type: `ErrorHandling`
* default: `ErrorHandling.TryToSolve`
* values: `Ignore`, `TryToSolve` or`Throw`

### ErrorToFewColumns
* How to handle the error, if there are less column headers than record cells.
* type: `ErrorHandling`
* default: `ErrorHandling.TryToSolve`
* values: `Ignore`, `TryToSolve` or`Throw`

### ErrorInvalidClassProperty
* How to handle the error, if a property doesn't exists in the object.
* type: `ErrorHandling`
* default: `ErrorHandling.TryToSolve` 
* values: `Ignore`, `TryToSolve` or`Throw`

## How can I ...

### ... limit the result
Since the `Read....()` method returns an `IEnumerable`, you can use all methods they define. This include the `Take()` , `Skip()`, `Where()` aso. :

```cs
// skip 2 and get 5 records
foreach (var obj in csv.ReadRecord(@"C:\orders.csv").Skip(2).Take(5))
{
    ...
}
```

### ... transform cell data during parsing

First create a callback method, that is called for each cell:
* Arguments
    * data ... raw cell data
    * columnIndex ... the index of the column of the current cell
    * columnName ... If headers are given, this argument will hold the column name of the current cell
 * Return
     * This method has to return final cell data

```cs
private static object MyCellTransform(object data, int columnIndex, string? columnName)
{
    switch (columnName)
    {
        case "DateOfPurchase":
            return DateTime.Parse((string)data).ToUniversalTime();
        default:
            return data;
    }
}
````

Set the method in the settings
```cs

var csv = new CSVReader(new Settings()
{
    ...
    CellTransformMethod = MyCellTransform
});```
```

## Performance
* Test System
    * Intel Core i5 13600KF 14x 5.1 GHz
    * 32GB DDR4-RAM PC-3600 
    * NVME M.2 SSD 1TB Kingston KC3000
    * Windows 11 Pro 64-Bit
  
### Scenario 1  (typical case)
* Test Scenario
    * real world data (shop orders)
    * 80 col x 100k rows = 8 Mio cells
    * ~75 MB
    * Average of 5 iterations

| | ReadRow | ReadRecord | ReadRecord (indexed) | ReadDynamic | ReadObject |
| --- | --- | --- | --- | --- | --- |
| Time in Seconds | 0.7918 | 0.8926  | **0.7074** | 3.7348 |  1.598  |
| Cells per Seconds | 10.10 Mio | 8.96 Mio| **11.31 Mio** | 2..14 Mio | 5.01 Mio |

### Scenario 2 (optimized case)
* Test Scenario
    * same as above using amazon orders, with the following options:
        * Enclosure = null
        * Delimeter = '\t'
        * Escape = null
        * RecordSeparator = Environment.NewLine
    * these options will use the _fast path_ for parsing
 
| | ReadRow | ReadRecord | ReadRecord (indexed) | ReadDynamic | ReadObject |
| --- | --- | --- | --- | --- | --- |
| Time in Seconds | 0.312 | 0.472 | **0.320** | 3.011 |  0.968 |
| Cells per Seconds | 25.64 Mio | 16.95 Mio| **25 Mio** | 2.66 Mio | 8.26 Mio |

## Open Topics
* [ ] CSVWriter: Allow Header alias or Transform
* [ ] CSVWriter: Cell Transform
* [ ] CSVWriter: ErrorToManyColumns, ErrorToFewColumns, ErrorInvalidClassProperty