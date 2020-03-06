using System.Collections.Generic;
using ashley.Utils;

namespace ashley.Core
{
    internal class FamilyManager
    {
        private ImmutableList<Entity> _entities;
        private Dictionary<Family, List<Entity>> _families = new Dictionary<Family, List<Entity>>();

        private Dictionary<Family, ImmutableList<Entity>> _immutableFamilies =
            new Dictionary<Family, ImmutableList<Entity>>();

        private List<EntityListenerData> _entityListeners = new List<EntityListenerData>();

        private Dictionary<Family, BitSet> _entityListenerMasks = new Dictionary<Family, BitSet>();
        
        private Pool<BitSet> _bitsPool = new Pool<BitSet>(() => new BitSet());
        public bool Notifying { get; private set; }

        public FamilyManager(ImmutableList<Entity> entities)
        {
            _entities = entities;
        }

        public ImmutableList<Entity> GetEntitiesFor(Family family) => RegisterFamily(family);

        public void AddEntityListener(Family family, int priority, IEntityListener listener)
        {
            RegisterFamily(family);

            var insertionIndex = 0;
            while (insertionIndex < _entityListeners.Count)
            {
                if (_entityListeners[insertionIndex].Priority <= priority)
                {
                    insertionIndex++;
                }
                else
                {
                    break;
                }
            }

            foreach (var mask in _entityListenerMasks.Values)
            {
                for (var i = mask.Length; i > insertionIndex; i--)
                {
                    if (mask.Get(i - 1))
                    {
                        mask.Set(i);
                    }
                    else
                    {
                        mask.Clear(i);
                    }
                }

                mask.Clear(insertionIndex);
            }

            _entityListenerMasks[family].Set(insertionIndex);

            _entityListeners.Add(new EntityListenerData
            {
                Listener = listener,
                Priority = priority
            });
        }

        public void RemoveEntityListener(IEntityListener listener)
        {
            for (var i = 0; i < _entityListeners.Count; i++)
            {
                var data = _entityListeners[i];
                if (data.Listener != listener) continue;
                
                foreach (var mask in _entityListenerMasks.Values)
                {
                    for (int k = i, n = mask.Length; k < n; k++)
                    {
                        if (mask.Get(k + 1))
                        {
                            mask.Set(k);
                        }
                        else
                        {
                            mask.Clear(k);
                        }
                    }
                }

                _entityListeners.RemoveAt(i--);
            }
        }

        public void UpdateFamilyMembership(Entity entity)
        {
            var addListenerBits = _bitsPool.Obtain();
            var removeListenerBits = _bitsPool.Obtain();

            foreach (var family in _entityListenerMasks.Keys)
            {
                var familyIndex = family.Index;
                var entityFamilyBits = entity.FamilyBits;

                var belongsToFamily = entityFamilyBits.Get(familyIndex);
                var matches = family.Matches(entity) && !entity._removing;

                if (belongsToFamily != matches)
                {
                    var listenersMask = _entityListenerMasks[family];
                    var familyEntities = _families[family];

                    if (matches)
                    {
                        addListenerBits.Or(listenersMask);
                        familyEntities.Add(entity);
                        entityFamilyBits.Set(familyIndex);
                    }
                    else
                    {
                        removeListenerBits.Or(listenersMask);
                        familyEntities.Remove(entity);
                        entityFamilyBits.Clear(familyIndex);
                    }
                }
            }

            Notifying = true;
            var items = new List<EntityListenerData>(_entityListeners);

            for (var i = removeListenerBits.NextSetBit(0); i >= 0; i = removeListenerBits.NextSetBit(i + 1))
            {
                items[i].Listener.EntityRemoved(entity);
            }

            for (var i = addListenerBits.NextSetBit(0); i >= 0; i = addListenerBits.NextSetBit(i + 1))
            {
                items[i].Listener.EntityAdded(entity);
            }

            addListenerBits.Clear();
            removeListenerBits.Clear();
            _bitsPool.Free(addListenerBits);
            _bitsPool.Free(removeListenerBits);
            Notifying = false;
        }

        private ImmutableList<Entity> RegisterFamily(Family family)
        {
            if (!_immutableFamilies.TryGetValue(family, out var entitiesInFamily))
            {
                var familyEntities = new List<Entity>(16);
                entitiesInFamily = new ImmutableList<Entity>(familyEntities);
                _families.Add(family, familyEntities);
                _immutableFamilies.Add(family, entitiesInFamily);
                _entityListenerMasks.Add(family, new BitSet());

                foreach (var entity in _entities)
                {
                    UpdateFamilyMembership(entity);
                }
            }

            return entitiesInFamily;
        }
    }
}