#region using directives

using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class DocumentColumnTests
    {
        [Test]
        public void CheckInt32PropDefaultHeader()
        {
            IDocumentColumn<SimplePoco> column = GetInt32PropColumn();

            Assert.AreEqual("Int32Prop", column.Header);
        }

        [Test]
        public void CheckStringPropHeader()
        {
            IDocumentColumn<SimplePoco> column = GetStringPropColumn();

            Assert.AreEqual("IAmAStringProperty", column.Header);
        }


        [Test]
        public void ExportInt32Prop()
        {
            IDocumentColumn<SimplePoco> column = GetInt32PropColumn();
            var poco = new SimplePoco {Int32Prop = Int32PropValue};
            string result = column.Serialize(poco);

            Assert.AreEqual(Int32PropValue.ToString(), result);
        }

        [Test]
        public void ExportStringProp()
        {
            IDocumentColumn<SimplePoco> column = GetStringPropColumn();

            var poco = new SimplePoco {StringProp = StringPropValue};
            string result = column.Serialize(poco);

            Assert.AreEqual(StringPropValue, result);
        }

        [Test]
        public void ImportInt32Prop()
        {
            IDocumentColumn<SimplePoco> column = GetInt32PropColumn();

            var poco = new SimplePoco();
            column.Parse(poco, Int32PropValue.ToString());

            Assert.AreEqual(Int32PropValue, poco.Int32Prop);
        }

        [Test]
        public void ImportStringProp()
        {
            IDocumentColumn<SimplePoco> column = GetStringPropColumn();

            var poco = new SimplePoco();
            column.Parse(poco, StringPropValue);

            Assert.AreEqual(StringPropValue, poco.StringProp);
        }

        private static IDocumentColumn<SimplePoco> GetInt32PropColumn()
        {
            var builder = new DocumentColumnBuilder<SimplePoco, int>(x => x.Int32Prop);
                
                //, null, () => 0, -1, ColumnAlignment.Left, "0", null, null, null, null);
            return builder.Build();
        }

        private static IDocumentColumn<SimplePoco> GetStringPropColumn()
        {
            var builder = new DocumentColumnBuilder<SimplePoco, string>(x => x.StringProp).SetHeader("IAmAStringProperty");
                //, null, -1, ColumnAlignment.Left, null, null, null, null, null);
            return builder.Build();
        }

        private const string StringPropValue = "Hello!";
        private const int Int32PropValue = 4;
    }
}