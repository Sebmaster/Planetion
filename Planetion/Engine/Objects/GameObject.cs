using System;
using System.Collections.Generic;

namespace Planetion.Objects {
	public class GameObject {

		public Transform Transform { get; set; }

		public World World { get; set; }

		internal Dictionary<Type, Component> Components { get; private set; }

		public List<GameObject> Children { get; private set; }

		public GameObject(World world) {
			World = world;
			Children = new List<GameObject>();
			Components = new Dictionary<Type, Component>();

			Transform = new Transform(this);
		}

        public Component AddComponent(Type t) {
            if (!t.IsSubclassOf(typeof(Component))) throw new ArgumentException("The given type has to be a subclass of Component!");

            Component c = (Component)Activator.CreateInstance(t);
            c.GameObject = this;
            Components.Add(t, c);

            foreach (var att in t.GetCustomAttributes(typeof(Planetion.Utils.RequireComponent), true)) {
                Type[] requiredTypes = ((Planetion.Utils.RequireComponent)att).Types;
                foreach (Type requiredType in requiredTypes) {
                    if (!HasComponent(requiredType)) {
                        AddComponent(requiredType);
                    }
                }
            }

            c.Awake();
            return c;
        }

        public T AddComponent<T>() where T : Component, new() {
            T c = new T();
            c.GameObject = this;
            Components.Add(typeof(T), c);

            foreach (var att in typeof(T).GetCustomAttributes(typeof(Planetion.Utils.RequireComponent), true)) {
                Type[] requiredTypes = ((Planetion.Utils.RequireComponent)att).Types;
                foreach (Type requiredType in requiredTypes) {
                    if (!HasComponent(requiredType)) {
                        AddComponent(requiredType);
                    }
                }
            }

            c.Awake();
            return c;
        }

        public bool HasComponent(Type t) {
            return Components.ContainsKey(t);
        }

        public bool HasComponent<T>() where T : Component {
            return HasComponent(typeof(T));
        }

        public T GetComponent<T>(bool subClasses = false) where T : Component {
            if (!subClasses) {
                Component cmp;
                Components.TryGetValue(typeof(T), out cmp);
                return cmp as T;
            } else {
                foreach (Component c in Components.Values) {
                    if (c is T) {
                        return c as T;
                    }
                }
            }
            return null;
        }

		public virtual void Update(float delta) {
			foreach (Component c in Components.Values) {
				c.Update(delta);
			}
		}
	}
}
