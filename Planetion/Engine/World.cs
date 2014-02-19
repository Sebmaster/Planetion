using System.Collections.Generic;
using Planetion.Objects;
using Planetion.Utils;
using System.Diagnostics;
using System;
using Planetion.Internals;

namespace Planetion {
    public class World {

        private struct CoroutineExec {
            public Stopwatch sw;
            public IEnumerator<long> ie;
            public Func<IEnumerator<long>> func;
            public Func<object, IEnumerator<long>> paramFunc;
        }

        public GroupedList<GameObject> Objects = new GroupedList<GameObject>();
        public List<Light> Lights = new List<Light>();
        public List<WorldScript> Scripts = new List<WorldScript>();

        public float TimeFactor = 1;
        public float PhysicsFactor = 1;

        public Jitter.World PhysicWorld = new Jitter.World(new Jitter.Collision.CollisionSystemSAP());

        private void SetPhysicsToWorld() {
            foreach (GameObject obj in Objects) {
                RigidbodyComponent rigid = obj.GetComponent<RigidbodyComponent>();
                if (rigid != null) {
                    Jitter.LinearMath.JVector vec = obj.GetComponent<RigidbodyComponent>().Body.Position;
                    obj.Transform.Position = new OpenTK.Vector3(vec.X, vec.Y, vec.Z) + rigid.Offset;
                }
            }
        }

        private void SetWorldToPhysics() {
            foreach (GameObject obj in Objects) {
                RigidbodyComponent rigid = obj.GetComponent<RigidbodyComponent>();
                if (rigid != null) {
                    OpenTK.Vector3 vec = obj.Transform.Position - rigid.Offset;
                    Jitter.Dynamics.RigidBody bdy = obj.GetComponent<RigidbodyComponent>().Body;
                    bdy.Position.Set(vec.X, vec.Y, vec.Z);
                    bdy.Update();
                }
            }
        }

        public GameObject Instantiate() {
            GameObject obj = new GameObject(this);
            Objects.Add(obj);
            return obj;
        }

        public WorldScript InstantiateScript<T>() where T : WorldScript, new() {
            WorldScript ws = new T();
            ws.World = this;
            this.Scripts.Add(ws);
            ws.Awake();
            return ws;
        }

        public T FindObjectByComponent<T>(bool subClasses = false) where T : Component {
            foreach (GameObject go in this.Objects) {
                Component c = go.GetComponent<T>(subClasses);
                if (c != null) {
                    return c as T;
                }
            }
            return null;
        }

        private List<CoroutineExec> Coroutines = new List<CoroutineExec>();

        #region Coroutines
        public void StartCoroutine(Func<IEnumerator<long>> func) {
            IEnumerator<long> ie = func();
            Stopwatch sw = new Stopwatch();
            if (ie.MoveNext()) {
                Coroutines.Add(new CoroutineExec() { sw = sw, ie = ie, func = func });
                sw.Start();
            }
        }
        public void StartCoroutine(Func<object, IEnumerator<long>> func, object o) {
            IEnumerator<long> ie = func(o);
            Stopwatch sw = new Stopwatch();
            if (ie.MoveNext()) {
                Coroutines.Add(new CoroutineExec() { sw = sw, ie = ie, paramFunc = func });
                sw.Start();
            }
        }

        public void StopCoroutine(Func<IEnumerator<long>> func) {
            foreach (CoroutineExec ce in Coroutines) {
                if (ce.func == func) {
                    Coroutines.Remove(ce);
                    break;
                }
            }
        }

        public void StopCoroutine(Func<object, IEnumerator<long>> func) {
            foreach (CoroutineExec ce in Coroutines) {
                if (ce.paramFunc == func) {
                    Coroutines.Remove(ce);
                    break;
                }
            }
        }
        #endregion

        public void Update(double delta) {
            if (TimeFactor != 0 && PhysicsFactor != 0) {
                SetWorldToPhysics();
                PhysicWorld.Step((float)(PhysicsFactor * TimeFactor * delta), true);
                SetPhysicsToWorld();
            }

            for (int i = 0; i < Coroutines.Count; ++i) {
                CoroutineExec ce = Coroutines[i];
                if (ce.ie.Current <= ce.sw.ElapsedMilliseconds) {
                    if (ce.ie.MoveNext()) {
                        ce.sw.Reset();
                        ce.sw.Start();
                    } else {
                        Coroutines.RemoveAt(i--);
                    }
                }
            }

            foreach (WorldScript s in this.Scripts) {
                s.Update((float)(TimeFactor * delta));
            }

            UpdateGameObjects((float)(TimeFactor * delta));
        }

        protected virtual void UpdateGameObjects(float delta) {
            foreach (GameObject obj in Objects) {
                obj.Update(delta);
            }
        }
    }
}

