using System;
using System.Collections.Generic;
using System.Linq;
using ashley.Core;
using ashley.Systems;
using ashley.Utils;
using Xunit;

namespace ashley.Tests.Core
{
    public class EngineTests
    {
        private float _deltaTime = 0.16f;

        private class ComponentA : IComponent
        {
        }

        private class ComponentB : IComponent
        {
        }

        private class ComponentC : IComponent
        {
        }

        private class ComponentD : IComponent
        {
        }

        private class EntityListenerMock : IEntityListener
        {
            public int AddedCount { get; private set; }
            public int RemovedCount { get; private set; }

            public virtual void EntityAdded(Entity entity)
            {
                AddedCount++;
                Assert.NotNull(entity);
            }

            public virtual void EntityRemoved(Entity entity)
            {
                RemovedCount++;
                Assert.NotNull(entity);
            }
        }

        private class AddComponentBEntityListenerMock : EntityListenerMock
        {
            public override void EntityAdded(Entity entity)
            {
                base.EntityAdded(entity);
                entity.Add(new ComponentB());
            }
        }

        private class EntitySystemMock : EntitySystem
        {
            public int UpdateCalls { get; private set; }
            public int AddedCalls { get; private set; }
            public int RemovedCalls { get; private set; }

            private List<int> Updates { get; }

            public EntitySystemMock() : base()
            {
            }

            public EntitySystemMock(List<int> updates)
            {
                Updates = updates;
            }

            public override void AddedToEngine(Engine engine)
            {
                AddedCalls++;

                Assert.NotNull(engine);
            }

            public override void RemovedFromEngine(Engine engine)
            {
                RemovedCalls++;

                Assert.NotNull(engine);
            }

            public override void Update(float deltaTime)
            {
                UpdateCalls++;

                Updates?.Add(Priority);
            }
        }

        private class EntitySystemMockA : EntitySystemMock
        {
            public EntitySystemMockA() : base()
            {
            }

            public EntitySystemMockA(List<int> updates) : base(updates)
            {
            }
        }
        
        private class EntitySystemMockB : EntitySystemMock
        {
            public EntitySystemMockB() : base()
            {
            }
            
            public EntitySystemMockB(List<int> updates) : base(updates)
            {
            }
        }

        private class CounterComponent : IComponent
        {
            public int Counter = 0;
        }

        private class CounterSystem : EntitySystem
        {
            private ImmutableList<Entity> _entities;
            
            public override void AddedToEngine(Engine engine)
            {
                _entities = engine.GetEntitiesFor(Family.WithAllOf(typeof(CounterComponent)).Build());
            }

            public override void Update(float deltaTime)
            {
                for (var i = 0; i < _entities.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        var counterComponent = _entities[i].GetComponent<CounterComponent>();
                        counterComponent.Counter++;
                    }
                    else
                    {
                        Engine.RemoveEntity(_entities[i]);
                    }
                }
            }
        }

        [Fact]
        public void AddAndRemoveEntity()
        {
            var engine = new Engine();

            var listenerA = new EntityListenerMock();
            var listenerB = new EntityListenerMock();

            engine.AddEntityListener(listenerA);
            engine.AddEntityListener(listenerB);

            var entity1 = new Entity();
            engine.AddEntity(entity1);

            Assert.Equal(1, listenerA.AddedCount);
            Assert.Equal(1, listenerB.AddedCount);

            engine.RemoveEntityListener(listenerB);

            var entity2 = new Entity();
            engine.AddEntity(entity2);

            Assert.Equal(2, listenerA.AddedCount);
            Assert.Equal(1, listenerB.AddedCount);

            engine.AddEntityListener(listenerB);

            engine.RemoveAllEntities();

            Assert.Equal(2, listenerA.RemovedCount);
            Assert.Equal(2, listenerB.RemovedCount);
        }

        [Fact]
        public void AddComponentInsideListener()
        {
            var engine = new Engine();

            var listenerA = new AddComponentBEntityListenerMock();
            var listenerB = new EntityListenerMock();

            engine.AddEntityListener(listenerA, Family.WithAllOf(typeof(ComponentA)).Build());
            engine.AddEntityListener(listenerB, Family.WithAllOf(typeof(ComponentB)).Build());

            var entity1 = new Entity();
            entity1.Add(new ComponentA());
            engine.AddEntity(entity1);

            Assert.Equal(1, listenerA.AddedCount);
            Assert.NotNull(entity1.GetComponent<ComponentB>());
            Assert.Equal(1, listenerB.AddedCount);
        }

