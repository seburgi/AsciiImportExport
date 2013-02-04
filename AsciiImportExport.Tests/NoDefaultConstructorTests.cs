using System.Collections.Generic;
using System.IO;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class NoDefaultConstructorTests
    {
        private IDocumentFormatDefinition<GpsLocation> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<GpsLocation>("\t", true)
                .SetCommentString("#")
                .AddColumn(x => x.Lat)
                .AddColumn(x => x.Lon)
                .Build();
        }

        [Test]
        public void NoDefaultConstructorTest()
        {
            var measurements = new List<GpsLocation>()
                                   {
                                       new GpsLocation(48.3224, 18.3224)
                                   };

            IDocumentFormatDefinition<GpsLocation> definition = GetDefinition();

            string exportResult = definition.Export(measurements);
            List<GpsLocation> importResult = definition.Import(new StringReader(exportResult));
            Assert.AreEqual(1, importResult.Count);
            Assert.AreEqual(measurements[0].Lat, importResult[0].Lat);
            Assert.AreEqual(measurements[0].Lon, importResult[0].Lon);
        }
    }
}