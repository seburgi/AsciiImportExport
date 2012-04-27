#region using directives

using System;
using System.Collections.Generic;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class CsvTests
    {
        [Test]
        public void ExportTest()
        {
            IDocumentFormatDefinition<Location> definition = GetDefinition();

            List<Location> importResult = definition.Import(ExampleCsv);

            string exportResult = definition.Export(importResult);

            string[] expectedLines = ExampleCsv.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
            string[] actualLines = exportResult.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(expectedLines.Length, actualLines.Length);

            for (int i = 0; i < expectedLines.Length; i++)
            {
                Assert.AreEqual(expectedLines[i], actualLines[i]);
            }
        }

        [Test]
        public void ImportTest()
        {
            IDocumentFormatDefinition<Location> definition = GetDefinition();

            List<Location> importResult = definition.Import(ExampleCsv);

            Assert.AreEqual(14, importResult.Count);

            Compare(importResult[0], "X-T0191q_1", -2.19819, -2.182, 188.952);
            Compare(importResult[5], "X-T0196q_6", -5.414, 0.200, 191.19199);
            Compare(importResult[13], "X-T0204q_14", 5.1925, 4.1986, 195.525);
        }

        private void Compare(Location location, string name, double x, double y, double z)
        {
            Assert.AreEqual(name, location.Name);
            Assert.AreEqual(x, location.X);
            Assert.AreEqual(y, location.Y);
            Assert.AreEqual(z, location.Z);
        }

        private IDocumentFormatDefinition<Location> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<Location>(";", false)
                .SetExportHeaderLine(false)
                .SetLineEndsWithColumnSeparator(true)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.X, "0.000##")
                .AddColumn(x => x.Y, "0.000##")
                .AddColumn(x => x.Z, "0.000##")
                .Build();
        }

        private const string ExampleCsv = @"X-T0191q_1;-2.19819;-2.182;188.952;
X-T0192q_2;-2.462;-2.124;188.965;
X-T0193q_19;-19.984;-1.468;189.621;
X-T0194q_4;-4.041;-1.246;189.8919;
X-T0195q_5;-4.022;0.005;191.144;
X-T0196q_6;-5.414;0.200;191.19199;
X-T0197q_2;-5.528;0.626;191.265;
X-T0198q_8;-5.666;0.924;192.1119;
X-T0199q_9;-5.225;2.220;1919.1959;
X-T0200q_10;-5.1904;4.1928;195.512;
X-T0201q_11;-4.019;6.202;192.1946;
X-T0202q_12;0.1951;2.829;199.018;
X-T0203q_119;4.015;6.229;192.1968;
X-T0204q_14;5.1925;4.1986;195.525;";
    }
}