        [Fact]
        public void AddAndRemoveSystem()
        {
            var engine = new Engine();
            var systemA = new EntitySystemMockA();
            var systemB = new EntitySystemMockB();
            
            Assert.Null(engine.GetSystem<EntitySystemMockA>());
            Assert.Null(engine.GetSystem<EntitySystemMockB>());

            engine.AddSystem(systemA);
            engine.AddSystem(systemB);
            
            Assert.NotNull(engine.GetSystem<EntitySystemMockA>());
            Assert.NotNull(engine.GetSystem<EntitySystemMockB>());
            Assert.Equal(1, systemA.AddedCalls);
            Assert.Equal(1, systemB.AddedCalls);

            engine.RemoveSystem(systemA);
            engine.RemoveSystem(systemB);
            
            Assert.Null(engine.GetSystem<EntitySystemMockA>());
            Assert.Null(engine.GetSystem<EntitySystemMockB>());
            Assert.Equal(1, systemA.RemovedCalls);
            Assert.Equal(1, systemB.RemovedCalls);

            engine.AddSystem(systemA);
            engine.AddSystem(systemB);
            engine.RemoveAllSystems();
            
            Assert.Null(engine.GetSystem<EntitySystemMockA>());
            Assert.Null(engine.GetSystem<EntitySystemMockB>());
            Assert.Equal(2, systemA.AddedCalls);
            Assert.Equal(2, systemB.AddedCalls);
            Assert.Equal(2, systemA.RemovedCalls);
            Assert.Equal(2, systemB.RemovedCalls);
        }

        [Fact]
        public void GetSystems()
        {
            var engine = new Engine();
            var systemA = new EntitySystemMockA();
            var systemB = new EntitySystemMockB();

            Assert.Empty(engine.Systems);

            engine.AddSystem(systemA);
            engine.AddSystem(systemB);

            Assert.Equal(2, engine.Systems.Count);
        }

        [Fact]
        public void AddTwoSystemsOfSameClass()
        {
            var engine = new Engine();
            var system1 = new EntitySystemMockA();
            var system2 = new EntitySystemMockA();
            
            Assert.Empty(engine.Systems);

            engine.AddSystem(system1);
            
            Assert.Single(engine.Systems);
            Assert.Equal(system1, engine.GetSystem<EntitySystemMockA>());

            engine.AddSystem(system2);

            Assert.Single(engine.Systems);
            Assert.Equal(system2, engine.GetSystem<EntitySystemMockA>());
        }

        [Fact]
        public void SystemUpdate()
        {
            var engine = new Engine();
            var systemA = new EntitySystemMockA();
            var systemB = new EntitySystemMockB();

            engine.AddSystem(systemA);
            engine.AddSystem(systemB);

            var updates = 10;

            for (var i = 0; i < updates; i++)
            {
                Assert.Equal(i, systemA.UpdateCalls);
                Assert.Equal(i, systemB.UpdateCalls);

                engine.Update(_deltaTime);

                Assert.Equal(i + 1, systemA.UpdateCalls);
                Assert.Equal(i + 1, systemB.UpdateCalls);
            }

            engine.RemoveSystem(systemB);

            for (var i = 0; i < updates; i++)
            {
                Assert.Equal(i + updates, systemA.UpdateCalls);
                Assert.Equal(updates, systemB.UpdateCalls);

                engine.Update(_deltaTime);
                
                Assert.Equal(i + updates + 1, systemA.UpdateCalls);
                Assert.Equal(updates, systemB.UpdateCalls);
            }
        }

        [Fact]
        public void SystemUpdateOrder()
        {
            var updates = new List<int>();

            var engine = new Engine();
            var system1 = new EntitySystemMockA(updates);
            var system2 = new EntitySystemMockB(updates);

            system1.Priority = 2;
            system2.Priority = 1;

            engine.AddSystem(system1);
            engine.AddSystem(system2);

            engine.Update(_deltaTime);

            var previous = int.MinValue;

            foreach (var value in updates)
            {
                Assert.True(value >= previous);
                previous = value;
            }
        }

        [Fact]
        public void EntitySystemEngineReference()
        {
            var engine = new Engine();
            var system = new EntitySystemMock();

            Assert.Null(system.Engine);
            engine.AddSystem(system);
            Assert.Equal(engine, system.Engine);
            engine.RemoveSystem(system);
            Assert.Null(system.Engine);
        }

