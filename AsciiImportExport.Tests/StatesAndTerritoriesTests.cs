#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class StatesAndTerritoriesTests
    {
        private DocumentFormatDefinition<State> GetDefinition()
        {
            // Possible Values are:
            // Empty              the state is not a member of the EU, take the default value of the column
            // 'N'                the state is not a member of the EU, so leave the property Year empty
            // 'J, seit yyyy'     the state is a member of the EU, so set property Year accordingly
            DocumentFormatDefinition<State.MemberOfEU> memberOfEuColumnDefinition = new DocumentFormatDefinitionBuilder<State.MemberOfEU>(",", false)
                .SetExportHeaderLine(false)
                .SetInstantiator(() => new State.MemberOfEU())
                .AddColumn(x => x.IsMember, "J", "N")
                .AddColumn(x => x.Year, builder => builder
                                                       .SetImportFunc(s =>
                                                                          {
                                                                              string year = s.Substring(5);
                                                                              return Int32.Parse(year);
                                                                          })
                                                       .SetExportFunc(i => i.HasValue ? "seit " + i.Value : ""))
                .Build();


            DocumentFormatDefinition<State> definition = new DocumentFormatDefinitionBuilder<State>(";", false)
                .SetExportHeaderLine(true)
                .SetInstantiator(() => new State())
                .AddColumn(x => x.ISO_NUM)
                .AddColumn(x => x.ISO_2)
                .AddColumn(x => x.ISO_3)
                .AddColumn(x => x.NAME_GER)
                .AddColumn(x => x.NAME_GER_OFF)
                .AddColumn(x => x.NAME_ENG)
                .AddColumn(x => x.NAME_ENG_OFF)
                .AddColumn(x => x.STATUS)
                .AddColumn(x => x.ASSOCIATED)
                .AddColumn(x => x.DOCUMENT)
                .AddColumn(x => x.EU_MEMBER, builder => builder
                    .SetDefaultValue(() => new State.MemberOfEU())
                    .SetImportFunc(s => memberOfEuColumnDefinition.Import(s).Single())
                    .SetExportFunc(m => memberOfEuColumnDefinition.Export(new [] { m })))
                .AddColumn(x => x.PART_OF_EU, "J", "N")
                .AddColumn(x => x.CONTINENT)
                .AddColumn(x => x.PREFIX)
                .AddColumn(x => x.TLD)
                .AddColumn(x => x.CAR_PLATE)
                .AddColumn(x => x.DATE_OF_FORMATION)
                .AddColumn(x => x.DATE_OF_RESOLUTION)
                .AddColumn(x => x.EXIST_ADD)
                .Build();

            return definition;
        }

        [Test]
        public void Import()
        {
            DocumentFormatDefinition<State> definition = GetDefinition();
            string data = File.ReadAllText("Data\\states.csv", Encoding.GetEncoding(1252));
            List<State> result = definition.Import(data.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries).Skip(1));

            List<State> euCountries = result.Where(x => x.EU_MEMBER.IsMember).OrderBy(x => x.EU_MEMBER.Year).ThenBy(x => x.NAME_ENG).ToList();

            Console.WriteLine("Members of EU: " + euCountries.Count);

            foreach (var euCountry in euCountries)
            {
                Console.WriteLine(euCountry.NAME_ENG + " (" + euCountry.EU_MEMBER.Year + ")");
            }

            Assert.AreEqual(27, euCountries.Count);
        }


        [Test]
        public void ImportExport()
        {
            DocumentFormatDefinition<State> definition = GetDefinition();
            string data = File.ReadAllText("Data\\states.csv", Encoding.GetEncoding(1252));
            List<State> importResult = definition.Import(data.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1));
            var exportResult = definition.Export(importResult);
            List<State> importResult2 = definition.Import(exportResult.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1));

            Assert.AreEqual(importResult.Count, importResult2.Count);
            for(int i=0; i < importResult.Count; i++)
            {
                Assert.AreEqual(importResult[i], importResult2[i]);
            }
        }
    }
}