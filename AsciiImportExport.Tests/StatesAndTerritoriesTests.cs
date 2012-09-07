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
        private IDocumentFormatDefinition<State> GetDefinition()
        {
            // Possible Values are:
            // Empty              the state is not a member of the EU, leave property EU_MEMBER null
            // 'N'                the state is not a member of the EU, so leave the property Year empty
            // 'J, seit yyyy'     the state is a member of the EU, so set property Year accordingly
            IDocumentFormatDefinition<State.MemberOfEU> memberOfEuColumnDefinition = new DocumentFormatDefinitionBuilder<State.MemberOfEU>(",", false)
                .SetLineEndsWithColumnSeparator(false)
                .AddColumn(x => x.IsMember, "J", "N")
                .AddColumn(x => x.Year, builder => builder
                                                       .SetImportFunc(s =>
                                                                          {
                                                                              string year = s.Substring(5);
                                                                              return Int32.Parse(year);
                                                                          })
                                                       .SetExportFunc(i => i.HasValue ? " seit " + i.Value : ""))
                .Build();


            IDocumentFormatDefinition<State> definition = new DocumentFormatDefinitionBuilder<State>(";", false)
                .SetInstantiator(() => new State())
                .SetExportHeaderLine(true)
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
                    .SetImportFunc(s => memberOfEuColumnDefinition.Import(new StringReader(s)).Single())
                    .SetExportFunc(m => memberOfEuColumnDefinition.Export(new [] { m }).TrimEnd(',')))
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
            IDocumentFormatDefinition<State> definition = GetDefinition();

            using (var reader = new StreamReader("Data\\states.csv", Encoding.GetEncoding(1252)))
            {
                List<State> allStates = definition.Import(reader, 1);
                List<State> membersOfEu = allStates.Where(x => x.EU_MEMBER != null && x.EU_MEMBER.IsMember).OrderBy(x => x.EU_MEMBER.Year).ThenBy(x => x.NAME_ENG).ToList();

                Console.WriteLine("Members of EU: " + membersOfEu.Count);

                foreach (var euCountry in membersOfEu)
                {
                    Console.WriteLine(euCountry.NAME_ENG + " (" + euCountry.EU_MEMBER.Year + ")");
                }

                Assert.AreEqual(27, membersOfEu.Count);
            }
        }


        [Test]
        public void ImportExport()
        {
            IDocumentFormatDefinition<State> definition = GetDefinition();
            using (var reader = new StreamReader("Data\\states.csv", Encoding.GetEncoding(1252)))
            {
                string fileContent = reader.ReadToEnd();

                List<State> allStates = definition.Import(new StringReader(fileContent), 1);

                var exportResult = definition.Export(allStates);

                Assert.AreEqual(fileContent, exportResult);
            }
        }
    }
}