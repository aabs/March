using System;
using arch;
using log4net.Config;
using Mono.GetOptions;
using MonoOptions = Mono.GetOptions.Options;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace March
{
    public class MarchOptions : MonoOptions
    {
        [Option("(required) Job Type - the kind of code generation task to perform.", 'j', "jobtype")]
        public string JobType { get; set; }

        [Option("(optional) Server to generate for.", 's', "server")]
        public string Server { get; set; }

        [Option("(required) The Architecture Declaration file path.", 'a', "arch")]
        public string ArchPath { get; set; }
    }

    public class Program
    {
        public static MarchOptions CommandLineOptions { get; set; }
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            CommandLineOptions = new MarchOptions();
            CommandLineOptions.ProcessArgs(args);

            if (string.IsNullOrEmpty(CommandLineOptions.ArchPath) || !File.Exists(CommandLineOptions.ArchPath))
            {
                var x = CommandLineOptions.DoUsage();
                return;
            }

            switch (CommandLineOptions.JobType)
            {
                case "DCL": // deployment checklist
                    CreateDeploymentChecklist();
                    break;
                default:
                    break;
            }
            Console.WriteLine("done");
        }

        private static void CreateDeploymentChecklist()
        {
            try
            {
                using (var fs = new FileInfo(@"opt.txt").CreateText())
                {
                    CodeGenHelpers.CodeGenHelper.DumpObjectToFile(
                    new CodeGenHelpers.OptionsDto 
                    {
                        JobType = CommandLineOptions.JobType,
                        Server = CommandLineOptions.Server,
                        ArchPath = CommandLineOptions.ArchPath
                    }
                    , fs);
                }
                
                var p = CreateCodeGeneratorProcess("DCL.txt", "DCL.t4.txt");
                Debug.Write(p.StandardError.ReadToEnd());
                Debug.Write(p.StandardOutput.ReadToEnd());
                p.WaitForExit();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.Message);
                throw;
            }
        }

        private static Process CreateCodeGeneratorProcess(string output, string tpl)
        {
            var p = new Process();
            p.StartInfo.FileName = @"C:\Program Files (x86)\Common Files\microsoft shared\TextTemplating\10.0\texttransform.exe";
            p.StartInfo.Arguments = @"-out " + output + @" ..\..\" + tpl;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.Start();
            return p;
        }
    }
}
