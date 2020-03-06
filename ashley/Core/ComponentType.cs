using System;
using System.Collections.Generic;
using ashley.Utils;

namespace ashley.Core
{
    /// <summary>
    /// Uniquely identifiers a <see cref="IComponent"/> implementing class. It assigns them an index which is used
    /// internally for fast comparison and retrieval. See <see cref="Family"/> and <see cref="Entity"/>. Access
    /// ComponentTypes for a specified type using <see cref="GetFor{T}"/> or <see cref="GetFor"/>. Each unique type
    /// will always return the same instance of ComponentType.
    /// </summary>
    public class ComponentType
    {
        private static Dictionary<Type, ComponentType> _assignedComponentTypes =
            new Dictionary<Type, ComponentType>();

        private static int _typeIndex;

        private readonly int _index;

        public int Index => _index;

        private ComponentType()
        {
            _index = _typeIndex++;
        }

        /// <summary>
        /// returns a ComponentType matching the specified runtime type
        /// </summary>
        /// <param name="componentType">The runtime type to get a ComponentType for</param>
        /// <returns>a ComponentType matching the specified runtime type</returns>
        /// <exception cref="ArgumentException">
        /// thrown when <paramref name="componentType"/> does not represent an instantiable class or is not an
        /// IComponent type.
        /// </exception>
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

        /// <summary>
        /// returns a ComponentType matching the specified type
        /// </summary>
        /// <typeparam name="T">
        /// The class which implements <see cref="IComponent"/> get the representative ComponentType for.
        /// </typeparam>
        /// <returns>a ComponentType matching the specified type</returns>
        public static ComponentType GetFor<T>() where T : class, IComponent => GetFor(typeof(T));

        /// <summary>
        /// Helper method. Short form of <code>ComponentType.GetFor(typeof(ComponentClass)).Index</code>
        /// </summary>
        /// <param name="componentType">The runtime type to get a matching ComponentType's index for.</param>
        /// <returns>The index for the matching ComponentType</returns>
        public static int GetIndexFor(Type componentType) => GetFor(componentType).Index;

        /// <summary>
        /// Helper method. Short form of <code>ComponentType.GetFor&lt;T&gt;().Index</code>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The index for the matching ComponentType</returns>
        public static int GetIndexFor<T>() where T : class, IComponent => GetIndexFor(typeof(T));

        /// <summary>
        /// returns a <see cref="BitSet"/> representing the collection of components for quick comparison and matching.
        /// See <see cref="Family"/>
        /// </summary>
        /// <param name="componentTypes">component types to get a representative bitset for</param>
        /// <returns>
        /// a <see cref="BitSet"/> representing the collection of components for quick comparison and matching.
        /// See <see cref="Family"/>
        /// </returns>
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