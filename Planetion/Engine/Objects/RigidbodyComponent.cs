
namespace Planetion.Objects {
	public class RigidbodyComponent : Component {

		private Jitter.Dynamics.RigidBody body;
		public Jitter.Dynamics.RigidBody Body {
			get { return body; }
			set {
                lock (GameObject.World.PhysicWorld) {
                    if (body != null) {
                        GameObject.World.PhysicWorld.RemoveBody(body);
                    }
                    body = value;
                    GameObject.World.PhysicWorld.AddBody(body);
                    body.Tag = this;
                }
			}
		}

        public OpenTK.Vector3 Offset { get; set; }

        public OpenTK.Vector3 Velocity {
            get {
                var velocity = Body.LinearVelocity;
                return new OpenTK.Vector3(velocity.X, velocity.Y, velocity.Z);
            }

            set {
                Body.LinearVelocity.Set(value.X, value.Y, value.Z);
            }
        }

		~RigidbodyComponent() {
            if (Body != null) {
                lock (GameObject.World.PhysicWorld) {
                    GameObject.World.PhysicWorld.RemoveBody(Body);
                }
            }
		}
	}
}

