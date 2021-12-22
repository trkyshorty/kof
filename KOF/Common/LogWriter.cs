using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace KOF.Common
{
    class LogWriter : TextWriter
    {
        StreamWriter _StreamWriter;

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public LogWriter(FileStream FileStream)
        {
            _StreamWriter = new StreamWriter(FileStream);
            _StreamWriter.AutoFlush = true;
        }

        public override void WriteLine(string value)
        {
            _StreamWriter.WriteLine(String.Format("{0} :: {1}: {2}", DateTime.Now.ToString("MMM dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture), (new System.Diagnostics.StackTrace()).GetFrame(4).GetMethod().Name, value));
        }
    }
}
