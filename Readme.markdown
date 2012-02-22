AsciiImportExport v0.1
======================================================================

## Overview
A library for easy ASCII-Import/Export in .NET applications.

For now this is only for experimental use.
Please don't use this in a productive environment.

## Example
This example was taken from the Autoresize folder.
We want to export a list of AutoresizePoco.

### POCO
    class AutoresizePoco
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string Memo { get; set; }
        public double Height { get; set; }
    }

    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }

### Data
This is our list:
	
	public List<AutoresizePoco> GetPocoList()
    {
        return new List<AutoresizePoco>
                    {
                        new AutoresizePoco {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                        new AutoresizePoco {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                        new AutoresizePoco {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                    };
    }


### Definition

Now we define how the data will be exported / imported:

    private static DocumentFormatDefinition<AutoresizePoco> GetDefinition_With_Tab_As_ColumnSeparator()
    {
        return new DocumentFormatDefinitionBuilder<AutoresizePoco>()
            .SetColumnSeparator("\t")
            .SetCommentString("!")
            .SetAutosizeColumns(true)
            .SetExportHeaderLine(false)
            .AddColumn(new DocumentColumn<AutoresizePoco>(x => x.Name))
            .AddColumn(new DocumentColumn<AutoresizePoco>(x => x.Gender).SetImportExportActions(s => StringToGender(s), o => GenderToString(o)))
            .AddColumn(new DocumentColumn<AutoresizePoco>(x => x.Height).SetAlignment(ColumnAlignment.Right).SetDoublePrecision(2))
            .AddColumn(new DocumentColumn<AutoresizePoco>(x => x.Birthday).SetDateTimeFormat("yyyyMMdd"))
            .AddColumn(new DocumentColumn<AutoresizePoco>(x => x.Memo))
            .Build();
    }
    
    private static string GenderToString(object o)
    {
        Gender gender = o is Gender ? (Gender) o : Gender.Unknown;
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

    private static object StringToGender(string s)
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

	public string Export(List<AutoresizePoco> list)
	{
	    var definition = GetDefinition_With_Tab_As_ColumnSeparator();
	    return definition.Export(list);
	}

Which will yield:

	Peter	M	 175.50	19830129	Nice guy!
	Paul 	M	 173.45	19311005	Sometimes a litte grumpy.
	Mary 	F	1193.00	19800412	Tall!
	
### Import

No surprises here!

	public List<AutoresizePoco> Import(string data)
	{
	    var definition = GetDefinition_With_Tab_As_ColumnSeparator();
	    return definition.Import(data);
	}