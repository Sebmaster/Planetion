using System;
using OpenTK;
using Planetion.Internals;
using System.Drawing;

#if PC
using OpenTK.Graphics.OpenGL;
using Planetion.Engine.Utils;
using Planetion.Game.Objects;
#endif

#if ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Planetion.Objects {
    public class CameraComponent : Component, Planetion.Internals.ICamera {

        public enum Rendering {
            FORWARD,
#if PC
            DEFERRED
#endif
        }

        public Vector3 Direction { get; set; }

        public RectangleF NormalizedViewport { get; set; }

        public Rendering Mode;

        public float FOV = (float)Math.PI / 2;

        public float NearPlane = 1;

        public float FarPlane = 4000;

        private Renderer rend;

        private Rectangle lastRect = new Rectangle(0, 0, 0, 0);

        public CameraComponent() {
            _frustum = new Engine.Utils.Frustum();
            Direction = new Vector3(1, 0, 0);
            NormalizedViewport = new RectangleF(0, 0, 1, 1);
        }

        private void ReinitViewport(Rectangle Viewport) {
            GL.Viewport(Viewport);
            if (rend != null) rend.Dispose();

            if (Mode == Rendering.FORWARD) {
                rend = new ForwardRenderer(Viewport.Width, Viewport.Height);
            } else {
                rend = new DeferredRenderer(Viewport.Width, Viewport.Height);
            }
            lastRect = Viewport;
        }

        public void Render() {
            Size Viewport = GameSurface.CurrentSurface.Window.ClientSize;

            Rectangle nowRect = new Rectangle((int)(Viewport.Width * NormalizedViewport.X), (int)(Viewport.Width * NormalizedViewport.Y),
                                              (int)(Viewport.Width * NormalizedViewport.Width), (int)(Viewport.Height * NormalizedViewport.Height));

            if (nowRect != lastRect) {
                ReinitViewport(nowRect);
            }
            _projection = Matrix4.CreatePerspectiveFieldOfView(this.FOV, (Viewport.Width * NormalizedViewport.Width) / (Viewport.Height * NormalizedViewport.Height), this.NearPlane, this.FarPlane);
            _view = Matrix4.LookAt(GameObject.Transform.Position, GameObject.Transform.Position + Direction, Vector3.UnitY);
            if (float.IsNaN(View.M11))
                _view = Matrix4.Identity;
            _frustum.SetCamInternals(this.FOV, (Viewport.Width * NormalizedViewport.Width) / (Viewport.Height * NormalizedViewport.Height), this.NearPlane, this.FarPlane);
            _frustum.SetCamDef(GameObject.Transform.Position, GameObject.Transform.Position + Direction, Vector3.UnitY);
            rend.Render(this.GameObject.World.Objects, this.GameObject.World.Lights, ref _projection, ref _view, this);
        }

        public void LookAt(Vector3 target) {
            Direction = target - GameObject.Transform.LocalPosition;
        }

        public Vector3 Position {
            get { return GameObject.Transform.Position; }
        }

        Frustum _frustum;
        public Engine.Utils.Frustum Frustum {
            get { return _frustum; }
        }

        Matrix4 _view;
        public Matrix4 View {
            get { return _view; }
        }

        Matrix4 _projection;
        public Matrix4 Projection {
            get { return _projection; }
        }

        public GameObject Raycast(int x, int y, out Vector3? hitPoint) {
            Size Viewport = GameSurface.CurrentSurface.Window.ClientSize;
            Rectangle nowRect = new Rectangle((int)(Viewport.Width * NormalizedViewport.X), (int)(Viewport.Width * NormalizedViewport.Y),
                                              (int)(Viewport.Width * NormalizedViewport.Width), (int)(Viewport.Height * NormalizedViewport.Height));

            Matrix4 invView = Matrix4.Invert(_view * _projection);

            Vector3 near = Vector3.TransformPerspective(new Vector3(2f * x / nowRect.Width - 1f, 1 - 2f * y / nowRect.Height, 0), invView);
            Vector3 far = Vector3.TransformPerspective(new Vector3(2f * x / nowRect.Width - 1f, 1 - 2f * y / nowRect.Height, 1), invView);
            Vector3 dir = far - near;

            var origin = new Jitter.LinearMath.JVector(near.X, near.Y, near.Z);
            var direction = new Jitter.LinearMath.JVector(dir.X, dir.Y, dir.Z);
            Jitter.Dynamics.RigidBody rigid;
            Jitter.LinearMath.JVector normal;
            float fraction;

            bool hit = this.GameObject.World.PhysicWorld.CollisionSystem.Raycast(origin, direction, null, out rigid, out normal, out fraction);
            if (hit) {
                Jitter.LinearMath.JVector hitVect = origin + direction * fraction;
                hitPoint = new Vector3(hitVect.X, hitVect.Y, hitVect.Z);
                RigidbodyComponent cmp = rigid.Tag as RigidbodyComponent;
                return cmp.GameObject;
            }
            hitPoint = null;
            return null;
        }

        public static Matrix4 CreateFromYawPitchRoll(float Yaw, float Pitch, float Roll) {
            float A = (float)Math.Cos(Pitch);
            float B = (float)Math.Sin(Pitch);
            float C = (float)Math.Cos(Yaw);
            float D = (float)Math.Sin(Yaw);
            float E = (float)Math.Cos(Roll);
            float F = (float)Math.Sin(Roll);

            float AD = A * D;
            float BD = B * D;
            Matrix4 mat = Matrix4.Identity;
            mat.M11 = C * E;
            mat.M12 = -C * F;
            mat.M13 = -D;
            mat.M21 = -BD * E + A * F;
            mat.M22 = BD * F + A * E;
            mat.M23 = -B * C;
            mat.M31 = AD * E + B * F;
            mat.M32 = -AD * F + B * E;
            mat.M33 = A * C;
            return mat;
        }
    }
}
