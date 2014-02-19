using System.Collections.Generic;
using Planetion.Objects;
using OpenTK;

#if PC
using OpenTK.Graphics.OpenGL;
#endif

#if ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Planetion.Internals {
    class ForwardRenderer : Renderer {

        private ShaderProgram Program = ShaderProgram.Load("Programs/TexturedMesh.fx");
        private int projectionLocation;
        private int modelViewLocation;
        private int DiffuseMapLocation;

        public ForwardRenderer(int width, int height) {
            GL.Viewport(0, 0, width, height);

            GL.BindAttribLocation(Program.ID, 0, "inVertex");
            GL.BindAttribLocation(Program.ID, 1, "inNormal");
            GL.BindAttribLocation(Program.ID, 2, "inTexCoord");
            Program.Relink();

            projectionLocation = GL.GetUniformLocation(Program.ID, "projection");
            modelViewLocation = GL.GetUniformLocation(Program.ID, "modelView");
            DiffuseMapLocation = GL.GetUniformLocation(Program.ID, "DiffuseMap");

            GL.EnableVertexAttribArray(0);
        }

        public void Render(IEnumerable<GameObject> Objects, IEnumerable<Light> Lights, ref Matrix4 projection, ref Matrix4 modelView, ICamera camera) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Program.Use();

            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.Uniform1(DiffuseMapLocation, 0);

            RenderableComponent lastObj = null;
            foreach (GameObject obj in Objects) {
                foreach (Component c in obj.Components.Values) {
                    RenderableComponent rc = c as RenderableComponent;
                    if (rc == null)
                        continue;

                    if (lastObj == null) {
                        rc.BeginRenderGroup();
                    } else if (rc.Identifier == null || rc.Identifier != lastObj.Identifier) {
                        lastObj.EndRenderGroup();
                        rc.BeginRenderGroup();
                    }

                    Matrix4 mat;
                    rc.GetFinalViewMatrix(ref modelView, out mat);

                    GL.UniformMatrix4(modelViewLocation, false, ref mat);

                    rc.Render();

                    lastObj = rc;
                }
            }

            if (lastObj != null) {
                lastObj.EndRenderGroup();
            }
        }

        public void Dispose() {
            Program.Dispose();
        }
    }
}
