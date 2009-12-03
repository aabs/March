using Common;
using System.Dataflow;
using DomainModel.Ast;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using MGraphXamlReader;
using Parser.Visitors;
using System.Diagnostics;

namespace arch
{
    public class ModelParser
    {
        private static readonly Logger Logger = new Logger(typeof(ModelParser));
        public MyErrorReporter err = new MyErrorReporter();

        public ModelParser(string sourcePath)
        {
            SourcePath = sourcePath;
            Parser = DynamicParser.LoadFromMgx(@"C:\shared.datastore\repository\personal\dev\projects\March\trunk\src\March\arch.mgx", "Languages.AppSpec");
        }

        public string SourcePath { get; set; }
        public Architecture Architecture { get; set; }
        public DynamicParser Parser { get; private set; }

        public bool Parse()
        {
            Dictionary<System.Dataflow.Identifier, System.Type> xamlMap = GetTypeMap();
            try
            {
                string source = File.ReadAllText(SourcePath);
                Architecture tmp = Parser.Parse<Architecture>(source, xamlMap, err);
                var stb = new SymbolTableBuilder();
                tmp.Visit(stb);
                if (stb.SymbolTable.Errors.Count > 0)
                {
                    var errors = new List<string>(stb.SymbolTable.Errors);
                    errors.ForEach(e => Debug.WriteLine(e));
                    return false;
                }
                Architecture = tmp;
            }
            catch (Exception e)
            {
                Logger.ErrorEx(e, "Unable to parse '{0}'.", SourcePath);
            }
            return Architecture != null;
        }

        public static Dictionary<System.Dataflow.Identifier, System.Type> GetTypeMap()
        {
            return
                typeof(DomainModel.Ast.Identifier)
                    .Assembly
                    .GetTypes()
                    .Where(t => t.Namespace.StartsWith("DomainModel.Ast"))
                    .Where(t => !t.IsAbstract)
                    .ToDictionary
                    (
                    t => (System.Dataflow.Identifier)t.Name,
                    t => t
                    );
        }
    }
}