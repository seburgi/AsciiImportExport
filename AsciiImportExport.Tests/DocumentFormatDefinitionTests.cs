#region using directives

using System.Collections.Generic;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class DocumentFormatDefinitionTests
    {
        private const int Int32PropValue = 4;
        private const string StringPropValue = "Hello!";

        private static DocumentFormatDefinition<Poco> GetPocoDefinition()
        {
            var builder = new DocumentFormatDefinitionBuilder<Poco>();
            return builder
                .SetColumnSeparator("\t")
                .SetCommentString("!")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(new DocumentColumn<Poco>(x => x.StringProp))
                .AddColumn(new DocumentColumn<Poco>(x => x.Int32Prop))
                .Build();
        }

        [Test]
        public void ExportPoco()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            var poco = new Poco {Int32Prop = Int32PropValue, StringProp = StringPropValue};
            string result = definition.Export(new[] {poco});

            Assert.AreEqual(StringPropValue + "\t" + Int32PropValue, result);
        }

        [Test]
        public void ImportMultiplePocos()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            const string importData = "Hello\t789\r\nWorld!\t-923";
            List<Poco> result = definition.Import(importData);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Hello", result[0].StringProp);
            Assert.AreEqual(789, result[0].Int32Prop);

            Assert.AreEqual("World!", result[1].StringProp);
            Assert.AreEqual(-923, result[1].Int32Prop);
        }

        [Test]
        public void ImportPoco()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            List<Poco> result = definition.Import(StringPropValue + "\t" + Int32PropValue);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(StringPropValue, result[0].StringProp);
            Assert.AreEqual(Int32PropValue, result[0].Int32Prop);
        }
    }
}