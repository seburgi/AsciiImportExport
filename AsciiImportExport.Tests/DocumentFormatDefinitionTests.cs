#region using directives

using System.Collections.Generic;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    internal class Poco
    {
        public int Int32Prop { get; set; }
        public string StringProp { get; set; }
    }

    [TestFixture]
    internal class DocumentFormatDefinitionTests
    {
        private const int Int32PropValue1 = 1233;
        private const int Int32PropValue2 = -344;
        private const string StringPropValue1 = "Hello";
        private const string StringPropValue2 = "World!";

        private static DocumentFormatDefinition<Poco> GetPocoDefinition()
        {
            return new DocumentFormatDefinitionBuilder<Poco>()
                .SetColumnSeparator("\t")
                .SetCommentString("!")
                .SetAutosizeColumns(false)
                .SetExportHeaderLine(false)
                .AddColumn(x => x.StringProp)
                .AddColumn(x => x.Int32Prop)
                .Build();
        }

        [Test]
        public void ExportMultiplePocos()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            var pocoList = new List<Poco>
                               {
                                   new Poco {Int32Prop = Int32PropValue1, StringProp = StringPropValue1},
                                   new Poco {Int32Prop = Int32PropValue2, StringProp = StringPropValue2}
                               };

            string result = definition.Export(pocoList);

            Assert.AreEqual(StringPropValue1 + "\t" + Int32PropValue1 + "\r\n" + StringPropValue2 + "\t" + Int32PropValue2, result);
        }

        [Test]
        public void ExportPoco()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            var poco = new Poco {Int32Prop = Int32PropValue1, StringProp = StringPropValue1};
            string result = definition.Export(new List<Poco> {poco});

            Assert.AreEqual(StringPropValue1 + "\t" + Int32PropValue1, result);
        }

        [Test]
        public void ImportMultiplePocos()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            List<Poco> result = definition.Import(StringPropValue1 + "\t" + Int32PropValue1 + "\r\n" + StringPropValue2 + "\t" + Int32PropValue2);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(StringPropValue1, result[0].StringProp);
            Assert.AreEqual(Int32PropValue1, result[0].Int32Prop);

            Assert.AreEqual(StringPropValue2, result[1].StringProp);
            Assert.AreEqual(Int32PropValue2, result[1].Int32Prop);
        }


        [Test]
        public void ImportPoco()
        {
            DocumentFormatDefinition<Poco> definition = GetPocoDefinition();

            List<Poco> result = definition.Import(StringPropValue1 + "\t" + Int32PropValue1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(StringPropValue1, result[0].StringProp);
            Assert.AreEqual(Int32PropValue1, result[0].Int32Prop);
        }
    }
}