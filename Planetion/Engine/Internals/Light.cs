using System;
using System.Collections.Generic;
using Planetion.Objects;
using OpenTK;

namespace Planetion.Internals {
    public abstract class Light : Component {

        public abstract void DeferredRender(ref Matrix4 modelView, ref Matrix4 projection, uint gBuffer, uint normalBuffer, uint positionBuffer, uint depthBuffer, uint frameBuffer, IEnumerable<GameObject> Object, ICamera camera);

        public abstract void ForwardRender();
    }
}
