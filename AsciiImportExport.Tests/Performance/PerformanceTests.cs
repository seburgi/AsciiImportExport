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
            var list = CreateList(100000);
            var definition = GetDefinition();

            Stopwatch sw = Stopwatch.StartNew();
            definition.Export(list);
            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);
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
                                 X = r.NextDouble() * 100000,
                                 Y = r.NextDouble() * 100000,
                                 Z = r.NextDouble() * 100000,
                                 IsActive = r.NextDouble() > 0.5
                             });
            }

            return list;
        }
    }
}
