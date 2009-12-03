using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using March;
using arch;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public static MarchOptions CommandLineOptions { get; set; }
        public static readonly string[] args = new []{"-j", "DCL", "-a", @"C:\shared.datastore\repository\personal\dev\projects\March\trunk\data\sample-input.march"};

        [TestInitialize]
        public void Setup()
        {
            CommandLineOptions = new MarchOptions();
            CommandLineOptions.ProcessArgs(args);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var parser = new ModelParser(CommandLineOptions.ArchPath);
            bool parsingWasSuccessful = parser.Parse();
            var arch = parser.Architecture;

        }
    }
}
