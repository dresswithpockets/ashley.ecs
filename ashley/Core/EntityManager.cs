using System;
using System.Collections.Generic;
using System.Linq;
using ashley.Utils;

namespace ashley.Core
{
    internal class EntityManager
    {
        private IEntityListener _listener;
        private List<Entity> _entities = new List<Entity>(16);
        private HashSet<Entity> _entitySet = new HashSet<Entity>();
        private List<EntityOperation> _pendingOperations = new List<EntityOperation>(16);
        private Pool<EntityOperation> _entityOperationPool = new Pool<EntityOperation>(() => new EntityOperation());

        public ImmutableList<Entity> Entities { get; }

        public bool HasPendingOperations => _pendingOperations.Count > 0;

        public EntityManager(IEntityListener listener)
        {
            _listener = listener;
            Entities = new ImmutableList<Entity>(_entities);
        }

        public void AddEntity(Entity entity, bool delayed = false)
        {
            if (delayed)
            {
                var operation = _entityOperationPool.Obtain();
                operation.Entity = entity;
                operation.OperationType = EntityOperation.Type.Add;
                _pendingOperations.Add(operation);
            }
            else
            {
                AddEntityInternal(entity);
            }
        }

        public void RemoveEntity(Entity entity, bool delayed = false)
        {
            if (delayed)
            {
                if (entity.ScheduledForRemoval) return;
                entity.ScheduledForRemoval = true;
                var operation = _entityOperationPool.Obtain();
                operation.Entity = entity;
                operation.OperationType = EntityOperation.Type.Remove;
                _pendingOperations.Add(operation);
            }
            else
            {
                RemoveEntityInternal(entity);
            }
        }

        public void RemoveAllEntities(bool delayed = false)
        {
            RemoveAllEntities(Entities, delayed);
        }

        public void RemoveAllEntities(ImmutableList<Entity> entities, bool delayed = false)
        {
            if (delayed)
            {
                foreach (var entity in entities)
                {
                    entity.ScheduledForRemoval = true;
                }

                var operation = _entityOperationPool.Obtain();
                operation.OperationType = EntityOperation.Type.RemoveAll;
                operation.Entities = entities;
                _pendingOperations.Add(operation);
            }
            else
            {
                while (entities.Count > 0)
                {
                    RemoveEntity(entities.First());
                }
            }
        }

        public void ProcessPendingOperations()
        {
            for (var i = 0; i < _pendingOperations.Count; i++)
            {
                var operation = _pendingOperations[i];
                switch (operation.OperationType)
                {
                    case EntityOperation.Type.Add:
                        AddEntityInternal(operation.Entity);
                        break;
                    case EntityOperation.Type.Remove:
                        RemoveEntityInternal(operation.Entity);
                        break;
                    case EntityOperation.Type.RemoveAll:
                        while (operation.Entities.Count > 0)
                        {
                            RemoveEntityInternal(operation.Entities.First());
                        }
                        break;
                }
                
                _entityOperationPool.Free(operation);
            }

            _pendingOperations.Clear();
        }

        protected void AddEntityInternal(Entity entity)
        {
            if (_entitySet.Contains(entity))
            {
                throw new ArgumentException($"Entity is already registered {entity}");
            }

            _entities.Add(entity);
            _entitySet.Add(entity);

            _listener.EntityAdded(entity);
        }

        protected void RemoveEntityInternal(Entity entity)
        {
            if (_entitySet.Remove(entity))
            {
                entity.ScheduledForRemoval = false;
                entity.Removing = true;
                _entities.Remove(entity);
                _listener.EntityRemoved(entity);
                entity.Removing = false;
            }
        }

        private class EntityOperation : IPoolable
        {
            public enum Type
            {
                Add,
                Remove,
                RemoveAll
            }

            public Type OperationType { get; set; }

            public Entity Entity { get; set; }

            public ImmutableList<Entity> Entities { get; set; }
            
            public void Reset()
            {
                Entity = null;
            }
        }
    }
}