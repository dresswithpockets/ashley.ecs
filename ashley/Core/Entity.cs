using System;
using System.Collections.Generic;
using ashley.Signals;
using ashley.Utils;

namespace ashley.Core
{
    public class Entity
    {
        private static readonly Type ComponentType = typeof(IComponent);
        
        /// <summary>
        /// A customizable bitmask. Up to the user to manage.
        /// </summary>
        public long Flags { get; set; }
        
        public ImmutableList<IComponent> Components { get; }

        internal BitSet ComponentBits { get; }

        internal BitSet FamilyBits { get; }
        
        public bool ScheduledForRemoval { get; internal set; }

        /// <summary>
        /// Will dispatch an event when a component is added.
        /// </summary>
        public Signal<Entity> ComponentAdded { get; } = new Signal<Entity>();

        /// <summary>
        /// Will dispatch an event when a component is removed.
        /// </summary>
        public Signal<Entity> ComponentRemoved { get; } = new Signal<Entity>();

        internal ComponentOperationHandler ComponentOperationHandler;
        internal bool Removing;
        
        private readonly Bag<IComponent> _components;
        private readonly List<IComponent> _componentsList;

        public Entity()
        {
            _components = new Bag<IComponent>();
            _componentsList = new List<IComponent>(16);
            Components = new ImmutableList<IComponent>(_componentsList);
            ComponentBits = new BitSet();
            FamilyBits = new BitSet();
        }

        public Entity Add(IComponent component)
        {
            if (AddInternal(component))
            {
                if (ComponentOperationHandler != null)
                {
                    ComponentOperationHandler.Add(this);
                }
                else
                {
                    OnComponentAdded();
                }
            }

            return this;
        }

        public IComponent AddAndReturn(IComponent component)
        {
            Add(component);
            return component;
        }

        public IComponent Remove(Type type)
        {
            if (!ComponentType.IsAssignableFrom(type) || !type.IsClass)
                throw new ArgumentException("The type must be a class that implements IComponent",
                    nameof(type));
            
            var componentType = Core.ComponentType.GetFor(type);

            if (_components.IsIndexWithinBounds(componentType.Index))
            {
                if (_components.TryGet(componentType.Index, out var removeComponent) && RemoveInternal(componentType) != null)
                {
                    if (ComponentOperationHandler != null)
                    {
                        ComponentOperationHandler.Remove(this);
                    }
                    else
                    {
                        OnComponentRemoved();
                    }
                }

                return removeComponent;
            }

            return null;
        }

        public TComponent Remove<TComponent>() where TComponent : class, IComponent
            => Remove(typeof(TComponent)) as TComponent;

        public void RemoveAll()
        {
            while (_componentsList.Count > 0)
            {
                Remove(_componentsList[0].GetType());
            }
        }

        public TComponent GetComponent<TComponent>() where TComponent : class, IComponent
            => GetComponent(Core.ComponentType.GetFor<TComponent>()) as TComponent;

        internal IComponent GetComponent(ComponentType componentType)
        {
            if (componentType.Index < _components.Capacity)
            {
                if (_components.TryGet(componentType.Index, out var component)) return component;
            }

            return null;
        }

        internal bool HasComponent(ComponentType componentType) => ComponentBits.Get(componentType.Index);

        internal virtual bool AddInternal(IComponent component)
        {
            if (component == null) return false;
            
            var type = Core.ComponentType.GetFor(component.GetType());
            var oldComponent = GetComponent(type);
            
            if (component == oldComponent)
            {
                return false;
            }

            if (oldComponent != null)
            {
                RemoveInternal(type);
            }

            _components.Set(type.Index, component);
            _componentsList.Add(component);
            ComponentBits.Set(type.Index);
            
            return true;
        }

        internal virtual IComponent RemoveInternal(ComponentType componentType)
        {            
            if (_components.TryGet(componentType.Index, out var removeComponent))
            {
                _components.Set(componentType.Index, null);
                _componentsList.Remove(removeComponent);
                ComponentBits.Clear(componentType.Index);

                return removeComponent;
            }

            return null;
        }

        internal virtual void OnComponentAdded()
        {
            ComponentAdded.Dispatch(this);
        }

        internal virtual void OnComponentRemoved()
        {
            ComponentRemoved.Dispatch(this);
        }
    }
}