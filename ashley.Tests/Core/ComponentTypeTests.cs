using ashley.Core;
using Xunit;

namespace ashley.Tests.Core
{
    public class ComponentTypeTests
    {
        private class ComponentA : IComponent
        {
        }

        private class ComponentB : IComponent
        {
        }

        [Fact]
        public void ValidComponentType()
        {
            Assert.NotNull(ComponentType.GetFor<ComponentA>());
            Assert.NotNull(ComponentType.GetFor<ComponentB>());
        }

        [Fact]
        public void SameComponentType()
        {
            var componentType1 = ComponentType.GetFor<ComponentA>();
            var componentType2 = ComponentType.GetFor<ComponentA>();

            Assert.Equal(componentType1, componentType2);
            Assert.Equal(componentType2, componentType1);

            Assert.Equal(componentType1.Index, componentType2.Index);
            Assert.Equal(componentType1.Index, ComponentType.GetIndexFor<ComponentA>());
            Assert.Equal(componentType2.Index, ComponentType.GetIndexFor<ComponentA>());
        }

        [Fact]
        public void DifferentComponentType()
        {
            var componentType1 = ComponentType.GetFor<ComponentA>();
            var componentType2 = ComponentType.GetFor<ComponentB>();

            Assert.NotEqual(componentType1, componentType2);
            Assert.NotEqual(componentType2, componentType1);

            Assert.NotEqual(componentType1.Index, componentType2.Index);
            Assert.NotEqual(componentType1.Index, ComponentType.GetIndexFor<ComponentB>());
            Assert.NotEqual(componentType2.Index, ComponentType.GetIndexFor<ComponentA>());
        }
    }
}