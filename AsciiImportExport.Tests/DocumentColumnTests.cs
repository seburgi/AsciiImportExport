#region using directives

using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class DocumentColumnTests
    {
        private const string StringPropValue = "Hello!";
        private const int Int32PropValue = 4;

        private static DocumentColumn<Poco> GetStringPropColumn()
        {
            var column = new DocumentColumn<Poco>(x => x.StringProp);
            return column;
        }

        private static DocumentColumn<Poco> GetInt32PropColumn()
        {
            var column = new DocumentColumn<Poco>(x => x.Int32Prop);
            return column;
        }

        [Test]
        public void ExportInt32Prop()
        {
            DocumentColumn<Poco> column = GetInt32PropColumn();

            var poco = new Poco {Int32Prop = Int32PropValue};
            string result = column.Format(poco);

            Assert.AreEqual(Int32PropValue.ToString(), result);
        }

        [Test]
        public void ExportStringProp()
        {
            DocumentColumn<Poco> column = GetStringPropColumn();

            var poco = new Poco {StringProp = StringPropValue};
            string result = column.Format(poco);

            Assert.AreEqual(StringPropValue, result);
        }

        [Test]
        public void ImportInt32Prop()
        {
            DocumentColumn<Poco> column = GetInt32PropColumn();

            var poco = new Poco();
            column.SetValue(poco, Int32PropValue.ToString());

            Assert.AreEqual(Int32PropValue, poco.Int32Prop);
        }

        [Test]
        public void ImportStringProp()
        {
            DocumentColumn<Poco> column = GetStringPropColumn();

            var poco = new Poco();
            column.SetValue(poco, StringPropValue);

            Assert.AreEqual(StringPropValue, poco.StringProp);
        }
    }

    internal class Poco
    {
        public int Int32Prop { get; set; }
        public string StringProp { get; set; }
    }
}