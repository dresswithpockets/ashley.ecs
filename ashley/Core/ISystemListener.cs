namespace ashley.Core
{
    public interface ISystemListener
    {
        void SystemAdded(EntitySystem system);
        void SystemRemoved(EntitySystem system);
    }
}