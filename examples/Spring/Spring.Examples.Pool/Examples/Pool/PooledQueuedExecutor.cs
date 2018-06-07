////<examples>
using System;
using System.Collections;
using System.Threading;
using Spring.Pool;
using Spring.Pool.Support;
using Spring.Threading;

namespace Spring.Examples.Pool
{
    /// <summary>
    /// Factory of <see cref="QueuedExecutor"/> istances.
    /// </summary>
    //// <example name='factory-declaration'>
    public class QueuedExecutorPoolableFactory : IPoolableObjectFactory
    {
        //// </example>
        //// <example name='destroy'>
        void IPoolableObjectFactory.DestroyObject(object o)
        {
            // ah, self documenting code:
            // Here you can see that we decided to let the 
            // executor process all the currently queued tasks.
            QueuedExecutor executor = o as Spring.Threading.QueuedExecutor;
            executor.ShutdownAfterProcessingCurrentlyQueuedTasks();
        }
        //// </example>
        
        //// <example name="validate">
        bool IPoolableObjectFactory.ValidateObject(object o)
        {
            QueuedExecutor executor = o as QueuedExecutor;
            return executor.Thread != null;
        }
        //// </example>

        //// <example name='activate'>
        void IPoolableObjectFactory.ActivateObject(object o)
        {
            QueuedExecutor executor = o as QueuedExecutor;
            executor.Restart();
        }
        //// </example>

        //// <example name='passivate'>
        void IPoolableObjectFactory.PassivateObject(object o)
        {
        }
        //// </example>

        //// <example name='make'>
        object IPoolableObjectFactory.MakeObject()
        {            
            // to actually make this work as a pooled executor
            // use a bounded queue of capacity 1.
            // If we don't do this one of the queued executors
            // will accept all the queued IRunnables as, by default
            // its queue is unbounded, and the PooledExecutor
            // will happen to always run only one thread ...
            return new QueuedExecutor(new BoundedBuffer(1));
        }
        //// </example>
    }

    //// <example name="holder">
    public class PooledObjectHolder : IDisposable
    {
        IObjectPool pool;
        object pooled;

        /// <summary>
        /// Builds a new <see cref="PooledObjectHolder"/>
        /// trying to borrow an object form it
        /// </summary>
        /// <param name="pool"></param>
        private PooledObjectHolder(IObjectPool pool)
        {
            this.pool = pool;
            this.pooled = pool.BorrowObject();
        }

        /// <summary>
        /// Allow to access the borrowed pooled object
        /// </summary>
        public object Pooled
        {
            get
            {
                return pooled;
            }
        }

        /// <summary>
        /// Returns the borrowed object to the pool
        /// </summary>
        public void Dispose()
        {
            pool.ReturnObject(pooled);
        }

        /// <summary>
        /// Creates a new <see cref="PooledObjectHolder"/> for the 
        /// given pool.
        /// </summary>
        public static PooledObjectHolder UseFrom(IObjectPool pool)
        {
            return new PooledObjectHolder(pool);
        }
    }
    //// </example>
    
    public class PooledQueuedExecutor : IExecutor
	{
	    SimplePool pool;
        private IList syncs;

        class Queuer
        {
            IObjectPool pool;
            IRunnable runnable;
            private ISync sync;

            public Queuer (IObjectPool pool, IRunnable runnable)
            {
                this.pool = pool;
                this.runnable = runnable;
                this.sync = new Latch();
            }

            public void Queue ()
            {
                //// <example name="execute">
                using (PooledObjectHolder holder = PooledObjectHolder.UseFrom(pool))
                {
                    QueuedExecutor executor = (QueuedExecutor) holder.Pooled;
                    executor.Execute(runnable);
                }
                //// </example>
                sync.Release();
            }

            public static ISync Queue (IObjectPool pool, IRunnable runnable)
            {
                Queuer queuer = new Queuer(pool, runnable);
                Thread thread = new Thread(new ThreadStart(queuer.Queue));
                thread.Start();
                return queuer.Sync;
            }

            public ISync Sync
            {
                get
                {
                    return sync;
                }
            }
        }

	    public PooledQueuedExecutor(int size)
	    {
            //// <example name="create-pool">
            pool = new SimplePool(new QueuedExecutorPoolableFactory(), size);
            //// </example>	        
            syncs = ArrayList.Synchronized(new ArrayList());
	    }

        public PooledQueuedExecutor()
            : this(10)
	    {
	    }

        public void Execute(IRunnable runnable)
        {
            // queue the task and remember its ISync ...
            syncs.Add(Queuer.Queue(pool, runnable));
        }

	    public void Execute(Action action)
        {
            throw new NotImplementedException();
        }

        //// <example name="stop">
        public void Stop ()
        {
            // waits for all the grep-task to have been queued ...
            foreach (ISync sync in syncs)
            {
                sync.Acquire();
            }
            pool.Close();
        }
        //// </example>
	}
}
//// </examples>