using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ashley.Core
{
    public class Pool<T>
    {
        public int Max { get; }
        public int Peak { get; set; }
        public int FreeCount => _freeObjects.Count;

        private readonly ConcurrentBag<T> _freeObjects;
        private readonly Func<T> _objectGenerator;

        public Pool(Func<T> objectGenerator, int max = int.MaxValue)
        {   
            _freeObjects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
            Max = max;
        }

        public T Obtain() => _freeObjects.TryTake(out var result) ? result : _objectGenerator.Invoke();

        public void Free(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), "the object passed must not be null");
            if (_freeObjects.Count < Max)
            {
                _freeObjects.Add(obj);
                Peak = Math.Max(Peak, _freeObjects.Count);
            }

            Reset(obj);
        }

        public void Free(IEnumerable<T> objects)
        {
            if (objects == null)
                throw new ArgumentNullException(nameof(objects), "Enumerable passed mustn't be null.");
            
            foreach (var obj in objects)
            {
                if (obj == null) continue;
                if (_freeObjects.Count < Max)
                {
                    _freeObjects.Add(obj);
                }

                Reset(obj);
            }
            
            Peak = Math.Max(Peak, _freeObjects.Count);
        }

        protected void Reset(T obj) => (obj as IPoolable)?.Reset();

        public void Fill(int size)
        {
            for (var i = 0; i < size; i++)
            {
                if (_freeObjects.Count < Max) _freeObjects.Add(_objectGenerator.Invoke());
            }
            
            Peak = Math.Max(Peak, _freeObjects.Count);
        }

        public void Clear() => _freeObjects.Clear();
    }
}