        [Fact]
        public void IgnoreSystem()
        {
            var engine = new Engine();
            var system = new EntitySystemMock();

            engine.AddSystem(system);

            var updates = 10;

            for (var i = 0; i < updates; i++)
            {
                system.Processing = i % 2 == 0;
                engine.Update(_deltaTime);
                Assert.Equal(i / 2 + 1, system.UpdateCalls);
            }
        }

        [Fact]
        public void EntitiesForFamily()
        {
            var engine = new Engine();
            var family = Family.WithAllOf(typeof(ComponentA), typeof(ComponentB)).Build();
            var familyEntities = engine.GetEntitiesFor(family);

            Assert.Empty(familyEntities);

            var entity1 = new Entity();
            var entity2 = new Entity();
            var entity3 = new Entity();
            var entity4 = new Entity();

            entity1.Add(new ComponentA());
            entity1.Add(new ComponentB());

            entity2.Add(new ComponentA());
            entity2.Add(new ComponentC());
            
            entity3.Add(new ComponentA());
            entity3.Add(new ComponentB());
            entity3.Add(new ComponentC());
            
            entity4.Add(new ComponentA());
            entity4.Add(new ComponentB());
            entity4.Add(new ComponentC());
            
            engine.AddEntities(entity1, entity2, entity3, entity4);

            Assert.Equal(3, familyEntities.Count);
            Assert.Contains(entity1, familyEntities);
            Assert.Contains(entity3, familyEntities);
            Assert.Contains(entity4, familyEntities);
            Assert.DoesNotContain(entity2, familyEntities);
        }

        [Fact]
        public void EntityForFamilyWithRemoval()
        {
            var engine = new Engine();

            var entity = new Entity();
            entity.Add(new ComponentA());

            engine.AddEntity(entity);

            var entities = engine.GetEntitiesFor(Family.WithAllOf<ComponentA>().Build());

            Assert.Single(entities);
            Assert.Contains(entity, entities);

            engine.RemoveEntity(entity);

            Assert.Empty(entities);
            Assert.DoesNotContain(entity, entities);
        }

        [Fact]
        public void EntitiesForFamilyAfter()
        {
            var engine = new Engine();

            var family = Family.WithAllOf<ComponentA, ComponentB>().Build();
            var familyEntities = engine.GetEntitiesFor(family);

            Assert.Empty(familyEntities);

            var entity1 = new Entity();
            var entity2 = new Entity();
            var entity3 = new Entity();
            var entity4 = new Entity();

            engine.AddEntities(entity1, entity2, entity3, entity4);

            entity1.Add(new ComponentA());
            entity1.Add(new ComponentB());

            entity2.Add(new ComponentA());
            entity2.Add(new ComponentC());

            entity3.Add(new ComponentA());
            entity3.Add(new ComponentB());
            entity3.Add(new ComponentC());

            entity4.Add(new ComponentA());
            entity4.Add(new ComponentB());
            entity4.Add(new ComponentC());

            Assert.Equal(3, familyEntities.Count);
            Assert.Contains(entity1, familyEntities);
            Assert.Contains(entity3, familyEntities);
            Assert.Contains(entity4, familyEntities);
            Assert.DoesNotContain(entity2, familyEntities);
        }

        [Fact]
        public void EntitiesForFamilyWithRemoval()
        {
            var engine = new Engine();
            
            var family = Family.WithAllOf<ComponentA, ComponentB>().Build();
            var familyEntities = engine.GetEntitiesFor(family);
            
            var entity1 = new Entity();
            var entity2 = new Entity();
            var entity3 = new Entity();
            var entity4 = new Entity();

            engine.AddEntities(entity1, entity2, entity3, entity4);

            entity1.Add(new ComponentA());
            entity1.Add(new ComponentB());

            entity2.Add(new ComponentA());
            entity2.Add(new ComponentC());

            entity3.Add(new ComponentA());
            entity3.Add(new ComponentB());
            entity3.Add(new ComponentC());

            entity4.Add(new ComponentA());
            entity4.Add(new ComponentB());
            entity4.Add(new ComponentC());
            
            Assert.Equal(3, familyEntities.Count);
            Assert.Contains(entity1, familyEntities);
            Assert.Contains(entity3, familyEntities);
            Assert.Contains(entity4, familyEntities);
            Assert.DoesNotContain(entity2, familyEntities);
            
            entity1.Remove<ComponentA>();
            engine.RemoveEntity(entity3);

            Assert.Single(familyEntities);
            Assert.Contains(entity4, familyEntities);
            Assert.DoesNotContain(entity1, familyEntities);
            Assert.DoesNotContain(entity2, familyEntities);
            Assert.DoesNotContain(entity3, familyEntities);
        }

