﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable RedundantTypeArgumentsOfMethod

namespace Playground.WpfApp.Forms.TreeViewEx.TreeViewLib
{
    public class OneTaskProcessor : IDisposable
    {
        //Fields
        private readonly OneTaskLimitedScheduler _myTaskScheduler;
        private readonly List<TaskItem> _myTaskList;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;

        //Constructor
        public OneTaskProcessor()
        {
            _myTaskScheduler = new OneTaskLimitedScheduler();
            _myTaskList = new List<TaskItem>();
            _semaphore = new SemaphoreSlim(1, 1);
            _disposed = false;
        }

        //Methods
        internal async Task<int> ExecuteOneTask(Func<int> funcToExecute, CancellationTokenSource tokenSource)
        {
            try
            {
                for (int i = 0; i < _myTaskList.Count; i++)
                {
                    if (_myTaskList[i].Cancellation != null)
                    {
                        _myTaskList[i].Cancellation.Cancel();
                        _myTaskList[i].Cancellation.Dispose();
                    }
                }
            }
            catch (AggregateException e)
            {
                Console.WriteLine(@"\nAggregateException thrown with the following inner exceptions:");
                // Display information about each exception. 
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        Console.WriteLine(@"   TaskCanceledException: Task {0}",
                            ((TaskCanceledException)v).Task.Id);
                    else
                        Console.WriteLine(@"   Exception: {0}", v.GetType().Name);
                }
                Console.WriteLine();
            }
            finally
            {
                _myTaskList.Clear();
            }

            await _semaphore.WaitAsync();
            try
            {
                // Do the search and return number of results as int
                var t = Task.Factory.StartNew<int>(funcToExecute,
                    tokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    _myTaskScheduler);

                _myTaskList.Add(new TaskItem(t, tokenSource));

                await t;

                return t.Result;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                if (disposing)
                {
                    try
                    {
                        for (int i = 0; i < _myTaskList.Count; i++)
                        {
                            if (_myTaskList[i].Cancellation != null)
                            {
                                _myTaskList[i].Cancellation.Cancel();
                                _myTaskList[i].Cancellation.Dispose();
                            }
                        }

                        _semaphore.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        //Private Class
        private class TaskItem
        {
            /// <summary>
            /// Class constructor.
            /// </summary>
            /// <param name="taskToProcess"></param>
            /// <param name="cancellation"></param>
            public TaskItem(Task taskToProcess, CancellationTokenSource cancellation)
                : this()
            {
                TaskToProcess = taskToProcess;
                Cancellation = cancellation;
            }

            /// <summary>
            /// Class constructor.
            /// </summary>
            protected TaskItem()
            {
                TaskToProcess = null;
                Cancellation = null;
            }

            /// <summary>
            /// Gets the task that shoulf be processed.
            /// </summary>
            public Task TaskToProcess { get; }

            /// <summary>
            /// Gets the <seealso cref="CancellationTokenSource"/> that can
            /// be used to cancel this task.
            /// </summary>
            public CancellationTokenSource Cancellation { get; }
        }
    }
}
