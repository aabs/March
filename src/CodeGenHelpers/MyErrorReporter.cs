using System.Collections.Generic;
using System.Dataflow;
using System.IO;
using System.Text;
using System.Diagnostics;
using Common;

namespace arch
{
    public class MyErrorReporter : ErrorReporter
    {
        private static readonly Logger Logger = new Logger(typeof(MyErrorReporter));
        List<ErrorInformation> Errors = new List<ErrorInformation>();
        protected override void OnError(ErrorInformation errorInformation)
        {
            Logger.Error("{0} {1}: {2}.", errorInformation.Location.FileName, errorInformation.Location, errorInformation);
            Errors.Add(errorInformation);
        }

        public string ErrorDump()
        {
            var sb = new StringBuilder();
            var tw = new StringWriter(sb);
            foreach (var error in Errors)
            {
                tw.Write(error.Location.Span.ToString());
                tw.Write("\t");
                tw.Write(error.Location.FileName);
                tw.Write("\t");
                tw.Write(error.Message);
            }
            return sb.ToString();
        }

        public void GetErrorList(TextWriter tw)
        {
            foreach (var error in Errors)
            {
                tw.Write(error.Location.Span.ToString());
                tw.Write("\t");
                tw.Write(error.Location.FileName);
                tw.Write("\t");
                tw.Write(error.Message);
            }
        }
    }
}