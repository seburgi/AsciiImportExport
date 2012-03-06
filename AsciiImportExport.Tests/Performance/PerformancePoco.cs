#region using directives

using System;

#endregion

namespace AsciiImportExport.Tests.Performance
{
    internal class PerformancePoco
    {
        public DateTime DateTime { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}