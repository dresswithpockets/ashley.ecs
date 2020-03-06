namespace ashley.Core
{
    /// <summary>
    /// Fast map from an Entity to a Component.
    /// </summary>
    /// <typeparam name="T">The class implementing <see cref="IComponent"/> to map to</typeparam>
    public class ComponentMapper<T> where T : class, IComponent
    {
        private readonly ComponentType _componentType;
        
        /// <summary>
        /// returns a ComponentMapper that provides fast access to the <see cref="IComponent"/> type specified
        /// </summary>
        /// <typeparam name="TComponent">component class to be retrieved by the mapper</typeparam>
        /// <returns>new instance that provides fast access to the <see cref="IComponent"/> type specified</returns>
        public static ComponentMapper<TComponent> GetFor<TComponent>()
            where TComponent : class, IComponent =>
            new ComponentMapper<TComponent>();

        /// <summary>
        /// returns the instance of the component type type belonging to a specific entity
        /// </summary>
        /// <returns>the instance of the component type belonging to a specific entity</returns>
        public T GetFrom(Entity entity) => entity.GetComponent<T>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if the entity has an instance of the specified component type.</returns>
        public bool In(Entity entity) => entity.HasComponent(_componentType);

        private ComponentMapper()
        {
            _componentType = ComponentType.GetFor<T>();
        }
    }
}