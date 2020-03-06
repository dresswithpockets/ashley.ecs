using System.Collections.Generic;

namespace ashley.Signals
{
    public class Signal<T>
    {
        private List<IListener<T>> _listeners = new List<IListener<T>>();

        public void Add(IListener<T> listener) => _listeners.Add(listener);

        public void Remove(IListener<T> listener) => _listeners.Remove(listener);

        public void RemoveAllListeners() => _listeners.Clear();

        public void Dispatch(T args)
        {
            var items = new IListener<T>[_listeners.Count];
            _listeners.CopyTo(items);
            foreach (var listener in items)
            {
                listener?.Receive(this, args);
            }
        }
    }
}