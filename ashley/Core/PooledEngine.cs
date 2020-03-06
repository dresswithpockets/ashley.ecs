using System;
using System.Collections.Generic;
using System.Reflection;

namespace ashley.Core
{
    public class PooledEngine : Engine
    {
        private Pool<Entity> _entityPool;
        private ComponentsPool _componentsPool;

        public PooledEngine(int entityPoolInitialSize = 10, int entityPoolMaxSize = 100, int componentPoolInitialSize = 10,
            int componentPoolMaxSize = 100) : base()
        {
            _entityPool = new Pool<Entity>(() => new Entity(), entityPoolMaxSize);
            _componentsPool = new ComponentsPool(componentPoolInitialSize, componentPoolMaxSize);
            if (entityPoolInitialSize > 0)
            {
                _entityPool.Fill(entityPoolInitialSize);
            }
        }

        public new Entity CreateEntity() => _entityPool.Obtain();

        public new T CreateComponent<T>() where T : class, IComponent, new() => _componentsPool.Obtain<T>();

        public void ClearPools()
        {
            _entityPool.Clear();
            _componentsPool.Clear();
        }

        protected override void RemoveEntityInternal(Entity entity)
        {
            base.RemoveEntityInternal(entity);

            if (entity is PooledEntity pooledEntity)
            {
                _entityPool.Free(pooledEntity);
            }
        }

        private class PooledEntity : Entity, IPoolable
        {
            internal ComponentsPool _componentsPool;
            
            internal override IComponent RemoveInternal(ComponentType componentType)
            {
                var removed = base.RemoveInternal(componentType);
                if (removed != null)
                {
                    _componentsPool.Free(removed);
                }
                return removed;
            }

            public void Reset()
            {
                RemoveAll();
                Flags = 0;
                ComponentAdded.RemoveAllListeners();
                ComponentRemoved.RemoveAllListeners();
                ScheduledForRemoval = false;
                Removing = false;
            }
        }
        
        private class ComponentsPool
        {
            private Dictionary<Type, object> _pools = new Dictionary<Type, object>();
            private int _initialSize;
            private int _maxSize;

            public ComponentsPool(int initialSize, int maxSize)
            {
                _initialSize = initialSize;
                _maxSize = maxSize;
            }

            public T Obtain<T>() where T : IComponent, new()
            {
                if (!_pools.TryGetValue(typeof(T), out var pool))
                {
                    pool = new Pool<T>(() => new T(), _maxSize);
                    if (_initialSize > 0)
                    {
                        ((Pool<T>) pool).Fill(_maxSize);
                    }
                    _pools.Add(typeof(T), pool);
                }

                return ((Pool<T>)pool).Obtain();
            }

            public void Free<T>(T item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                if (!_pools.TryGetValue(typeof(T), out var pool))
                {
                    return;
                }

                ((Pool<T>) pool).Free(item);
            }

            public void Free(object item)
            {
                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }

                if (!_pools.TryGetValue(item.GetType(), out var pool))
                {
                    return;
                }

                pool.GetType().GetMethod("Free", BindingFlags.Instance | BindingFlags.Public)
                    ?.Invoke(pool, new[] {item});
            }

            public void Clear()
            {
                foreach (var pool in _pools.Values)
                {
                    pool.GetType().GetMethod("Clear", BindingFlags.Instance | BindingFlags.Public)
                        ?.Invoke(pool, new object[0]);
                }
            }
        }
    }
}