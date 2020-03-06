using System;
using System.Collections.Generic;
using ashley.Utils;

namespace ashley.Core
{
    public class ComponentType
    {
        private static Dictionary<Type, ComponentType> _assignedComponentTypes =
            new Dictionary<Type, ComponentType>();

        private static int _typeIndex;

        private readonly int _index;

        public int Index => _index;

        public ComponentType()
        {
            _index = _typeIndex++;
        }

        public static ComponentType GetFor(Type componentType)
        {
            if (!typeof(IComponent).IsAssignableFrom(componentType) || !componentType.IsClass)
                throw new ArgumentException("The type must be a class that implements IComponent",
                    nameof(componentType));

            if (!_assignedComponentTypes.TryGetValue(componentType, out var type))
            {
                type = new ComponentType();
                _assignedComponentTypes.Add(componentType, type);
            }

            return type;
        }

        public static ComponentType GetFor<T>() where T : class, IComponent => GetFor(typeof(T));

        public static int GetIndexFor(Type componentType) => GetFor(componentType).Index;

        public static int GetIndexFor<T>() where T : class, IComponent => GetIndexFor(typeof(T));

        public static BitSet GetBitsFor(params Type[] componentTypes)
        {
            var bitArray = new BitSet(componentTypes.Length);

            for (var i = 0; i < componentTypes.Length; i++)
            {
                bitArray.Set(GetIndexFor(componentTypes[i]));
            }
            
            return bitArray;
        }

        public override int GetHashCode()
        {
            return _index;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;
            return _index == ((ComponentType) obj)._index;
        }
    }
}