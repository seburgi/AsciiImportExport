#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    internal class AutosizeTests
    {
        [Test]
        public void Export()
        {
            string result = GetDefinition().Export(GetPocoList());
            Assert.AreEqual(Text, result);
        }

        [Test]
        public void ExportImportExport()
        {
            IDocumentFormatDefinition<Person> definition = GetDefinition();
            string exportData1 = definition.Export(GetPocoList());
            List<Person> importResult = definition.Import(new StringReader(exportData1));
            string exportData2 = definition.Export(importResult);

            Assert.AreEqual(exportData1, exportData2);
        }

        [Test]
        public void Import()
        {
            List<Person> result = GetDefinition().Import(new StringReader(Text));
            List<Person> expected = GetPocoList();

            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expected[i].Birthday, result[i].Birthday);
                Assert.AreEqual(expected[i].Gender, result[i].Gender);
                Assert.AreEqual(expected[i].Height, result[i].Height, 0.001);
                Assert.AreEqual(expected[i].Name, result[i].Name);
                Assert.AreEqual(expected[i].Memo, result[i].Memo);
            }
        }

        private static string GenderToString(Gender gender)
        {
            switch (gender)
            {
                case Gender.Female:
                    return "F";
                case Gender.Male:
                    return "M";
                default:
                    return "U";
            }
        }

        private static IDocumentFormatDefinition<Person> GetDefinition()
        {
            return new DocumentFormatDefinitionBuilder<Person>("\t", true)
                .SetCommentString("#")
                .SetExportHeaderLine(true, "# ")
                .AddColumn(x => x.Name)
                .AddColumn(x => x.Gender, b => b.SetImportFunc(StringToGender).SetExportFunc(GenderToString))
                .AddColumn(x => x.Height, "0.00", b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.Birthday, "yyyyMMdd")
                .AddColumn(x => x.Memo, b => b.SetHeader("Description"))
                .Build();
        }

        private List<Person> GetPocoList()
        {
            return new List<Person>
                       {
                           new Person {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5f, Name = "Peter", Memo = "Nice guy!"},
                           new Person {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45f, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                           new Person {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193f, Name = "Mary", Memo = "Tall!"},
                       };
        }

        private static Gender StringToGender(string s)
        {
            switch (s)
            {
                case "M":
                    return Gender.Male;
                case "F":
                    return Gender.Female;
                default:
                    return Gender.Unknown;
            }
        }

        private const string Text =
@"# Name	Gender	 Height	Birthday	Description
Peter 	M     	 175.50	19830129	Nice guy!
Paul  	M     	 173.45	19311005	Sometimes a litte grumpy.
Mary  	F     	1193.00	19800412	Tall!";

    }
}