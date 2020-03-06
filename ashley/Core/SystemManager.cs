using System;
using System.Collections.Generic;
using System.Linq;
using ashley.Utils;

namespace ashley.Core
{
    internal class SystemManager
    {
        private readonly SystemComparator _systemComparator = new SystemComparator();
        private readonly List<EntitySystem> _systems = new List<EntitySystem>(16);
        private readonly Dictionary<Type, EntitySystem> _systemsByType = new Dictionary<Type, EntitySystem>();
        private readonly ISystemListener _listener;

        public ImmutableList<EntitySystem> Systems { get; }
        
        public SystemManager(ISystemListener listener)
        {
            _listener = listener;
            Systems = new ImmutableList<EntitySystem>(_systems);
        }

        public void AddSystem<T>(T system) where T : EntitySystem
        {
            var oldSystem = GetSystem<T>();

            if (oldSystem != null)
            {
                RemoveSystem(oldSystem);
            }

            _systems.Add(system);
            _systemsByType.Add(system.GetType(), system);
            _systems.Sort(_systemComparator);
            _listener.SystemAdded(system);
        }

        public void RemoveSystem<T>(T system) where T : EntitySystem
        {
            if (_systems.Remove(system))
            {
                _systemsByType.Remove(system.GetType());
                _listener.SystemRemoved(system);
            }
        }

        public void RemoveAllSystems()
        {
            while (_systems.Count > 0)
            {
                RemoveSystem(_systems.First());
            }
        }

        public T GetSystem<T>() where T : EntitySystem
        {
            if (_systemsByType.TryGetValue(typeof(T), out var value))
            {
                return (T)value;
            }

            return null;
        }

        private class SystemComparator : IComparer<EntitySystem>
        {
            public int Compare(EntitySystem x, EntitySystem y)
            {
                return x.Priority > y.Priority ? 1 : (x.Priority == y.Priority) ? 0 : -1;
            }
        }
    }
}