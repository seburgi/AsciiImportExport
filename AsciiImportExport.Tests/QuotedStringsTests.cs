using System.Collections.Generic;
using System.Linq;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class QuotedStringsTests
    {
        private IDocumentFormatDefinition<Location> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<Location>(";", true)
                .SetCommentString("'")
                .SetExportHeaderLine(true, "' ")
                .AddColumn(x => x.Name, x => x.SetExportQuotedStrings(true))
                .AddColumn(x => x.X)
                .AddColumn(x => x.Y)
                .AddColumn(x => x.Z)
                .Build();
        }

        [Test]
        public void ColumnSeparatorInColumn()
        {
            var location = new Location() {Name = "House;Green", X = 12.24, Y = 34.213, Z = 12.1231231};

            IDocumentFormatDefinition<Location> definition = GetDefinition();
            string exportResult = definition.Export(new[] {location});

            List<Location> importResult = definition.Import(exportResult);

            Assert.AreEqual(1, importResult.Count);

            Assert.AreEqual(location.Name, importResult.First().Name);
            Assert.AreEqual(location.X, importResult.First().X);
            Assert.AreEqual(location.Y, importResult.First().Y);
            Assert.AreEqual(location.Z, importResult.First().Z);
        }

        // Comment in Column not possible yet
        //[Test]
        //public void CommentStringInColumn()
        //{
        //    var location = new Location() { Name = "House'Green", X = 12.24, Y = 34.213, Z = 12.1231231 };

        //    IDocumentFormatDefinition<Location> definition = GetDefinition();
        //    string exportResult = definition.Export(new[] { location });

        //    List<Location> importResult = definition.Import(exportResult);

        //    Assert.AreEqual(1, importResult.Count);

        //    Assert.AreEqual(location.Name, importResult.First().Name);
        //    Assert.AreEqual(location.X, importResult.First().X);
        //    Assert.AreEqual(location.Y, importResult.First().Y);
        //    Assert.AreEqual(location.Z, importResult.First().Z);
        //}

        [Test]
        public void ExportQuotedStrings()
        {
            var location = new Location() {Name = "House \"Red\"", X = 12.24, Y = 34.213, Z = 12.1231231};

            IDocumentFormatDefinition<Location> definition = GetDefinition();
            string exportResult = definition.Export(new[] {location});

            List<Location> importResult = definition.Import(exportResult);

            Assert.AreEqual(1, importResult.Count);
            Assert.AreEqual(2, importResult.First().Name.Count(x => x == '"'));

            Assert.AreEqual(location.Name, importResult.First().Name);
            Assert.AreEqual(location.X, importResult.First().X);
            Assert.AreEqual(location.Y, importResult.First().Y);
            Assert.AreEqual(location.Z, importResult.First().Z);
        }
    }
}