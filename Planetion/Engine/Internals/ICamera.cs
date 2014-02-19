using System.Drawing;
using Planetion.Utils;
using Planetion.Objects;
using OpenTK;
using Planetion.Engine.Utils;

namespace Planetion.Internals {
    public interface ICamera {

        Vector3 Position {
            get;
        }

        Frustum Frustum {
            get;
        }

        Matrix4 View {
            get;
        }

        Matrix4 Projection {
            get;
        }

        Vector3 Direction {
            get;
        }

        void Render();
    }
}
