using System;
using ashley.Signals;
using ashley.Utils;

namespace ashley.Core
{
    public class Engine
    {
        private static readonly Family emptyFamily = Family.WithAllOf().WithNoneOf().Build();

        private readonly SystemManager _systemManager;
        private readonly EntityManager _entityManager;
        private readonly FamilyManager _familyManager;
        private readonly ComponentOperationHandler _componentOperationHandler;
        private bool _updating;

        private readonly IListener<Entity> _componentAdded;
        private readonly IListener<Entity> _componentRemoved;

        public Engine()
        {
            _systemManager = new SystemManager(new EngineSystemListener(this));
            _entityManager = new EntityManager(new EngineEntityListener(this));
            _componentOperationHandler = new ComponentOperationHandler(new GenericBooleanInformer(() => _updating));
            _familyManager = new FamilyManager(_entityManager.Entities);
            _componentAdded = new ComponentListener(_familyManager);
            _componentRemoved = new ComponentListener(_familyManager);
        }

        public Entity CreateEntity() => new Entity();

        public T CreateComponent<T>() => Activator.CreateInstance<T>();

        public void AddEntities(params Entity[] entities)
        {
            foreach (var entity in entities) AddEntity(entity);
        }
        
        public void AddEntity(Entity entity)
        {
            var delayed = _updating || _familyManager.Notifying;
            _entityManager.AddEntity(entity, delayed);
        }

        public void RemoveEntity(Entity entity)
        {
            var delayed = _updating || _familyManager.Notifying;
            _entityManager.RemoveEntity(entity, delayed);
        }

        public void RemoveAllEntities(Family family)
        {
            var delayed = _updating || _familyManager.Notifying;
            var entities = GetEntitiesFor(family);
            _entityManager.RemoveAllEntities(entities, delayed);
        }

        public void RemoveAllEntities()
        {
            var delayed = _updating || _familyManager.Notifying;
            _entityManager.RemoveAllEntities(delayed);
        }

        public ImmutableList<Entity> Entities => _entityManager.Entities;

        public void AddSystem<T>(T system) where T : EntitySystem => _systemManager.AddSystem(system);

        public void RemoveSystem<T>(T system) where T : EntitySystem => _systemManager.RemoveSystem(system);

        public void RemoveAllSystems() => _systemManager.RemoveAllSystems();

        public T GetSystem<T>() where T : EntitySystem => _systemManager.GetSystem<T>();

        public ImmutableList<EntitySystem> Systems => _systemManager.Systems;

        public ImmutableList<Entity> GetEntitiesFor(Family family) => _familyManager.GetEntitiesFor(family);

        public void AddEntityListener<TEntityListener>(TEntityListener listener, Family family = null, int priority = 0)
            where TEntityListener : IEntityListener
            => _familyManager.AddEntityListener(family ?? emptyFamily, priority, listener);

        public void RemoveEntityListener<TEntityListener>(TEntityListener listener)
            where TEntityListener : IEntityListener => _familyManager.RemoveEntityListener(listener);

        public void Update(float deltaTime)
        {
            if (_updating)
            {
                throw new InvalidOperationException("Cannot call Update on an engine that is already updating");
            }

            _updating = true;
            var systems = _systemManager.Systems;
            try
            {
                foreach (var system in systems)
                {
                    if (system.Processing)
                    {
                        system.Update(deltaTime);
                    }

                    while (_componentOperationHandler.HasOperationsToProcess || _entityManager.HasPendingOperations)
                    {
                        _componentOperationHandler.ProcessOperations();
                        _entityManager.ProcessPendingOperations();
                    }
                }
            }
            finally
            {
                _updating = false;
            }
        }

        protected void AddEntityInternal(Entity entity)
        {
            entity.ComponentAdded.Add(_componentAdded);
            entity.ComponentRemoved.Add(_componentRemoved);
            entity.ComponentOperationHandler = _componentOperationHandler;

            _familyManager.UpdateFamilyMembership(entity);
        }

        protected virtual void RemoveEntityInternal(Entity entity)
        {
            _familyManager.UpdateFamilyMembership(entity);

            entity.ComponentAdded.Remove(_componentAdded);
            entity.ComponentRemoved.Remove(_componentRemoved);
            entity.ComponentOperationHandler = null;
        }

        private class EngineEntityListener : IEntityListener
        {
            private readonly Engine _engine;

            public EngineEntityListener(Engine engine)
            {
                _engine = engine;
            }

            public void EntityAdded(Entity entity)
            {
                _engine.AddEntityInternal(entity);
            }

            public void EntityRemoved(Entity entity)
            {
                _engine.RemoveEntityInternal(entity);
            }
        }

        private class EngineSystemListener : ISystemListener
        {
            private readonly Engine _engine;

            public EngineSystemListener(Engine engine)
            {
                _engine = engine;
            }
            
            public void SystemAdded(EntitySystem system)
            {
                system.AddedToEngineInternal(_engine);
            }

            public void SystemRemoved(EntitySystem system)
            {
                system.RemovedFromEngineInternal(_engine);
            }
        }
    }

    internal class ComponentListener : IListener<Entity>
    {
        private readonly FamilyManager _familyManager;
        
        public ComponentListener(FamilyManager familyManager)
        {
            _familyManager = familyManager;
        }
        
        public void Receive(Signal<Entity> signal, Entity value)
        {
            _familyManager.UpdateFamilyMembership(value);
        }
    }
}