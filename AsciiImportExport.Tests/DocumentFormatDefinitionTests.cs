#region using directives

using System.Collections.Generic;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class DocumentFormatDefinitionTests
    {
        [Test]
        public void ExportMultiplePocos()
        {
            IDocumentFormatDefinition<SimplePoco> definition = GetPocoDefinition();

            var pocoList = new List<SimplePoco>
                               {
                                   new SimplePoco {Int32Prop = Int32PropValue1, StringProp = StringPropValue1},
                                   new SimplePoco {Int32Prop = Int32PropValue2, StringProp = StringPropValue2}
                               };

            string result = definition.Export(pocoList);

            Assert.AreEqual(StringPropValue1 + "\t" + Int32PropValue1 + "\r\n" + StringPropValue2 + "\t" + Int32PropValue2, result);
        }

        [Test]
        public void ExportPoco()
        {
            IDocumentFormatDefinition<SimplePoco> definition = GetPocoDefinition();

            var poco = new SimplePoco {Int32Prop = Int32PropValue1, StringProp = StringPropValue1};
            string result = definition.Export(new List<SimplePoco> {poco});

            Assert.AreEqual(StringPropValue1 + "\t" + Int32PropValue1, result);
        }

        [Test]
        public void ImportMultiplePocos()
        {
            IDocumentFormatDefinition<SimplePoco> definition = GetPocoDefinition();

            List<SimplePoco> result = definition.Import(StringPropValue1 + "\t" + Int32PropValue1 + "\r\n" + StringPropValue2 + "\t" + Int32PropValue2);

            Assert.AreEqual(2, result.Count);

            Assert.AreEqual(StringPropValue1, result[0].StringProp);
            Assert.AreEqual(Int32PropValue1, result[0].Int32Prop);

            Assert.AreEqual(StringPropValue2, result[1].StringProp);
            Assert.AreEqual(Int32PropValue2, result[1].Int32Prop);
        }


        [Test]
        public void ImportPoco()
        {
            IDocumentFormatDefinition<SimplePoco> definition = GetPocoDefinition();

            List<SimplePoco> result = definition.Import(StringPropValue1 + "\t" + Int32PropValue1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(StringPropValue1, result[0].StringProp);
            Assert.AreEqual(Int32PropValue1, result[0].Int32Prop);
        }

        private static IDocumentFormatDefinition<SimplePoco> GetPocoDefinition()
        {
            return new DocumentFormatDefinitionBuilder<SimplePoco>("\t", false)
                .SetCommentString("#")
                .AddColumn(x => x.StringProp)
                .AddColumn(x => x.Int32Prop)
                .Build();
        }

        private const int Int32PropValue1 = 1233;
        private const int Int32PropValue2 = -344;
        private const string StringPropValue1 = "Hello";
        private const string StringPropValue2 = "World!";
    }
}