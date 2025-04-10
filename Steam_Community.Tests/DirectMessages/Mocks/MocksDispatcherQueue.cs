using System;
using System.Collections.Generic;
using Microsoft.UI.Dispatching;

namespace Steam_Community.Tests.DirectMessages.Mocks
{
    /// <summary>
    /// A mock implementation of DispatcherQueue for testing purposes.
    /// This class doesn't inherit from DispatcherQueue because it's sealed,
    /// but it implements the same interface pattern used in the application.
    /// </summary>
    public interface IDispatcherQueue
    {
        bool TryEnqueue(DispatcherQueueHandler callback);
    }

    /// <summary>
    /// A wrapper for DispatcherQueue for testing purposes that implements IDispatcherQueue.
    /// </summary>
    public class DispatcherQueueWrapper : IDispatcherQueue
    {
        private readonly DispatcherQueue _dispatcherQueue;

        public DispatcherQueueWrapper(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue;
        }

        public bool TryEnqueue(DispatcherQueueHandler callback)
        {
            return _dispatcherQueue.TryEnqueue(callback);
        }
    }

    /// <summary>
    /// A mock dispatcher queue implementation that enqueues actions immediately for testing.
    /// </summary>
    public class MockDispatcherQueue : IDispatcherQueue
    {
        private readonly List<DispatcherQueueHandler> _queuedActions = new List<DispatcherQueueHandler>();

        /// <summary>
        /// Enqueues an action to be executed. In tests, executes immediately.
        /// </summary>
        /// <param name="callback">The action to execute</param>
        /// <returns>True if the action was enqueued successfully</returns>
        public bool TryEnqueue(DispatcherQueueHandler callback)
        {
            if (callback == null)
                return false;

            // Execute immediately for testing purposes
            callback.Invoke();
            _queuedActions.Add(callback);
            return true;
        }

        /// <summary>
        /// Returns the actual mock dispatcher queue instance to use in tests
        /// </summary>
        public static MockDispatcherQueue Instance { get; } = new MockDispatcherQueue();
    }
}
