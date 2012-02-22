AsciiImportExport v0.1
======================================================================

## Overview
A library for easy ASCII-Import/Export in .NET applications.

## Example
	class Poco
	{
		public int Int32Prop { get; set; }
		public string StringProp { get; set; }
	}

	DocumentFormatDefinitionBuilder<Poco> builder = new DocumentFormatDefinitionBuilder<Poco>();
	DocumentFormatDefinition<Poco> definition = builder
												 .SetColumnSeparator("\t")
												 .SetCommentString("!")
												 .SetAutosizeColumns(true)
												 .SetExportHeaderLine(false)
												 .AddColumn(new DocumentColumn<Poco>(x => x.StringProp))
												 .AddColumn(new DocumentColumn<Poco>(x => x.Int32Prop))
												 .Build();
												 
	Poco poco = new Poco {Int32Prop = Int32PropValue, StringProp = StringPropValue};

	string result = definition.Export(new[] {poco});
