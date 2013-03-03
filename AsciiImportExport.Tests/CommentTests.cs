#region using directives

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

#endregion

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void Test_CommentString_Is_Null()
        {
            const string importData = @"001-01  12.11 1222.87 334.32
001-02 223.91  778.64 234.24
002-01 106.18  345.33 789.32
002-02  22.60  233.22 321.88 #34234
                                             
003-01  98.31  983.33 564.89
003-02 345.77  324.21 123.94
";

            IDocumentFormatDefinition<Location> definition = GetDefinition(null);

            List<Location> importList = definition.Import(new StringReader(importData));

            Assert.AreEqual(6, importList.Count());

            Compare(importList[0], "001-01", 12.11, 1222.87, 334.32);
            Compare(importList[1], "001-02", 223.91, 778.64, 234.24);
            Compare(importList[2], "002-01", 106.18, 345.33, 789.32);
            Compare(importList[3], "002-02", 22.60, 233.22, 321.88);
            Compare(importList[4], "003-01", 98.31, 983.33, 564.89);
            Compare(importList[5], "003-02", 345.77, 324.21, 123.94);
        }

        [Test]
        public void Test_With_Comments()
        {
            const string importData = @"001-01  12.11 1222.87 334.32 #Test
001-02 223.91  778.64 234.24
002-01 106.18  345.33 789.32
002-02  22.60  233.22 321.88 #Test
# 002-02 106.18  345.33 789.32
 # 002-02 106.18  345.33 789.32
003-01  98.31  983.33 564.89
003-02 345.77  324.21 123.94
#003-02 345.77  324.21 123.94
";

            IDocumentFormatDefinition<Location> definition = GetDefinition("#");

            List<Location> importList = definition.Import(new StringReader(importData));

            Assert.AreEqual(6, importList.Count());

            Compare(importList[0], "001-01", 12.11, 1222.87, 334.32);
            Compare(importList[1], "001-02", 223.91, 778.64, 234.24);
            Compare(importList[2], "002-01", 106.18, 345.33, 789.32);
            Compare(importList[3], "002-02", 22.60, 233.22, 321.88);
            Compare(importList[4], "003-01", 98.31, 983.33, 564.89);
            Compare(importList[5], "003-02", 345.77, 324.21, 123.94);
        }

        [Test]
        public void Test_With_Comments_In_The_Middle_of_The_Line()
        {
            const string importData = @"001-01  12.11 1222.87 334.32
001-02 223.91  778.64 234.24
002-01 106.18  #345.33 789.32
002-02  22.60  233.22 321.88
003-01  98.31  983.33 564.89
003-02 #345.77  324.21 123.94
";

            IDocumentFormatDefinition<Location> definition = GetDefinition("#");

            List<Location> importList = definition.Import(new StringReader(importData));

            Assert.AreEqual(6, importList.Count());

            Compare(importList[0], "001-01", 12.11, 1222.87, 334.32);
            Compare(importList[1], "001-02", 223.91, 778.64, 234.24);
            Compare(importList[2], "002-01", 106.18, 0, 0);
            Compare(importList[3], "002-02", 22.60, 233.22, 321.88);
            Compare(importList[4], "003-01", 98.31, 983.33, 564.89);
            Compare(importList[5], "003-02", 0, 0, 0);
        }

        [Test]
        public void Test_Without_Comments()
        {
            const string importData = @"001-01  12.11 1222.87 334.32
001-02 223.91  778.64 234.24
002-01 106.18  345.33 789.32
002-02  22.60  233.22 321.88
003-01  98.31  983.33 564.89
003-02 345.77  324.21 123.94
";

            IDocumentFormatDefinition<Location> definition = GetDefinition("#");

            List<Location> importList = definition.Import(new StringReader(importData));

            Assert.AreEqual(6, importList.Count());

            Compare(importList[0], "001-01", 12.11, 1222.87, 334.32);
            Compare(importList[1], "001-02", 223.91, 778.64, 234.24);
            Compare(importList[2], "002-01", 106.18, 345.33, 789.32);
            Compare(importList[3], "002-02", 22.60, 233.22, 321.88);
            Compare(importList[4], "003-01", 98.31, 983.33, 564.89);
            Compare(importList[5], "003-02", 345.77, 324.21, 123.94);
        }

        private void Compare(Location location, string name, double x, double y, double z)
        {
            Assert.AreEqual(name, location.Name);
            Assert.AreEqual(x, location.X);
            Assert.AreEqual(y, location.Y);
            Assert.AreEqual(z, location.Z);
        }

        private IDocumentFormatDefinition<Location> GetDefinition(string commentString)
        {
            return new DocumentFormatDefinitionBuilder<Location>(" ", false)
                .SetCommentString(commentString)
                .AddColumn(x => x.Name, b => b.SetColumnWidth(6))
                .AddColumn(x => x.X, "0.00", b => b.SetColumnWidth(6))
                .AddColumn(x => x.Y, "0.00", b => b.SetColumnWidth(7))
                .AddColumn(x => x.Z, "0.00", b => b.SetColumnWidth(6))
                .Build();
        }
    }
}