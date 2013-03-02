using System;
using NUnit.Framework;

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class SpecialityBankFormatTest
    {
        public class ChequeRow
        {
            public enum RecordTypeEnum
            {
                Outstanding,
                Void
            }

            public ulong AccountNo { get; set; }
            public ulong AmountCents { get; set; }
            public DateTime Date { get; set; }
            public RecordTypeEnum RecordType { get; set; }
            public ulong SerialNumber { get; set; }
        }

        private DocumentFormatDefinitionBuilder<ChequeRow> GetDefinition()
        {
            var definition = new DocumentFormatDefinitionBuilder<ChequeRow>(string.Empty, false);
            definition.AddColumn(q => q.AccountNo,
                                 c => c.SetColumnWidth(12).SetAlignment(ColumnAlignment.Right).SetFillChar('0').ThrowOnOverflow());
            definition.AddColumn(q => q.RecordType, c => c.SetExportFunc(EnumConvertFunc));
            definition.AddColumn(q => q.SerialNumber,
                                 c => c.SetColumnWidth(10).SetAlignment(ColumnAlignment.Right).SetFillChar('0').ThrowOnOverflow());
            definition.AddColumn(q => q.AmountCents,
                                 c => c.SetColumnWidth(12).SetAlignment(ColumnAlignment.Right).SetFillChar('0').ThrowOnOverflow());
            definition.AddColumn(q => q.Date,
                                 c => c.SetColumnWidth(8).SetStringFormat("yyyyMMdd"));
            definition.AddDummyColumn(1);
            definition.AddDummyColumn(50);
            definition.AddDummyColumn(16);
            definition.AddDummyColumn(256);
            definition.AddDummyColumn(16);
            definition.SetExportHeaderLine(false);
            definition.SetTrimLineEnds(false);
            return definition;
        }

        private string EnumConvertFunc(ChequeRow arg1, ChequeRow.RecordTypeEnum arg2)
        {
            return arg2 == ChequeRow.RecordTypeEnum.Outstanding ? "O" : "I";
        }

        [Test]
        public void DataOverflowInColumn_ThrowsExportException()
        {
            DocumentFormatDefinitionBuilder<ChequeRow> definition = GetDefinition();
            var record = new ChequeRow()
                             {
                                 AccountNo = 1234567890123 // Overflowed
                             };
            IDocumentFormatDefinition<ChequeRow> document = definition.Build();
            Assert.Throws<ExportException>(() => document.Export(new ChequeRow[] {record}));
        }

        [Test]
        public void RightJustifiedValues_AreFilledUpWithFillChar()
        {
            DocumentFormatDefinitionBuilder<ChequeRow> definition = GetDefinition();

            var record = new ChequeRow()
                             {
                                 AccountNo = 123456789,
                                 RecordType = ChequeRow.RecordTypeEnum.Outstanding,
                                 AmountCents = 152,
                                 SerialNumber = 123,
                                 Date = new DateTime(2013, 12, 31)
                             };
            string resultString = definition.Build().Export(new ChequeRow[] {record});
            Assert.That(resultString,
                        Is.EqualTo("000123456789O000000012300000000015220131231" +
                                   new string(' ', 1 + 50 + 16 + 256 + 16)));
        }

        [Test]
        public void ValuesWithFullWidth_ExportedByFillingOutFullSpace()
        {
            DocumentFormatDefinitionBuilder<ChequeRow> definition = GetDefinition();

            var record = new ChequeRow()
                             {
                                 AccountNo = 123456789012,
                                 RecordType = ChequeRow.RecordTypeEnum.Outstanding,
                                 AmountCents = 210987654321,
                                 SerialNumber = 9876543210,
                                 Date = new DateTime(2013, 12, 31)
                             };
            string resultString = definition.Build().Export(new ChequeRow[] {record});
            Assert.That(resultString,
                        Is.EqualTo("123456789012O987654321021098765432120131231" +
                                   new string(' ', 1 + 50 + 16 + 256 + 16)));
        }
    }
}