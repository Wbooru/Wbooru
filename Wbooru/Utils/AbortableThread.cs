using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wbooru.Utils
{
    public class AbortableThread
    {
        private Thread thread;
        private CancellationTokenSource cancellationTokenSource;

        public AbortableThread(Action<CancellationToken> cancellableMethod)
        {
            cancellationTokenSource = new CancellationTokenSource();
            thread = new Thread(() => cancellableMethod?.Invoke(cancellationTokenSource.Token));
        }

        public bool IsBackground
        {
            get
            {
                return thread.IsBackground;
            }

            set
            {
                thread.IsBackground = value;
            }
        }

        public string Name
        {
            get
            {
                return thread.Name;
            }
            set
            {
                thread.Name = value;
            }
        }

        public void Start()
        {
            thread.Start();
            Log.Info($"Thread {Name} started.", "AbortableThread");
        }


        public void Abort()
        {
            Log.Info($"Begin to abort thread {Name}.", "AbortableThread");
            cancellationTokenSource.Cancel();
            thread?.Join();
            Log.Info($"Aborted thread {Name}.", "AbortableThread");
        }
    }
}
