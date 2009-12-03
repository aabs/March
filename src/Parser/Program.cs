using System;
using System.Collections.Generic;
using System.Dataflow;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common;
using DomainModel.Ast;
using log4net.Config;
using MGraphXamlReader;
using Identifier=System.Dataflow.Identifier;
using Type=System.Type;

namespace arch
{
    public class Program
    {
//cd $(ProjectDir) "C:\Program Files\Microsoft Oslo SDK 1.0\Bin\mg.exe" "arch.mg"
        private static readonly Logger Logger = new Logger(typeof (Program));

        public static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            const string input = @"C:\shared.datastore\repository\personal\dev\projects\March\trunk\data\sample-input.march";
            var parser = new ModelParser(input);
            Logger.Debug("Parsing {0}", input);
            bool parsingWasSuccessful = parser.Parse();
            Debug.WriteLineIf(parsingWasSuccessful, "Successfully parsed input");
            Debug.WriteLineIf(!parsingWasSuccessful, "Error - failed to parse the input.");
            Debug.WriteLineIf(!parsingWasSuccessful && (parser.err.ErrorCount > 0), parser.err.ErrorDump());
        }
    }


}