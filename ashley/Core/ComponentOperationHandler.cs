using System;
using ashley.Utils;

namespace ashley.Core
{
    public class ComponentOperationHandler
    {
        private IBooleanInformer _delayedInformer;
        private Pool<ComponentOperation> _operationPool = new Pool<ComponentOperation>(() => new ComponentOperation());
        private Bag<ComponentOperation> _operations = new Bag<ComponentOperation>();

        public ComponentOperationHandler(IBooleanInformer delayedInformer)
        {
            _delayedInformer = delayedInformer;
        }

        public void Add(Entity entity)
        {
            if (_delayedInformer.Value)
            {
                var operation = _operationPool.Obtain();
                operation.MakeAdd(entity);
                _operations.Add(operation);
            }
            else
            {
                entity.OnComponentAdded();
            }
        }

        public void Remove(Entity entity)
        {
            if (_delayedInformer.Value)
            {
                var operation = _operationPool.Obtain();
                operation.MakeRemove(entity);
                _operations.Add(operation);
            }
            else
            {
                entity.OnComponentRemoved();
            }
        }

        public bool HasOperationsToProcess => _operations.Size > 0;

        public void ProcessOperations()
        {
            foreach (var operation in _operations)
            {
                switch (operation.Type)
                {
                    case ComponentOperationType.Add:
                        operation.Entity.OnComponentAdded();
                        break;
                    case ComponentOperationType.Remove:
                        operation.Entity.OnComponentRemoved();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _operationPool.Free(operation);
            }

            _operations.Clear();
        }
    }
}