        [Fact]
        public void EntitiesForFamilyWithRemovalAndFiltering()
        {
            var engine = new Engine();

            var entitiesWithComponentAOnly =
                engine.GetEntitiesFor(Family.WithAllOf<ComponentA>().WithNoneOf<ComponentB>().Build());

            var entitiesWithComponentB = engine.GetEntitiesFor(Family.WithAllOf<ComponentB>().Build());

            var entity1 = new Entity();
            var entity2 = new Entity();

            engine.AddEntities(entity1, entity2);
            
            entity1.Add(new ComponentA());

            entity2.Add(new ComponentA());
            entity2.Add(new ComponentB());
            
            Assert.Single(entitiesWithComponentAOnly);
            Assert.Single(entitiesWithComponentB);

            entity2.Remove<ComponentB>();

            Assert.Equal(2, entitiesWithComponentAOnly.Count);
            Assert.Empty(entitiesWithComponentB);
        }

        [Fact]
        public void EntitySystemRemovalWhileIterating()
        {
            var engine = new Engine();

            engine.AddSystem(new CounterSystem());

            for (var i = 0; i < 20; i++)
            {
                var entity = new Entity();
                entity.Add(new CounterComponent());
                engine.AddEntity(entity);
            }

            var entities = engine.GetEntitiesFor(Family.WithAllOf<CounterComponent>().Build());

            foreach (var entity in entities)
            {
                var counterComponent = entity.GetComponent<CounterComponent>();
                Assert.NotNull(counterComponent);
                Assert.Equal(0, counterComponent.Counter);
            }

            engine.Update(_deltaTime);

            foreach (var entity in entities)
            {
                var counterComponent = entity.GetComponent<CounterComponent>();
                Assert.NotNull(counterComponent);
                Assert.Equal(1, counterComponent.Counter);
            }
        }

        public class ComponentAddSystem : IteratingSystem
        {
            private readonly ComponentAddedListener _listener;
            
            public ComponentAddSystem(ComponentAddedListener listener) : base(Family.WithAllOf().Build())
            {
                _listener = listener;
            }

            protected override void ProcessEntity(Entity entity, float deltaTime)
            {
                Assert.Null(entity.GetComponent<ComponentA>());
                entity.Add(new ComponentA());
                Assert.NotNull(entity.GetComponent<ComponentA>());
                _listener.CheckEntityListenerUpdate();
            }
        }
        
        public class ComponentRemoveSystem : IteratingSystem
        {
            private readonly ComponentRemovedListener _listener;
            
            public ComponentRemoveSystem(ComponentRemovedListener listener) : base(Family.WithAllOf().Build())
            {
                _listener = listener;
            }

            protected override void ProcessEntity(Entity entity, float deltaTime)
            {
                Assert.NotNull(entity.GetComponent<ComponentA>());
                entity.Remove<ComponentA>();
                Assert.Null(entity.GetComponent<ComponentA>());
                _listener.CheckEntityListenerUpdate();
            }
        }

        public class ComponentAddedListener : IEntityListener
        {
            internal int AddedCalls;
            internal readonly int EntityCount;

            public ComponentAddedListener(int entityCount)
            {
                EntityCount = entityCount;
            }
            
            public void EntityAdded(Entity entity)
            {
                AddedCalls++;
            }

            public void EntityRemoved(Entity entity)
            {
            }

            public void CheckEntityListenerNonUpdate()
            {
                Assert.Equal(EntityCount, AddedCalls);
                AddedCalls = 0;
            }

            public void CheckEntityListenerUpdate()
            {
                Assert.Equal(0, AddedCalls);
            }
        }
        
        public class ComponentRemovedListener : IEntityListener
        {
            internal int RemovedCalls;
            internal readonly int EntityCount;

            public ComponentRemovedListener(int entityCount)
            {
                EntityCount = entityCount;
            }
            
            public void EntityAdded(Entity entity)
            {
            }

