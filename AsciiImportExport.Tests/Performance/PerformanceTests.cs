using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AsciiImportExport.Tests.Performance
{
    [TestFixture]
    class PerformanceTests
    {
        [Test]
        public void Test()
        {
            const int count = 100000;
            var list = CreateList(count);
            var definition = GetDefinition();

            Stopwatch sw = Stopwatch.StartNew();
            var exportResult = definition.Export(list);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            var perSec = Math.Round(count/(double) sw.ElapsedMilliseconds*1000);
            Console.WriteLine("Export: " + perSec + " pocos/sec");
            Console.WriteLine();

            sw = Stopwatch.StartNew();
            var importList = definition.Import(exportResult);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            perSec = Math.Round(count / (double)sw.ElapsedMilliseconds * 1000);
            Console.WriteLine("Import: " + perSec + " pocos/sec");


            for (int i = 0; i < count; i++)
            {
                Assert.AreEqual(list[i].Name, importList[i].Name);
                Assert.IsTrue(Math.Abs((list[i].DateTime - importList[i].DateTime).TotalMinutes) < 1);
                Assert.AreEqual(list[i].X, importList[i].X);
                Assert.AreEqual(list[i].Y, importList[i].Y);
                Assert.AreEqual(list[i].Z, importList[i].Z);
                Assert.AreEqual(list[i].IsActive, importList[i].IsActive);
            }
        }


        private DocumentFormatDefinition<PerformancePoco> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<PerformancePoco>()
                .SetColumnSeparator("\t")
                .SetCommentString("#")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.Name))
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.DateTime))
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.X).SetDoublePrecision(4).SetAlignment(ColumnAlignment.Right))
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.Y).SetDoublePrecision(4).SetAlignment(ColumnAlignment.Right))
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.Z).SetDoublePrecision(4).SetAlignment(ColumnAlignment.Right))
                .AddColumn(new DocumentColumn<PerformancePoco>(x => x.IsActive))
                .Build();
        }

        private List<PerformancePoco> CreateList(int count)
        {
            var list = new List<PerformancePoco>();
            var r = new Random();
            for (int i = 0; i < count; i++)
            {
                list.Add(new PerformancePoco
                             {
                                 Name = "Poco-" + count,
                                 DateTime = DateTime.Now,
                                 X = Math.Round(r.NextDouble() * 100000, 4),
                                 Y = Math.Round(r.NextDouble() * 100000, 4),
                                 Z = Math.Round(r.NextDouble() * 100000, 4),
                                 IsActive = r.NextDouble() > 0.5
                             });
            }

            return list;
        }
    }
}
