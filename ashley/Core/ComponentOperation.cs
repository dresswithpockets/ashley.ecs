namespace ashley.Core
{
    internal class ComponentOperation : IPoolable
    {
        public ComponentOperationType Type { get; set; }
        public Entity Entity { get; set; }

        public void MakeAdd(Entity entity)
        {
            Type = ComponentOperationType.Add;
            Entity = entity;
        }

        public void MakeRemove(Entity entity)
        {
            Type = ComponentOperationType.Remove;
            Entity = entity;
        }

        public void Reset()
        {
            Entity = null;
        }
    }
}