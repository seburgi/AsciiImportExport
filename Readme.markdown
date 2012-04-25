AsciiImportExport v0.8
======================================================================

## Overview
A .NET library providing fast and easy de/serialization of arbitrary column-based text data.

## Changelog
  * v0.8 - Fixed column default value problems, added flag that indicates if exported lines terimate with the column separator, other small tweaks
  * v0.7 - Now supports all built-in value types, Massive performance improvements
  * v0.6 - Fixed some smaller bugs, minor restructuring of public surface
  * v0.5 - Cleaned up column handling, fixed problems with comments, changed target framework to .NET 3.5 Client Profile
  * v0.4 - Resolved some initialization problems in DocumentColumn
  * v0.3 - Added comments, cleaned method names
  * v0.2 - Added DocumentColumnBuilder for better separation of concerns
  * v0.1 - Initial release

## Example

### POCO
    public class Person
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
	
    public List<Person> GetPersonList()
    {
        return new List<Person>
                    {
                        new Person {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                        new Person {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                        new Person {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                    };
    }


### Definition

Now we define how the data will be exported / imported:

    private static DocumentFormatDefinition<Person> GetDefinition()
    {
        return new DocumentFormatDefinitionBuilder<Person>("\t", true)
            .SetCommentString("#")
            .AddColumn(x => x.Name)
            .AddColumn(x => x.Gender, b => b.SetImportFunc(StringToGender).SetExportFunc(GenderToString))
            .AddColumn(x => x.Height, "0.00", b => b.SetAlignment(ColumnAlignment.Right))
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
* automatically determine the width of a column
* lines starting with '#' are recognized as comments
* add new column for property Name
* add new column for property Gender and use special import / export methods
* add new column for property Height. Align the data of the column to the right and use a double precision of 2.
* add new column for property Birthday with a custom date format
* add new column for property Memo


### Export

And now we can export the list:

	public string Export(List<Person> list)
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

	public List<Person> Import(string data)
	{
	    var definition = GetDefinition();
	    return definition.Import(data);
	}
