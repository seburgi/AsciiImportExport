AsciiImportExport v0.1
======================================================================

## Overview
A .NET library providing fast and easy de/serialization of arbitrary column-based text data.

## Example

### POCO
    internal class Poco
    {
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public double Height { get; set; }
        public string Memo { get; set; }
        public string Name { get; set; }
    }

    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }

### Data
This is our list:
	
    public List<Poco> GetPocoList()
    {
        return new List<Poco>
                    {
                        new Poco {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                        new Poco {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                        new Poco {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                    };
    }


### Definition

Now we define how the data will be exported / imported:

    private static DocumentFormatDefinition<Poco> GetDefinition_With_Tab_As_ColumnSeparator()
    {
        return new DocumentFormatDefinitionBuilder<Poco>()
            .SetColumnSeparator("\t")
            .SetCommentString("!")
            .SetAutosizeColumns(true)
            .SetExportHeaderLine(false)
            .AddColumn(x => x.Name)
            .AddColumn(x => x.Gender, ColumnAlignment.Left, StringToGender, GenderToString)
            .AddColumn(x => x.Height, ColumnAlignment.Right, "0.00")
            .AddColumn(x => x.Birthday, "yyyyMMdd")
            .AddColumn(x => x.Memo)
            .Build();
    }
    
    private static string GenderToString(Gender gender)
    {
        switch (gender)
        {
            case Gender.Female:
                return "F";
            case Gender.Male:
                return "M";
            default:
                return "U";
        }
    }

    private static Gender StringToGender(string s)
    {
        switch (s)
        {
            case "M":
                return Gender.Male;
            case "F":
                return Gender.Female;
            default:
                return Gender.Unknown;
        }
    }
    
That is what we defined:

* seperate columns by Tabs
* lines starting with '!' are recognized as comments
* automatically determine the width of a column
* do not insert a header line
* add new column for property Name
* add new column for property Gender and use special import / export methods
* add new column for property Height. Align the data of the column to the right and use a double precision of 2.
* add new column for property Birthday with a custom date format
* add new column for property Memo


### Export

And now we can export the list:

	public string Export(List<Poco> list)
	{
	    var definition = GetDefinition();
	    return definition.Export(list);
	}

Which will yield:

	Peter	M	 175.50	19830129	Nice guy!
	Paul 	M	 173.45	19311005	Sometimes a litte grumpy.
	Mary 	F	1193.00	19800412	Tall!
	
### Import

No surprises here!

	public List<Poco> Import(string data)
	{
	    var definition = GetDefinition();
	    return definition.Import(data);
	}
