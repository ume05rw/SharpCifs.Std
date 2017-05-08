using System;
using System.Collections.Generic;
using System.Threading;

namespace SharpCifs.Util.Sharpen
{
    public class Thread : IRunnable
    {
        private static ThreadGroup _defaultGroup = new ThreadGroup();
        private bool _interrupted;
        private IRunnable _runnable;
        private ThreadGroup _tgroup;

        //private System.Threading.Thread _thread;
        private string _name = string.Empty;
        private bool _isBackground = true;
        private int? _id = null;
        private System.Threading.Tasks.Task _task = null;
        private System.Threading.CancellationTokenSource _canceller = null;

        public int? Id => this._id;



        [ThreadStatic]
        private static Thread _wrapperThread;

        public Thread() : this(null, null, null)
        {
        }

        public Thread(string name) : this(null, null, name)
        {
        }

        public Thread(ThreadGroup grp, string name) : this(null, grp, name)
        {
        }

        public Thread(IRunnable runnable) : this(runnable, null, null)
        {
        }

        Thread(IRunnable runnable, ThreadGroup grp, string name)
        {
            //_thread = new System.Threading.Thread(InternalRun);

            this._runnable = runnable ?? this;
            _tgroup = grp ?? _defaultGroup;
            _tgroup.Add(this);


            if (name != null)
            {
                //_thread.Name = name;
                this._name = name;
            }
        }

        private Thread(int threadId)
        {
            //_thread = t;
            this._id = threadId;

            _tgroup = _defaultGroup;
            _tgroup.Add(this);
        }

        public static Thread CurrentThread()
        {
            if (_wrapperThread == null)
            {
                _wrapperThread = new Thread(System.Environment.CurrentManagedThreadId);
            }
            return _wrapperThread;
        }

        public string GetName()
        {
            //return _thread.Name;
            return this._name;
        }

        public ThreadGroup GetThreadGroup()
        {
            return _tgroup;
        }

        //moved into Task.Run of Start method
        //private void InternalRun()
        //{
        //    _wrapperThread = this;
        //    try
        //    {
        //        _runnable.Run();
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine(exception);
        //    }
        //    finally
        //    {
        //        _tgroup.Remove(this);
        //    }
        //}

        public static void Yield()
        {
        }

        public void Interrupt()
        {
            //lock (_thread)
            //{
            //    _interrupted = true;
            //    _thread.Interrupt ();
            //    _thread.Abort();
            //}

            this._interrupted = true;
            this._canceller?.Cancel(true);
        }

        public static bool Interrupted()
        {
            if (Thread._wrapperThread == null)
            {
                return false;
            }
            Thread wrapperThread = Thread._wrapperThread;
            lock (wrapperThread)
            {
                bool interrupted = Thread._wrapperThread._interrupted;
                Thread._wrapperThread._interrupted = false;
                return interrupted;
            }
        }

        public bool IsAlive()
        {
            //return _thread.IsAlive;
            if (this._task == null)
                return true; //実行されていない

            //Taskが存在し、続行中のときtrue
            return (!this._task.IsCanceled
                    && !this._task.IsFaulted
                    && !this._task.IsCompleted);
        }

        public void Join()
        {
            //_thread.Join();
            this._task?.Wait();
        }

        public void Join(long timeout)
        {
            //_thread.Join((int)timeout);
            this._task?.Wait((int) timeout);
        }

        public virtual void Run()
        {
        }

        public void SetDaemon(bool daemon)
        {
            //_thread.IsBackground = daemon;
            this._isBackground = daemon;
        }

        public void SetName(string name)
        {
            //_thread.Name = name;
            this._name = name;
        }

        public static void Sleep(long milis)
        {
            //System.Threading.Thread.Sleep((int)milis);
            System.Threading.Tasks.Task.Delay((int) milis).Wait();
        }

        public void Start(Action startedCallback = null)
        {
            //_thread.Start();
            this._canceller = new CancellationTokenSource();
            
            this._task = System.Threading.Tasks.Task.Run(() =>
            {
                //ThreadPool's thread NOT to use Foreground
                //if (!this._isBackground)
                //{
                //    System.Threading.Thread.CurrentThread.IsBackground = false;
                //}

                _wrapperThread = this;
                this._id = System.Environment.CurrentManagedThreadId;

                try
                {
                    System.Threading.Tasks.Task.Delay(10).ContinueWith(t => 
                    {
                        startedCallback?.Invoke();
                    });
                }
                catch(Exception ex)
                {
                    var a = 1;
                }

                try
                {
                    _runnable.Run();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
                finally
                {
                    _tgroup.Remove(this);


                    this._canceller.Dispose();
                    this._canceller = null;
                }
            }, this._canceller.Token);
        }

        public void Abort()
        {
            //_thread.Abort();
            this._canceller?.Cancel(true);
        }


        public bool Equals(Thread thread)
        {
            //渡し値スレッドがnullのとき、合致しない
            if (thread == null)
                return false;

            //自身か渡し値スレッドが実行されていないとき、合致しない
            if (this.Id == null
                || thread.Id == null)
                return false;

            return (this.Id == thread.Id);
        }

    }

    public class ThreadGroup
    {
        private List<Thread> _threads = new List<Thread>();

        public ThreadGroup()
        {
        }

        public ThreadGroup(string name)
        {
        }

        internal void Add(Thread t)
        {
            lock (_threads)
            {
                _threads.Add(t);
            }
        }

        internal void Remove(Thread t)
        {
            lock (_threads)
            {
                _threads.Remove(t);
            }
        }

        public int Enumerate(Thread[] array)
        {
            lock (_threads)
            {
                int count = Math.Min(array.Length, _threads.Count);
                _threads.CopyTo(0, array, 0, count);
                return count;
            }
        }
    }
}
