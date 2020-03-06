using ashley.Core;
using ashley.Utils;

namespace ashley.Systems
{
    /// <summary>
    /// A simple EntitySystem that iterates over each entity and calls ProcessEntity() for each entity every time the
    /// EntitySystem is updated. This is really just a convenience class as most systems iterate over a list of entities
    ///
    /// Source/Credit: https://github.com/libgdx/ashley/blob/55241f5256c0ec186992262c7d598811bc4664fe/ashley/src/com/badlogic/ashley/systems/IntervalIteratingSystem.java
    /// </summary>
    public abstract class IteratingSystem : EntitySystem
    {
        public Family Family { get; }
        public ImmutableList<Entity> Entities { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="family">The family of entities iterated over in this system</param>
        /// <param name="priority">The priority to execute this system with (lower means higher priority)</param>
        public IteratingSystem(Family family, int priority = 0) : base(priority)
        {
            Family = family;
        }

        public override void AddedToEngine(Engine engine)
        {
            Entities = engine.GetEntitiesFor(Family);
        }

        public override void RemovedFromEngine(Engine engine)
        {
            Entities = null;
        }

        public override void Update(float deltaTime)
        {
            foreach (var entity in Entities)
            {
                ProcessEntity(entity, deltaTime);
            }
        }

        /// <summary>
        /// Called on every entity on every update call of the EntitySystem. Override this to implement your system's
        /// specific processing
        /// </summary>
        /// <param name="entity">the current entity being processed</param>
        /// <param name="deltaTime">The time since the last frame</param>
        protected abstract void ProcessEntity(Entity entity, float deltaTime);
    }
}