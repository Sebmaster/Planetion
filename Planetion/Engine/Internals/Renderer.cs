using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Objects;
using OpenTK;

namespace Planetion.Internals {
    interface Renderer : IDisposable {

        void Render(IEnumerable<GameObject> Objects, IEnumerable<Light> Lights, ref Matrix4 projection, ref Matrix4 view, ICamera camera);
    }
}
