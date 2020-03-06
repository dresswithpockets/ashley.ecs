namespace ashley.Core
{
    public class ComponentMapper<T> where T : class, IComponent
    {
        public static ComponentMapper<TComponent> GetFor<TComponent>()
            where TComponent : class, IComponent =>
            new ComponentMapper<TComponent>();

        public T GetFrom(Entity entity) => entity.GetComponent<T>();

        public bool In(Entity entity) => entity.HasComponent(ComponentType.GetFor<T>());
        
        private ComponentMapper() {}
    }
}