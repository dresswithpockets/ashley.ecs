namespace ashley.Core
{
    public interface IEntityListener
    {
        void EntityAdded(Entity entity);
        void EntityRemoved(Entity entity);
    }
}