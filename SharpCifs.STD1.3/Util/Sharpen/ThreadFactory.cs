using System.Threading.Tasks;

namespace SharpCifs.Util.Sharpen
{
    internal class ThreadFactory
    {
        public Thread NewThread(IRunnable r)
        {
            Thread t = new Thread(r);
            t.SetDaemon(true);

            var started = false;
            t.Start(() => { started = true; });

            //wait for start thread
            while (!started)
                Task.Delay(300).GetAwaiter().GetResult();

            return t;
        }
    }
}
