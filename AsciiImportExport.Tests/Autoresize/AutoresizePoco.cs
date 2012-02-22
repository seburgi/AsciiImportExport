using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsciiImportExport.Tests.Autoresize
{
    class AutoresizePoco
    {
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime Birthday { get; set; }
        public string Memo { get; set; }
        public double Height { get; set; }
    }

    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }
}
