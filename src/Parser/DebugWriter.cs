using System.Diagnostics;
using System.IO;
using System.Text;
using Common;

namespace arch
{
    public class DebugWriter : TextWriter
    {
        private readonly Logger logger;

        public DebugWriter(Logger logger)
        {
            this.logger = logger;
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Write(string msg)
        {
            Debug.Write(msg);
        }

        public override void WriteLine(string msg)
        {
            Debug.WriteLine(msg);
        }
    }
}