#region using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class PerformanceTests
    {
        [Test]
        public void Test()
        {
            const int count = 1000000;
            List<Measurement> list = CreateList(count);
            DocumentFormatDefinition<Measurement> definition = GetDefinition();

            Stopwatch sw = Stopwatch.StartNew();
            string exportResult = definition.Export(list);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            double perSec = Math.Round(count/(double) sw.ElapsedMilliseconds*1000);
            Console.WriteLine("Export: " + perSec + " pocos/sec");
            Console.WriteLine();

            sw = Stopwatch.StartNew();
            List<Measurement> importList = definition.Import(exportResult);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
            perSec = Math.Round(count/(double) sw.ElapsedMilliseconds*1000);
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

        private List<Measurement> CreateList(int count)
        {
            var list = new List<Measurement>();
            var r = new Random();
            for (int i = 0; i < count; i++)
            {
                list.Add(new Measurement
                             {
                                 Name = "Poco-" + count,
                                 DateTime = DateTime.Now,
                                 X = Math.Round(r.NextDouble()*100000, 4),
                                 Y = Math.Round(r.NextDouble()*100000, 4),
                                 Z = Math.Round(r.NextDouble()*100000, 4),
                                 IsActive = r.NextDouble() > 0.5
                             });
            }

            return list;
        }

        private DocumentFormatDefinition<Measurement> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<Measurement>("\t", true)
                .SetCommentString("#")
                .SetExportHeaderLine(false)
                .SetInstantiator(() => new Measurement())
                .AddColumn(x => x.Name)
                .AddColumn(x => x.DateTime, "dd.MM.yyyy HH:mm:ss")
                .AddColumn(x => x.X, b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.Y, b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.Z, b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.IsActive)
                .Build();
        }
    }
}