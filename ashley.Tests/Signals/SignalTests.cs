using System.Collections.Generic;
using ashley.Signals;
using Xunit;

namespace ashley.Tests.Signals
{
    public class SignalTests
    {
        private class Dummy
        {
        }

        private class ListenerMock : IListener<Dummy>
        {
            public int Count { get; private set; }

            public void Receive(Signal<Dummy> signal, Dummy value)
            {
                Count++;
                Assert.NotNull(signal);
                Assert.NotNull(value);
            }
        }

        private class RemoveWhileDispatchListenerMock : IListener<Dummy>
        {
            public int Count { get; private set; }
            
            public void Receive(Signal<Dummy> signal, Dummy value)
            {
                Count++;
                signal.Remove(this);
            }
        }

        [Fact]
        public void AddListenerAndDispatch()
        {
            var dummy = new Dummy();
            var signal = new Signal<Dummy>();
            var listener = new ListenerMock();
            signal.Add(listener);

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal(i, listener.Count);
                signal.Dispatch(dummy);
                Assert.Equal(i + 1, listener.Count);
            }
        }

        [Fact]
        public void AddListenersAndDispatch()
        {
            var dummy = new Dummy();
            var signal = new Signal<Dummy>();
            var listeners = new List<ListenerMock>();

            const int listenerCount = 10;

            while (listeners.Count < listenerCount)
            {
                var listener = new ListenerMock();
                listeners.Add(listener);
                signal.Add(listener);
            }

            const int dispatchCount = 10;

            for (var i = 0; i < dispatchCount; i++)
            {
                foreach (var listener in listeners)
                {
                    Assert.Equal(i, listener.Count);
                }

                signal.Dispatch(dummy);

                foreach (var listener in listeners)
                {
                    Assert.Equal(i + 1, listener.Count);
                }
            }
        }

        [Fact]
        public void AddListenerDispatchAndRemove()
        {
            var dummy = new Dummy();
            var signal = new Signal<Dummy>();
            var listenerA = new ListenerMock();
            var listenerB = new ListenerMock();

            signal.Add(listenerA);
            signal.Add(listenerB);

            const int dispatchCount = 5;
            
            for (var i = 0; i < dispatchCount; ++i)
            {
                Assert.Equal(i, listenerA.Count);
                Assert.Equal(i, listenerB.Count);

                signal.Dispatch(dummy);

                Assert.Equal(i + 1, listenerA.Count);
                Assert.Equal(i + 1, listenerB.Count);
            }

            signal.Remove(listenerB);

            for (var i = 0; i < dispatchCount; ++i)
            {
                Assert.Equal(i + dispatchCount, listenerA.Count);
                Assert.Equal(dispatchCount, listenerB.Count);

                signal.Dispatch(dummy);

                Assert.Equal(i + 1 + dispatchCount, listenerA.Count);
                Assert.Equal(dispatchCount, listenerB.Count);
            }
        }

        [Fact]
        public void RemoveWhileDispatch()
        {
            var dummy = new Dummy();
            var signal = new Signal<Dummy>();
            var listenerA = new RemoveWhileDispatchListenerMock();
            var listenerB = new ListenerMock();

            signal.Add(listenerA);
            signal.Add(listenerB);

            signal.Dispatch(dummy);

            Assert.Equal(1, listenerA.Count);
            Assert.Equal(1, listenerB.Count);
        }

        [Fact]
        public void RemoveAll()
        {
            var dummy = new Dummy();
            var signal = new Signal<Dummy>();

            var listenerA = new ListenerMock();
            var listenerB = new ListenerMock();

            signal.Add(listenerA);
            signal.Add(listenerB);

            signal.Dispatch(dummy);

            Assert.Equal(1, listenerA.Count);
            Assert.Equal(1, listenerB.Count);

            signal.RemoveAllListeners();

            signal.Dispatch(dummy);

            Assert.Equal(1, listenerA.Count);
            Assert.Equal(1, listenerB.Count);
        }
    }
}