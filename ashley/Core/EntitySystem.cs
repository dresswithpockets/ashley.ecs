namespace ashley.Core
{
    public abstract class EntitySystem
    {
        public int Priority { get; set; }

        public virtual bool Processing { get; set; } = true;

        public Engine Engine { get; private set; }

        public EntitySystem(int priority = 0)
        {
            Priority = priority;
        }

        public virtual void AddedToEngine(Engine engine)
        {
        }

        public virtual void RemovedFromEngine(Engine engine)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        internal void AddedToEngineInternal(Engine engine)
        {
            Engine = engine;
            AddedToEngine(engine);
        }

        internal void RemovedFromEngineInternal(Engine engine)
        {
            Engine = null;
            RemovedFromEngine(engine);
        }
    }
}