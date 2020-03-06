using System;

namespace ashley.Core
{
    internal class GenericBooleanInformer : IBooleanInformer
    {
        private readonly Func<bool> _provider;

        public GenericBooleanInformer(Func<bool> provider)
        {
            _provider = provider;
        }

        public bool Value => _provider?.Invoke() ?? false;
    }
}