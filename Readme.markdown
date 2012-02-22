AsciiImportExport v0.1
======================================================================

## Overview
A library for easy ASCII-Import/Export in .NET applications.

## Example

These are the pocos present in 

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

	

	public List<AutoresizePoco> GetPocoList()
    {
        return new List<AutoresizePoco>
                    {
                        new AutoresizePoco {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                        new AutoresizePoco {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                        new AutoresizePoco {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                    };
    }


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