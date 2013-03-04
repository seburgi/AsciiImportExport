using System;
using System.Collections.Generic;
using System.Reflection;
using AsciiImportExport.Tests.Pocos;
using NUnit.Framework;

namespace AsciiImportExport.Tests
{
    [TestFixture]
    public class PropertyInfoTests
    {
        private readonly Measurement _measurement = new Measurement
                                                        {
                                                            DateTime = new DateTime(2013, 03, 03, 14, 48, 00),
                                                            IsActive = true,
                                                            Name = "Point01",
                                                            X = 10,
                                                            Y = 20,
                                                            Z = 30
                                                        };

        private void AssertAreEqual(List<Measurement> importList)
        {
            Assert.AreEqual(_measurement.Name, importList[0].Name);
            Assert.AreEqual(_measurement.DateTime, importList[0].DateTime);
            Assert.AreEqual(_measurement.X, importList[0].X);
            Assert.AreEqual(_measurement.Y, importList[0].Y);
            Assert.AreEqual(_measurement.Z, importList[0].Z);
            Assert.AreEqual(_measurement.IsActive, importList[0].IsActive);
        }

        [Test]
        public void Add_Columns_Manually_Test()
        {
            Type type = typeof (Measurement);

            DocumentFormatDefinitionBuilder<Measurement> builder = new DocumentFormatDefinitionBuilder<Measurement>("\t", true)
                .SetCommentString("'")
                .SetExportHeaderLine(true, "' ");

            builder.AddColumn(type.GetProperty("Name"));
            builder.AddColumn(type.GetProperty("X"), "0.000");
            builder.AddColumn(type.GetProperty("Y"), "0.000");
            builder.AddColumn(type.GetProperty("Z"), "0.000");
            builder.AddColumn(type.GetProperty("DateTime"), "dd.MM.yyyy HH:mm:ss");
            builder.AddColumn(type.GetProperty("IsActive"));

            IDocumentFormatDefinition<Measurement> definition = builder.Build();

            string exportResult = definition.Export(new[] {_measurement});

            Assert.AreEqual(@"' Name 	X     	Y     	Z     	DateTime           	IsActive
Point01	10.000	20.000	30.000	03.03.2013 14:48:00	T", exportResult);

            List<Measurement> importList = definition.Import(exportResult);
            AssertAreEqual(importList);
        }

        [Test]
        public void AutoBuild_Test()
        {
            IDocumentFormatDefinition<Measurement> definition = DocumentFormatDefinitionBuilder<Measurement>.AutoBuild("\t", true, "'", true, "' ");

            string exportResult = definition.Export(new[] {_measurement});
            List<Measurement> importList = definition.Import(exportResult);

            AssertAreEqual(importList);
        }

        [Test]
        public void PropertyList_Test()
        {
            var propertyList = new List<PropertyInfo>
                                   {
                                       typeof (Measurement).GetProperty("Name"),
                                       typeof (Measurement).GetProperty("X"),
                                       typeof (Measurement).GetProperty("Y"),
                                       typeof (Measurement).GetProperty("Z"),
                                       typeof (Measurement).GetProperty("DateTime"),
                                       typeof (Measurement).GetProperty("IsActive"),
                                   };

            IDocumentFormatDefinition<Measurement> definition = DocumentFormatDefinitionBuilder<Measurement>.Build(propertyList, "\t", true, "'", true, "' ");

            string exportResult = definition.Export(new[] {_measurement});
            List<Measurement> importList = definition.Import(exportResult);

            AssertAreEqual(importList);
        }
    }
}