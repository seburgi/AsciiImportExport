﻿#region using directives

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
        public List<Person> GetPocoList()
        {
            return new List<Person>
                       {
                           new Person {Birthday = new DateTime(1983, 1, 29), Gender = Gender.Male, Height = 175.5, Name = "Peter", Memo = "Nice guy!"},
                           new Person {Birthday = new DateTime(1931, 10, 5), Gender = Gender.Male, Height = 173.45, Name = "Paul", Memo = "Sometimes a litte grumpy."},
                           new Person {Birthday = new DateTime(1980, 4, 12), Gender = Gender.Female, Height = 1193, Name = "Mary", Memo = "Tall!"},
                       };
        }


        [Test]
        public void SpaceExport()
        {
            Export(GetDefinition_With_Space_As_ColumnSeparator(), "Data\\space.txt");
        }

        [Test]
        public void SpaceExportImportExport()
        {
            ExportImportExport(GetDefinition_With_Space_As_ColumnSeparator());
        }

        [Test]
        public void SpaceImport()
        {
            Import(GetDefinition_With_Space_As_ColumnSeparator(), "Data\\space.txt");
        }

        [Test]
        public void TabExport()
        {
            Export(GetDefinition_With_Tab_As_ColumnSeparator(), "Data\\tab.txt");
        }

        [Test]
        public void TabExportImportExport()
        {
            ExportImportExport(GetDefinition_With_Tab_As_ColumnSeparator());
        }

        [Test]
        public void TabImport()
        {
            Import(GetDefinition_With_Tab_As_ColumnSeparator(), "Data\\tab.txt");
        }

        private void Export(DocumentFormatDefinition<Person> definition, string fileName)
        {
            string result = definition.Export(GetPocoList());
            Assert.AreEqual(File.ReadAllText(fileName), result);
        }

        private void ExportImportExport(DocumentFormatDefinition<Person> definition)
        {
            string exportData1 = definition.Export(GetPocoList());
            List<Person> importResult = definition.Import(exportData1);
            string exportData2 = definition.Export(importResult);

            Assert.AreEqual(exportData1, exportData2);
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

        private static DocumentFormatDefinition<Person> GetDefinition_With_Space_As_ColumnSeparator()
        {
            return new DocumentFormatDefinitionBuilder<Person>()
                .SetColumnSeparator(" ")
                .SetCommentString("!")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.Gender, b => b.SetImportFunc(StringToGender).SetExportFunc(GenderToString))
                .AddColumn(x => x.Height, "0.00", b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.Birthday, "yyyyMMdd")
                .AddColumn(x => x.Memo)
                .Build();
        }

        private static DocumentFormatDefinition<Person> GetDefinition_With_Tab_As_ColumnSeparator()
        {
            return new DocumentFormatDefinitionBuilder<Person>()
                .SetColumnSeparator("\t")
                .SetCommentString("!")
                .SetAutosizeColumns(true)
                .SetExportHeaderLine(false)
                .AddColumn(x => x.Name)
                .AddColumn(x => x.Gender, b => b.SetImportFunc(StringToGender).SetExportFunc(GenderToString))
                .AddColumn(x => x.Height, "0.00", b => b.SetAlignment(ColumnAlignment.Right))
                .AddColumn(x => x.Birthday, "yyyyMMdd")
                .AddColumn(x => x.Memo)
                .Build();
        }

        private void Import(DocumentFormatDefinition<Person> definition, string fileName)
        {
            List<Person> result = definition.Import(File.ReadAllText(fileName));
            List<Person> expected = GetPocoList();

            for (int i = 0; i < result.Count; i++)
            {
                Assert.AreEqual(expected[i].Birthday, result[i].Birthday);
                Assert.AreEqual(expected[i].Gender, result[i].Gender);
                Assert.AreEqual(expected[i].Height, result[i].Height);
                Assert.AreEqual(expected[i].Name, result[i].Name);
                Assert.AreEqual(expected[i].Memo, result[i].Memo);
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
    }
}