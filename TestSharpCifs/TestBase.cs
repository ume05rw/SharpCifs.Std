using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestXb
{
    public class TestBase : IDisposable
    {
        private TextWriterTraceListener _listener;

        public TestBase()
        {
            this._listener = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(this._listener);
            this.Out("TestBase.Constructor.");
        }

        protected void Out(string message)
        {
            Trace.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")}: {message}");
        }


        protected void OutHighlighted(params System.String[] messages)
        {
            var time = DateTime.Now;
            var list = new List<string>();

            list.Add("");
            list.Add("");
            list.Add(time.ToString("HH:mm:ss.fff") + ":");
            list.Add("##################################################");
            list.Add("#");

            foreach (string message in messages)
            {
                var lines = message.Replace("\r\n", "\n").Replace("\r", "\n").Trim('\n').Split('\n');
                foreach (var line in lines)
                {
                    list.Add($"# {line}");
                }
            }

            list.Add("#");
            list.Add("##################################################");
            list.Add("");
            list.Add("");

            Trace.WriteLine(string.Join("\r\n", list));
        }


        public virtual void Dispose()
        {
            this.Out("TestBase.Dispose.");
            try
            {
                Trace.Listeners.Remove(this._listener);
            }
            catch (Exception)
            {
            }
        }
    }
}
