#region using directives

using System;

#endregion

namespace AsciiImportExport.Tests.Pocos
{
    internal class Person
    {
        public DateTime Birthday { get; set; }
        public Gender Gender { get; set; }
        public float Height { get; set; }
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