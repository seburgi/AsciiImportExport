#region using directives

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests.Autoresize
{
    [TestFixture]
    internal class AutoresizeTests
    {
        public List<AutoresizePoco> GetPocoList()
        {
            return new List<AutoresizePoco>
                       {
                           new AutoresizePoco {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                           new AutoresizePoco {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                           new AutoresizePoco {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                       };
        }


        private static DocumentFormatDefinition<AutoresizePoco> GetDefinition_With_Tab_As_ColumnSeparator()
        {
            return new DocumentFormatDefinitionBuilder<AutoresizePoco>()
                .SetColumnSeparator("\t")
                .SetCommentString("!")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.Gender, ColumnAlignment.Left, StringToGender, GenderToString)
                .AddColumn(x => x.Height, ColumnAlignment.Right, "0.00")
                .AddColumn(x => x.Birthday, "yyyyMMdd")
                .AddColumn(x => x.Memo)
                .Build();
        }

        private static DocumentFormatDefinition<AutoresizePoco> GetDefinition_With_Space_As_ColumnSeparator()
        {
            return new DocumentFormatDefinitionBuilder<AutoresizePoco>()
                .SetColumnSeparator(" ")
                .SetCommentString("!")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.Gender, ColumnAlignment.Left, StringToGender, GenderToString)
                .AddColumn(x => x.Height, ColumnAlignment.Right, "0.00")
                .AddColumn(x => x.Birthday, "yyyyMMdd")
                .AddColumn(x => x.Memo)
                .Build();
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

        private void ExportImportExport(DocumentFormatDefinition<AutoresizePoco> definition)
        {
            string exportData1 = definition.Export(GetPocoList());
            List<AutoresizePoco> importResult = definition.Import(exportData1);
            string exportData2 = definition.Export(importResult);

            Assert.AreEqual(exportData1, exportData2);
        }


        private void Export(DocumentFormatDefinition<AutoresizePoco> definition, string fileName)
        {
            string result = definition.Export(GetPocoList());
            Assert.AreEqual(File.ReadAllText(fileName), result);
        }


        private void Import(DocumentFormatDefinition<AutoresizePoco> definition, string fileName)
        {
            List<AutoresizePoco> result = definition.Import(File.ReadAllText(fileName));
            List<AutoresizePoco> expected = GetPocoList();

            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expected[i].Birthday, result[i].Birthday);
                Assert.AreEqual(expected[i].Gender, result[i].Gender);
                Assert.AreEqual(expected[i].Height, result[i].Height);
                Assert.AreEqual(expected[i].Name, result[i].Name);
                Assert.AreEqual(expected[i].Memo, result[i].Memo);
            }
        }

        [Test]
        public void SpaceExport()
        {
            Export(GetDefinition_With_Space_As_ColumnSeparator(), "Autoresize\\space.txt");
        }

        [Test]
        public void SpaceExportImportExport()
        {
            ExportImportExport(GetDefinition_With_Space_As_ColumnSeparator());
        }

        [Test]
        public void SpaceImport()
        {
            Import(GetDefinition_With_Space_As_ColumnSeparator(), "Autoresize\\space.txt");
        }

        [Test]
        public void TabExport()
        {
            Export(GetDefinition_With_Tab_As_ColumnSeparator(), "Autoresize\\tab.txt");
        }

        [Test]
        public void TabExportImportExport()
        {
            ExportImportExport(GetDefinition_With_Tab_As_ColumnSeparator());
        }

        [Test]
        public void TabImport()
        {
            Import(GetDefinition_With_Tab_As_ColumnSeparator(), "Autoresize\\tab.txt");
        }
    }
}