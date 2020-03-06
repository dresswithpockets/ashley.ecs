using ashley.Core;
using ashley.Signals;
using Xunit;

namespace ashley.Tests.Core
{
    public class ComponentOperationHandlerTests
    {
        private class BooleanInformerMock : IBooleanInformer
        {
            public bool Delayed;

            public bool Value => Delayed;
        }

        private class ComponentSpy : IListener<Entity>
        {
            public bool Called { get; private set; }

            public void Receive(Signal<Entity> signal, Entity value)
            {
                Called = true;
            }
        }

        [Fact]
        public void Add()
        {
            var spy = new ComponentSpy();
            var informer = new BooleanInformerMock();
            var handler = new ComponentOperationHandler(informer);

            var entity = new Entity {ComponentOperationHandler = handler};
            entity.ComponentAdded.Add(spy);

            handler.Add(entity);

            Assert.True(spy.Called);
        }

        [Fact]
        public void AddDelayed()
        {
            var spy = new ComponentSpy();
            var informer = new BooleanInformerMock();
            var handler = new ComponentOperationHandler(informer);
            
            informer.Delayed = true;

            var entity = new Entity {ComponentOperationHandler = handler};
            entity.ComponentAdded.Add(spy);
		
            handler.Add(entity);
		
            Assert.False(spy.Called);
            handler.ProcessOperations();
            Assert.True(spy.Called);
        }

        [Fact]
        public void Remove()
        {
            var spy = new ComponentSpy();
            var informer = new BooleanInformerMock();
            var handler = new ComponentOperationHandler(informer);

            var entity = new Entity {ComponentOperationHandler = handler};
            entity.ComponentRemoved.Add(spy);

            handler.Remove(entity);

            Assert.True(spy.Called);
        }

        [Fact]
        public void RemoveDelayed()
        {
            var spy = new ComponentSpy();
            var informer = new BooleanInformerMock();
            var handler = new ComponentOperationHandler(informer);

            informer.Delayed = true;

            var entity = new Entity {ComponentOperationHandler = handler};
            entity.ComponentRemoved.Add(spy);

            handler.Remove(entity);

            Assert.False(spy.Called);
            handler.ProcessOperations();
            Assert.True(spy.Called);
        }
    }
}