#if PC
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Planetion.Objects;

namespace Planetion.Internals {
    internal class DeferredRenderer : Renderer {

        private bool disposed = false;
        private ShaderProgram GBuffer = ShaderProgram.Load("Programs/GBuffer.fx");
        private int projectionLocation;
        private int modelLocation;
        private int modelViewLocation;
        private int DiffuseMapLocation;

        private uint framebuffer;
        private int depth;
        private int lightMap;
        private uint[] textures = new uint[3];

        protected FullScreenTexture fst = new FullScreenTexture();

        public DeferredRenderer(int width, int height) {
            GL.BindAttribLocation(GBuffer.ID, 0, "inVertex");
            GL.BindAttribLocation(GBuffer.ID, 1, "inNormal");
            GL.BindAttribLocation(GBuffer.ID, 2, "inTexCoord");

            GL.BindFragDataLocation(GBuffer.ID, 0, "color");
            GL.BindFragDataLocation(GBuffer.ID, 1, "normalsSpecular");
            GBuffer.Relink();

            projectionLocation = GL.GetUniformLocation(GBuffer.ID, "projection");
            modelViewLocation = GL.GetUniformLocation(GBuffer.ID, "modelView");
            modelLocation = GL.GetUniformLocation(GBuffer.ID, "model");

            DiffuseMapLocation = GL.GetUniformLocation(GBuffer.ID, "DiffuseMap");

            GL.GenFramebuffers(1, out framebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.Viewport(0, 0, width, height);

            GL.GenTextures(3, textures);

            GL.BindTexture(TextureTarget.Texture2D, textures[0]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, textures[0], 0);


            GL.BindTexture(TextureTarget.Texture2D, textures[1]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, textures[1], 0);

            GL.BindTexture(TextureTarget.Texture2D, textures[2]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, textures[2], 0);

            lightMap = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, lightMap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, lightMap, 0);
            /*
            GL.GenRenderbuffers(1, out renderbuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbuffer);
            */
            depth = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, depth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, width, height, 0, OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depth, 0);

#if DEBUG
            FramebufferErrorCode error = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (error != FramebufferErrorCode.FramebufferComplete) {
                throw new Exception("Framebuffer incomplete: " + error);
            }
#endif

            GL.EnableVertexAttribArray(0);
        }

        public void Render(IEnumerable<GameObject> Objects, IEnumerable<Light> Lights, ref Matrix4 projection, ref Matrix4 view, ICamera camera) {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.DrawBuffers(3, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment3 });

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GBuffer.Use();
            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.Uniform1(DiffuseMapLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture0);

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
                    Matrix4 modelMatrix;
                    rc.GetModelMatrix(out modelMatrix);
                    Matrix4.Mult(ref modelMatrix, ref view, out mat);

                    GL.UniformMatrix4(modelViewLocation, false, ref mat);
                    GL.UniformMatrix4(modelLocation, false, ref modelMatrix);

                    rc.Render();
                    lastObj = rc;
                }
            }
            if (lastObj != null) {
                lastObj.EndRenderGroup();
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment2 });
            GL.Clear(ClearBufferMask.ColorBufferBit);

            foreach (Light light in Lights) {
                light.DeferredRender(ref view, ref projection, textures[0], textures[1], textures[2], (uint)depth, framebuffer, Objects, camera);
            }
            GL.Disable(EnableCap.Blend);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back); // double buffered

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textures[0]);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, lightMap);

            fst.Draw();
        }

        public void Dispose() {
            if (disposed) return;

            GBuffer.Dispose();
            fst.Dispose();

            GL.DeleteTexture(depth);
            GL.DeleteTextures(2, textures);
            GL.DeleteFramebuffers(1, ref framebuffer);
            disposed = true;
        }
    }

    internal class FullScreenTexture : IDisposable {
        readonly private static float[] vertexTextureBuffer = new float[] { 
            1,1, 1, 1,
            0, 1, -1,1,
            1, 0, 1, -1,
            0, 0, -1,-1,
        };

        private ShaderProgram program = ShaderProgram.Load("Programs/FullScreenTexture.fx");

        private bool disposed = false;
        private int buf;
        private int textureLocation;
        private int lightMapLocation;

        private int texCoordLocation;
        private int vertexLocation;

        public FullScreenTexture() {
            GL.GenBuffers(1, out buf);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(16 * sizeof(float)), vertexTextureBuffer, BufferUsageHint.StaticDraw);

            textureLocation = GL.GetUniformLocation(program.ID, "uTexture");
            lightMapLocation = GL.GetUniformLocation(program.ID, "uLightMap");

            texCoordLocation = GL.GetAttribLocation(program.ID, "inTexCoord");
            vertexLocation = GL.GetAttribLocation(program.ID, "inVertex");
        }

        public void Draw() {
            if (disposed) throw new ObjectDisposedException("FullScreenTexture");

            program.Use();
            GL.Uniform1(textureLocation, 0);
            GL.Uniform1(lightMapLocation, 1);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buf);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.VertexAttribPointer(vertexLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DisableVertexAttribArray(1);
        }

        public void Dispose() {
            if (disposed) return;

            program.Dispose();
            GL.DeleteBuffers(1, ref buf);
            disposed = true;
        }
    }
}
#endif