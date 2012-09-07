#region using directives

using System;

#endregion

namespace AsciiImportExport.Tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var performanceTests = new PerformanceTests();
            performanceTests.Test();

            Console.Write("Press any key to exit...");
            Console.ReadLine();
        }
    }
}