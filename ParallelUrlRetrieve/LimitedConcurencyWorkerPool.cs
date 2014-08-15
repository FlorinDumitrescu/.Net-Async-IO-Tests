using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace ParallelUrlRetrieve
{
    public class LimitedConcurencyWorkerPool
    {
        #region types
        public interface ILogProvider
        {
            void LogException(Exception ex);
        }
        #endregion types

        #region private fields
        private int _maxConcurencyLevel;
        private ILogProvider _logger;
        private readonly object _syncLock = new object();
        private Queue<Action> _workItemsQueue = new Queue<Action>();
        private int _runningThreads = 0;
        #endregion private fields

        #region constructor
        public LimitedConcurencyWorkerPool(int maximumConcurencyLevel, ILogProvider logger)
        {
            _maxConcurencyLevel = maximumConcurencyLevel;
            _logger = logger;
        }
        #endregion constructor

        #region private methods
        private void LogException(Exception ex)
        {
            if (_logger != null)
                _logger.LogException(ex);
        }

        private void StartNewExecutor()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    while (true)
                    {
                        Action crtWorkItem = null;
                        lock (_syncLock)
                        {
                            if (_workItemsQueue.Count == 0)
                                break; // Exit the loop and terminate executor when there aren't any more items to execute
                            crtWorkItem = _workItemsQueue.Dequeue();
                        }

                        // Execute work item
                        if (crtWorkItem != null)
                        {
                            try
                            {
                                crtWorkItem();
                            }
                            // Work items exceptions shoud not terminate the executor
                            catch (Exception ex)
                            {
                                this.LogException(ex);

                                if (ex is ThreadAbortException)
                                    Thread.ResetAbort();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.LogException(ex);
                }
                finally
                {
                    lock (_syncLock)
                    {
                        --_runningThreads;
                    }
                }
            }));
            thread.Start();
        }
        #endregion private methods

        #region public methods
        public void QueueWorkItem(Action workItem)
        {
            if (workItem == null)
                return;

            lock (_syncLock)
            {
                _workItemsQueue.Enqueue(workItem);

                if (_runningThreads < _maxConcurencyLevel)
                {
                    ++_runningThreads;
                    this.StartNewExecutor();
                }
            }
        }

        public void EmptyQueue()
        {
            lock (_syncLock)
            {
                _workItemsQueue.Clear();
            }
        }

        public bool IsIdle
        {
            get
            {
                lock (_syncLock)
                {
                    return (_runningThreads == 0 && _workItemsQueue.Count == 0);
                }
            }
        }
        #endregion public methods
    }
}
