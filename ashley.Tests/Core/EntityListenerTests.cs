using ashley.Core;
using Xunit;

namespace ashley.Tests.Core
{
    public class EntityListenerTests
    {
        [Fact]
        public void AddEntityListenerFamilyRemove()
        {
            var engine = new Engine();

            var e = new Entity();
            e.Add(new PositionComponent());

            var family = Family.WithAllOf<PositionComponent>().Build();
            engine.AddEntityListener(new EngineTests.GenericEntityListener(entity => engine.AddEntity(new Entity()),
                _ => { }), family);

            engine.RemoveEntity(e);
        }

        [Fact]
        public void AddEntityListenerFamilyAdd()
        {
            var engine = new Engine();
            var e = new Entity();
            e.Add(new PositionComponent());

            var family = Family.WithAllOf<PositionComponent>().Build();
            engine.AddEntityListener(
                new EngineTests.GenericEntityListener(_ => { }, entity => engine.AddEntity(new Entity())), family);

            engine.AddEntity(e);
        }

        private class PositionComponent : IComponent
        {
        }

        private class ComponentA : IComponent
        {
        }

        private class ComponentB : IComponent
        {
        }

        private interface ComponentRecorder
        {
            void AddingComponentA();
            void RemovingComponentA();
            void AddingComponentB();
            void RemovingComponentB();
        }
    }
}