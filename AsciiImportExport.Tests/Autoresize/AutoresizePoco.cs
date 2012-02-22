#region using directives

using System;

#endregion

namespace AsciiImportExport.Tests.Autoresize
{
    internal class AutoresizePoco
    {
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public double Height { get; set; }
        public string Memo { get; set; }
        public string Name { get; set; }
    }

    public enum Gender
    {
        Unknown,
        Female,
        Male,
    }
}