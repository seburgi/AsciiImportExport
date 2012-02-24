using System;
using AsciiImportExport.Tests.Performance;

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