using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class CultureInfoTests
    {
        [Test]
        public void TestCultureInfo()
        {
            var measurements = new List<Measurement>()
                                   {
                                       new Measurement
                                           {
                                               DateTime = DateTime.Now,
                                               IsActive = true,
                                               Name = "ABC",
                                               X = 10.2,
                                               Y = 13.49,
                                               Z = 45.4
                                           }
                                   };

            var definition = GetDefinition();
            
            var exportResult = definition.Export(measurements);
            Assert.IsTrue(exportResult.Contains("10,2")); // Uses de-DE
            Assert.IsTrue(exportResult.Contains("13,49")); // Uses de-DE
            Assert.IsTrue(exportResult.Contains("45.4")); // Uses InvariantCulture

            var  importResult = definition.Import(new StringReader(exportResult));
            Assert.AreEqual(measurements[0].X, importResult[0].X);
            Assert.AreEqual(measurements[0].Y, importResult[0].Y);
            Assert.AreEqual(measurements[0].Z, importResult[0].Z);
        }

        private IDocumentFormatDefinition<Measurement> GetDefinition()
        {
            var austria = new CultureInfo("de-DE");

            return new DocumentFormatDefinitionBuilder<Measurement>("\t", true)
                .SetCommentString("#")
                .AddColumn(x => x.Name)
                .AddColumn(x => x.DateTime, "dd.MM.yyyy HH:mm:ss")
                .AddColumn(x => x.X, b => b.SetAlignment(ColumnAlignment.Right).SetFormatProvider(austria))
                .AddColumn(x => x.Y, b => b.SetAlignment(ColumnAlignment.Right).SetFormatProvider(austria))
                .AddColumn(x => x.Z, b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.IsActive)
                .Build();
        }
    }
}
