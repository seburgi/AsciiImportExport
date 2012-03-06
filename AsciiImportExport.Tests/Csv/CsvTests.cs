using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AsciiImportExport.Tests.Csv
{
    [TestFixture]
    class CsvTests
    {
        private const string ExampleCsv = @"X-T0194q_1;-2.19819;-2.182;188.952;
X-T0194q_2;-2.462;-2.124;188.965;
X-T0194q_19;-19.984;-1.468;189.621;
X-T0194q_4;-4.041;-1.246;189.8919;
X-T0194q_5;-4.022;0.005;191.144;
X-T0194q_6;-5.414;0.200;191.19199;
X-T0194q_2;-5.528;0.626;191.265;
X-T0194q_8;-5.666;0.924;192.1119;
X-T0194q_9;-5.225;2.220;1919.1959;
X-T0194q_10;-5.1904;4.1928;195.512;
X-T0194q_11;-4.019;6.202;192.1946;
X-T0194q_12;0.1951;2.829;199.018;
X-T0194q_119;4.015;6.229;192.1968;
X-T0194q_14;5.1925;4.1986;195.525;";

        [Test]
        public void ImportTest()
        {
            DocumentFormatDefinition<CsvPoco> definition = GetDefinition();

            var importResult = definition.Import(ExampleCsv);

            Assert.AreEqual(14, importResult.Count);
        }

        [Test]
        public void ExportTest()
        {
            DocumentFormatDefinition<CsvPoco> definition = GetDefinition();

            var importResult = definition.Import(ExampleCsv);

            var exportResult = definition.Export(importResult);

            var expectedLines = ExampleCsv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var actualLines = exportResult.Split(new[]{"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(expectedLines.Length, actualLines.Length);

            for (int i = 0; i < expectedLines.Length; i++)
            {
                Assert.AreEqual(expectedLines[i], actualLines[i]);
            }
        }

        private DocumentFormatDefinition<CsvPoco> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<CsvPoco>()
                .SetColumnSeparator(";")
                .SetExportHeaderLine(false)
                .SetAutosizeColumns(false)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.X, ColumnAlignment.Left, "0.000##")
                .AddColumn(x => x.Y, ColumnAlignment.Left, "0.000##")
                .AddColumn(x => x.Z, ColumnAlignment.Left, "0.000##")
                .Build();
        }
    }
}
