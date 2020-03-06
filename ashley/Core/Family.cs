using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ashley.Utils;

namespace ashley.Core
{
    public class Family
    {
        private static Dictionary<string, Family> _families = new Dictionary<string,Family>();
        private static int _familyIndex = 0;
        private static readonly Builder _builder = new Builder();

        private readonly BitSet _all;
        private readonly BitSet _any;
        private readonly BitSet _exclude;
        public int Index { get; }

        private Family(BitSet all, BitSet any, BitSet exclude)
        {
            _all = all;
            _any = any;
            _exclude = exclude;
            Index = _familyIndex++;
        }

        public bool Matches(Entity entity)
        {
            if (!entity.ComponentBits.ContainsAll(_all))
            {
                return false;
            }

            if (_any.NotEmpty && !_any.Intersects(entity.ComponentBits))
            {
                return false;
            }

            if (_exclude.NotEmpty && _exclude.Intersects(entity.ComponentBits))
            {
                return false;
            }

            return true;
        }

        public static Builder WithAllOf(params Type[] componentTypes) => _builder.Reset().WithAllOf(componentTypes);
        
        public static Builder WithAllOf<T>() where T : class, IComponent => WithAllOf(typeof(T));

        public static Builder WithAllOf<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            => WithAllOf(typeof(T1), typeof(T2));
            
        public static Builder WithAllOf<T1, T2, T3>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            => WithAllOf(typeof(T1), typeof(T2), typeof(T3));
            
        public static Builder WithAllOf<T1, T2, T3, T4>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
            => WithAllOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static Builder WithAnyOf(params Type[] componentTypes) => _builder.Reset().WithAnyOf(componentTypes);
        
        public static Builder WithAnyOf<T>() where T : class, IComponent => WithAnyOf(typeof(T));

        public static Builder WithAnyOf<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            => WithAnyOf(typeof(T1), typeof(T2));
            
        public static Builder WithAnyOf<T1, T2, T3>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            => WithAnyOf(typeof(T1), typeof(T2), typeof(T3));
            
        public static Builder WithAnyOf<T1, T2, T3, T4>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
            => WithAnyOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public static Builder WithNoneOf(params Type[] componentTypes) => _builder.Reset().WithNoneOf(componentTypes);
        
        public static Builder WithNoneOf<T>() where T : class, IComponent => WithNoneOf(typeof(T));

        public static Builder WithNoneOf<T1, T2>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            => WithNoneOf(typeof(T1), typeof(T2));
            
        public static Builder WithNoneOf<T1, T2, T3>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            => WithNoneOf(typeof(T1), typeof(T2), typeof(T3));
            
        public static Builder WithNoneOf<T1, T2, T3, T4>()
            where T1 : class, IComponent
            where T2 : class, IComponent
            where T3 : class, IComponent
            where T4 : class, IComponent
            => WithNoneOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

        public override int GetHashCode() => Index;

        public override bool Equals(object obj) => this == obj;

        private static string GetFamilyHash(BitSet all, BitSet one, BitSet exclude)
        {
            var sb = new StringBuilder();
            if (all.NotEmpty)
            {
                sb.Append($"{{all:{GetBitSetString(all)}}}");
            }

            if (one.NotEmpty)
            {
                sb.Append($"{{all:{GetBitSetString(one)}}}");
            }

            if (exclude.NotEmpty)
            {
                sb.Append($"{{all:{GetBitSetString(exclude)}}}");
            }

            return sb.ToString();
        }

        public static string GetBitSetString(BitSet bitSet)
        {
            var sb = new StringBuilder();

            var bitCount = bitSet.Length;
            for (var i = 0; i < bitCount; i++)
            {
                sb.Append(bitSet.Get(i) ? '1' : '0');
            }

            return sb.ToString();
        }
        
        public class Builder
        {
            private BitSet _all = new BitSet();
            private BitSet _any = new BitSet();
            private BitSet _exclude = new BitSet();

            internal Builder()
            {
            }

            public Builder Reset()
            {
                _all = new BitSet();
                _any = new BitSet();
                _exclude = new BitSet();
                return this;
            }

            public Builder WithAllOf(params Type[] componentTypes)
            {
                if (componentTypes.Any(t => !typeof(IComponent).IsAssignableFrom(t)))
                {
                    throw new ArgumentException("Each component type must be of a type that implements IComponent",
                        nameof(componentTypes));
                }

                _all = ComponentType.GetBitsFor(componentTypes);
                return this;
            }

            public Builder WithAllOf<T>() where T : class, IComponent => WithAllOf(typeof(T));

            public Builder WithAllOf<T1, T2>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                => WithAllOf(typeof(T1), typeof(T2));
            
            public Builder WithAllOf<T1, T2, T3>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                => WithAllOf(typeof(T1), typeof(T2), typeof(T3));
            
            public Builder WithAllOf<T1, T2, T3, T4>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                where T4 : class, IComponent
                => WithAllOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            public Builder WithAnyOf(params Type[] componentTypes)
            {
                if (componentTypes.Any(t => !typeof(IComponent).IsAssignableFrom(t)))
                {
                    throw new ArgumentException("Each component type must be of a type that implements IComponent",
                        nameof(componentTypes));
                }

                _any = ComponentType.GetBitsFor(componentTypes);
                return this;
            }

            public Builder WithAnyOf<T>() where T : class, IComponent => WithAnyOf(typeof(T));

            public Builder WithAnyOf<T1, T2>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                => WithAnyOf(typeof(T1), typeof(T2));
            
            public Builder WithAnyOf<T1, T2, T3>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                => WithAnyOf(typeof(T1), typeof(T2), typeof(T3));
            
            public Builder WithAnyOf<T1, T2, T3, T4>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                where T4 : class, IComponent
                => WithAnyOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            public Builder WithNoneOf(params Type[] componentTypes)
            {
                if (componentTypes.Any(t => !typeof(IComponent).IsAssignableFrom(t)))
                {
                    throw new ArgumentException("Each component type must be of a type that implements IComponent",
                        nameof(componentTypes));
                }

                _exclude = ComponentType.GetBitsFor(componentTypes);
                return this;
            }

            public Builder WithNoneOf<T>() where T : class, IComponent => WithNoneOf(typeof(T));

            public Builder WithNoneOf<T1, T2>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                => WithNoneOf(typeof(T1), typeof(T2));
            
            public Builder WithNoneOf<T1, T2, T3>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                => WithNoneOf(typeof(T1), typeof(T2), typeof(T3));
            
            public Builder WithNoneOf<T1, T2, T3, T4>()
                where T1 : class, IComponent
                where T2 : class, IComponent
                where T3 : class, IComponent
                where T4 : class, IComponent
                => WithNoneOf(typeof(T1), typeof(T2), typeof(T3), typeof(T4));

            public Family Build()
            {
                var hash = GetFamilyHash(_all, _any, _exclude);
                
                if (!_families.TryGetValue(hash, out var family))
                {
                    family = new Family(_all, _any, _exclude);
                    _families.Add(hash, family);
                }

                return family;
            }
        }
    }
}