            public void EntityRemoved(Entity entity)
            {
                RemovedCalls++;
            }

            public void CheckEntityListenerNonUpdate()
            {
                Assert.Equal(EntityCount, RemovedCalls);
                RemovedCalls = 0;
            }

            public void CheckEntityListenerUpdate()
            {
                Assert.Equal(0, RemovedCalls);
            }
        }

        [Fact]
        public void EntityAddRemoveComponentWhileIterating()
        {
            var entityCount = 20;
            var engine = new Engine();
            var addedListener = new ComponentAddedListener(entityCount);
            var addSystem = new ComponentAddSystem(addedListener);

            var removedListener = new ComponentRemovedListener(entityCount);
            var removeSystem = new ComponentRemoveSystem(removedListener);

            for (var i = 0; i < entityCount; i++)
            {
                engine.AddEntity(new Entity());
            }

            engine.AddEntityListener(addedListener, Family.WithAllOf<ComponentA>().Build());
            engine.AddEntityListener(removedListener, Family.WithAllOf<ComponentA>().Build());

            engine.AddSystem(addSystem);
            engine.Update(_deltaTime);
            addedListener.CheckEntityListenerNonUpdate();
            engine.RemoveSystem(addSystem);

            engine.AddSystem(removeSystem);
            engine.Update(_deltaTime);
            removedListener.CheckEntityListenerNonUpdate();
            engine.RemoveSystem(removeSystem);
        }

        public class GenericEntityListener : IEntityListener
        {
            private readonly Action<Entity> _entityAdded;
            private readonly Action<Entity> _entityRemoved;

            public GenericEntityListener(Action<Entity> entityAdded, Action<Entity> entityRemoved)
            {
                _entityAdded = entityAdded;
                _entityRemoved = entityRemoved;
            }
            
            public void EntityAdded(Entity entity) => _entityAdded?.Invoke(entity);

            public void EntityRemoved(Entity entity) => _entityRemoved?.Invoke(entity);
        }

        public class GenericEntitySystem : EntitySystem
        {
            private readonly Action<GenericEntitySystem, float> _update;

            public GenericEntitySystem(Action<GenericEntitySystem, float> update)
            {
                _update = update;
            }

            public override void Update(float deltaTime) => _update?.Invoke(this, deltaTime);
        }
        
        [Fact]
        public void CascadeOperationsInListenersWhileUpdating()
        {
            const int entityCount = 20;
            var engine = new Engine();
            var addedListener = new ComponentAddedListener(entityCount);
            var removedListener = new ComponentRemovedListener(entityCount);

            var entities = new List<Entity>();
            
            engine.AddEntityListener(family: Family.WithAllOf<ComponentA>().Build(), listener: new GenericEntityListener(
                entity =>
                {
                    if (entities.Count >= entityCount) return;
                    var e = new Entity();
                    engine.AddEntity(e);
                },
                engine.RemoveEntity));
            
            engine.AddEntityListener(listener: new GenericEntityListener(
                entity =>
                {
                    entities.Add(entity);
                    entity.Add(new ComponentA());
                },
                entity =>
                {
                    entities.Remove(entity);
                    entities.FirstOrDefault()?.Remove<ComponentA>();
                }));
            
            engine.AddEntityListener(addedListener, Family.WithAllOf<ComponentA>().Build());
            engine.AddEntityListener(removedListener, Family.WithAllOf<ComponentA>().Build());

            var addSystem =
                new GenericEntitySystem((genericEntitySystem, _) => genericEntitySystem.Engine.AddEntity(new Entity()));

            engine.AddSystem(addSystem);
            engine.Update(_deltaTime);
            engine.RemoveSystem(addSystem);
            addedListener.CheckEntityListenerNonUpdate();
            removedListener.CheckEntityListenerUpdate();
            
            var removeSystem =
                new GenericEntitySystem((genericEntitySystem, _) => genericEntitySystem.Engine.RemoveEntity(entities.First()));

            engine.AddSystem(removeSystem);
            engine.Update(_deltaTime);
            engine.RemoveSystem(removeSystem);
            addedListener.CheckEntityListenerUpdate();
            removedListener.CheckEntityListenerNonUpdate();
        }

