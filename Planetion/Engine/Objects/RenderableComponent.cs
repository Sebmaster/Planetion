using OpenTK;
using Planetion.Utils;

namespace Planetion.Objects {
    public abstract class RenderableComponent : Component {

        internal virtual object Identifier {
            get {
                return null;
            }
        }

        public virtual void BeginRenderGroup() { }

        public virtual void EndRenderGroup() { }

        public void GetModelMatrix(out Matrix4 resMatrix, GeometryHelpers.TransformSetters setters = GeometryHelpers.TransformSetters.ALL) {
            resMatrix = GameObject.Transform.Model;

            if (setters != GeometryHelpers.TransformSetters.ALL) {
                GeometryHelpers.Recompose(ref resMatrix, GameObject.Transform, setters);
            }
        }

        public virtual void GetFinalViewMatrix(ref Matrix4 modelView, out Matrix4 resMatrix) {
            GetModelMatrix(out resMatrix);

            Matrix4.Mult(ref resMatrix, ref modelView, out resMatrix);
        }

        public abstract void Render();
    }
}