        [Fact]
        public void FamilyListener()
        {
            var engine = new Engine();
            
            var listenerA = new EntityListenerMock();
            var listenerB = new EntityListenerMock();

            var familyA = Family.WithAllOf<ComponentA>().Build();
            var familyB = Family.WithAllOf<ComponentB>().Build();

            engine.AddEntityListener(listenerA, familyA);
            engine.AddEntityListener(listenerB, familyB);

            var entity1 = new Entity();
            engine.AddEntity(entity1);

            Assert.Equal(0, listenerA.AddedCount);
            Assert.Equal(0, listenerB.AddedCount);

            var entity2 = new Entity();
            engine.AddEntity(entity2);

            Assert.Equal(0, listenerA.AddedCount);
            Assert.Equal(0, listenerB.AddedCount);

            entity1.Add(new ComponentA());

            Assert.Equal(1, listenerA.AddedCount);
            Assert.Equal(0, listenerB.AddedCount);

            entity2.Add(new ComponentB());
            
            Assert.Equal(1, listenerA.AddedCount);
            Assert.Equal(1, listenerB.AddedCount);

            entity1.Remove<ComponentA>();
            
            Assert.Equal(1, listenerA.RemovedCount);
            Assert.Equal(0, listenerB.RemovedCount);

            engine.RemoveEntity(entity2);

            Assert.Equal(1, listenerA.RemovedCount);
            Assert.Equal(1, listenerB.RemovedCount);

            engine.RemoveEntityListener(listenerB);

            engine.AddEntity(entity2);

            Assert.Equal(1, listenerA.AddedCount);
            Assert.Equal(1, listenerB.AddedCount);

            entity1.Add(new ComponentB());
            entity1.Add(new ComponentA());

            Assert.Equal(2, listenerA.AddedCount);
            Assert.Equal(1, listenerB.AddedCount);

            engine.RemoveAllEntities();

            Assert.Equal(2, listenerA.RemovedCount);
            Assert.Equal(1, listenerB.RemovedCount);

            engine.AddEntityListener(listenerB);
            
            engine.AddEntity(entity1);
            engine.AddEntity(entity2);

            Assert.Equal(3, listenerA.AddedCount);
            Assert.Equal(3, listenerB.AddedCount);

            engine.RemoveAllEntities(familyA);

            Assert.Equal(3, listenerA.RemovedCount);
            Assert.Equal(2, listenerB.RemovedCount);

            engine.RemoveAllEntities(familyB);
            
            Assert.Equal(3, listenerA.RemovedCount);
            Assert.Equal(3, listenerB.RemovedCount);
        }

        [Fact]
        public void CreateManyEntitiesNoStackOverflow()
        {
            var engine = new Engine();
            engine.AddSystem(new CounterSystem());

            for (var i = 0; i < 15000; i++)
            {
                var e = new Entity();
                e.Add(new CounterComponent());
                engine.AddEntity(e);
            }

            engine.Update(0f);
        }

        [Fact]
        public void GetEntities()
        {
            var entityCount = 10;
            var engine = new Engine();
            var entities = new List<Entity>();

            for (var i = 0; i < entityCount; i++)
            {
                var entity = new Entity();
                entities.Add(entity);
                engine.AddEntity(entity);
            }

            var engineEntities = engine.Entities;

            Assert.Equal(entities, engineEntities);
            
            engine.RemoveAllEntities();

            Assert.Empty(engineEntities);
        }

        [Fact]
        public void NestedUpdateException()
        {   
            var engine = new Engine();
            
            var duringCallback = false;
            var system = new GenericEntitySystem((self, dt) =>
            {
                if (!duringCallback)
                {
                    duringCallback = true;
                    self.Engine.Update(dt);
                    duringCallback = false;
                }
            });
            engine.AddSystem(system);

            Assert.Throws<InvalidOperationException>(() => engine.Update(_deltaTime));
        }

        [Fact]
        public void SystemUpdateThrows()
        {
            var engine = new Engine();
            var system = new GenericEntitySystem((_, dt) => throw new Exception("throwing"));

            engine.AddSystem(system);

            Assert.Throws<Exception>(() => engine.Update(0f));

            engine.RemoveSystem(system);

            engine.Update(0f);
        }

        [Fact]
        public void CreateNewEntity()
        {
            var engine = new Engine();
            var entity = engine.CreateEntity();

            Assert.NotNull(entity);
        }

        [Fact]
        public void CreateNewComponent()
        {
            var engine = new Engine();
            var component = engine.CreateComponent<ComponentD>();

            Assert.NotNull(component);
        }
    }
}