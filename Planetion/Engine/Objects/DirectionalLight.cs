using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Internals;
using OpenTK;
using OpenTK.Graphics;

#if PC
using OpenTK.Graphics.OpenGL;
using Planetion.Engine.Programs;
using Planetion.Objects;
#endif

#if ANDROID
using OpenTK.Graphics.ES20;
#endif
namespace Planetion.Engine.Objects {
    public class DirectionalLight : Light {

        private const float BLUR_COEF = 0.25f;
        private const int SHADOWMAPSIZE = 512;
        private const int SHADOWMAPS = 4;

        private bool IsDeferredIntialized {
            get;
            set;
        }

        private bool IsForwardIntialized {
            get;
            set;
        }

        private Vector3 Direction {
            get {
                return Vector3.Transform(Vector3.UnitZ, GameObject.Transform.Rotation);
            }
        }

        public Color4 Color {
            get;
            set;
        }

        public bool CastShadows {
            get;
            set;
        }
        public int ShadowSplits {
            get;
            set;
        }

        private float _intensity = 1f;
        public float Intensity {
            get {
                return _intensity;
            }
            set {
                _intensity = Math.Min(Math.Max(0, value), 1);
            }
        }

        public DirectionalLight() {
            //Direction = new Vector3(1, -1, 1);
            Color = Color4.White;
            CastShadows = true;
            ShadowSplits = 4;
        }

        public override void Awake() {
            GameObject.World.Lights.Add(this);
        }

        #region Forward Rendering

        public override void ForwardRender() {
            throw new NotImplementedException();
        }

        #endregion

        #region Deferred Rendering
        private ShaderProgram defProgram;

        readonly private static float[] defVertexTextureBuffer = new float[] { 
            1, 1, 1, 1,
            0, 1, -1,1,
            1, 0, 1, -1,
            0, 0, -1,-1,
        };

        private int defGBufferLocation;
        private int defNormalLocation;
        private int defPositionLocation;
        private int defModelViewLocation;
        private int defLightDirectionLocation;
        private int defLightColorLocation;
        private int defLightIntensityLocation;
        private int defBuf;
        private int defDoShadowMapping;
        private int defShadow;
        private int defShadowMaps;
        private int defShadowMats;

        private StoreDepth shadowShader;

        private uint shadowFB;
        private uint shadowTex;

        private void InitializeDeferred() {
            defProgram = ShaderProgram.Load("Programs/DefDirectionalLight.fx");
            defGBufferLocation = GL.GetUniformLocation(defProgram.ID, "uColor");
            defNormalLocation = GL.GetUniformLocation(defProgram.ID, "uNormalsSpecular");
            defPositionLocation = GL.GetUniformLocation(defProgram.ID, "uPosition");

            defModelViewLocation = GL.GetUniformLocation(defProgram.ID, "uModelView");
            defLightDirectionLocation = GL.GetUniformLocation(defProgram.ID, "uLightDirection");
            defLightColorLocation = GL.GetUniformLocation(defProgram.ID, "uLightColor");
            defLightIntensityLocation = GL.GetUniformLocation(defProgram.ID, "uLightIntensity");
            defDoShadowMapping = GL.GetUniformLocation(defProgram.ID, "uDoShadowMapping");
            defShadow = GL.GetUniformLocation(defProgram.ID, "uShadowMap");

            defShadowMaps = GL.GetUniformLocation(defProgram.ID, "uShadowMaps");
            defShadowMats = GL.GetUniformLocation(defProgram.ID, "uShadowMats");

            GL.GenBuffers(1, out defBuf);
            GL.BindBuffer(BufferTarget.ArrayBuffer, defBuf);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(16 * sizeof(float)), defVertexTextureBuffer, BufferUsageHint.StaticDraw);

            if (CastShadows) {
                shadowShader = new StoreDepth();

                GL.GenTextures(1, out shadowTex);
                GL.BindTexture(TextureTarget.Texture2D, shadowTex);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, SHADOWMAPSIZE * SHADOWMAPS, SHADOWMAPSIZE, 0, PixelFormat.DepthComponent, PixelType.UnsignedByte, IntPtr.Zero);

                GL.GenFramebuffers(1, out shadowFB);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFB);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, shadowTex, 0);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            IsDeferredIntialized = true;
        }

        public override void DeferredRender(ref Matrix4 view, ref Matrix4 projection, uint gBuffer, uint normalBuffer, uint positionBuffer, uint depthBuffer, uint frameBuffer, IEnumerable<GameObject> Objects, ICamera camera) {
            if (!IsDeferredIntialized) InitializeDeferred();

            Matrix4[] shadowMats = new Matrix4[SHADOWMAPS];
            if (CastShadows) {
                Matrix4 shadowMat = Matrix4.Identity;
                Matrix4 shadowMat2 = Matrix4.Identity;
                Matrix4 bias = new Matrix4() {
                    M11 = 0.5f,
                    M22 = 0.5f,
                    M33 = 0.5f,
                    M41 = 0.5f,
                    M42 = 0.5f,
                    M43 = 0.5f,
                    M44 = 1
                };

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFB);
                shadowShader.Use();
                GL.DrawBuffers(0, new DrawBuffersEnum[] { });
                Vector3 pos = camera.Position;
                int width = GameSurface.CurrentSurface.Window.ClientRectangle.Width;
                int height = GameSurface.CurrentSurface.Window.ClientRectangle.Height;

                float N = SHADOWMAPS;
                float near = camera.Frustum.Near, far = camera.Frustum.Far;
                float[] splitDepths = new float[SHADOWMAPS + 1];
                splitDepths[0] = near;
                splitDepths[SHADOWMAPS] = far;
                const float splitConstant = 0.95f;
                for (int i = 1; i < splitDepths.Length - 1; i++)
                    splitDepths[i] = splitConstant * near * (float)Math.Pow(far / near, i / N) + (1.0f - splitConstant) * ((near + (i / N)) * (far - near));

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.CullFace(CullFaceMode.Front);

                for (int i = 0; i < SHADOWMAPS; i++) {
                    Matrix4 shadowProj, shadowView;
                    float minZ = splitDepths[i];
                    float maxZ = splitDepths[i + 1];
                    CalculateFrustum(camera, minZ, maxZ, out shadowProj, out shadowView);

                    Matrix4.Mult(ref shadowProj, ref bias, out shadowMats[i]);
                    Matrix4.Mult(ref shadowView, ref shadowMats[i], out shadowMats[i]);
                    RenderShadowMap(shadowView, shadowProj, Objects, i);
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
                GL.CullFace(CullFaceMode.Back);
                GL.Viewport(0, 0, width, height);
            }
            defProgram.Use();
            GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment2 });

            GL.UniformMatrix4(defModelViewLocation, false, ref view);
            GL.Uniform3(defLightDirectionLocation, Direction);
            GL.Uniform4(defLightColorLocation, Color);
            GL.Uniform1(defLightIntensityLocation, _intensity);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gBuffer);
            GL.Uniform1(defGBufferLocation, 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normalBuffer);
            GL.Uniform1(defNormalLocation, 1);

            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, positionBuffer);
            GL.Uniform1(defPositionLocation, 3);

            if (CastShadows) {
                GL.Uniform1(defDoShadowMapping, 1);
                float[] matrizen = new float[16 * SHADOWMAPS];
                for (int i = 0; i < SHADOWMAPS; i++) {
                    int j = i * 16;
                    Matrix4 m = shadowMats[i];

                    matrizen[j + 0] = m.Row0.X;
                    matrizen[j + 1] = m.Row0.Y;
                    matrizen[j + 2] = m.Row0.Z;
                    matrizen[j + 3] = m.Row0.W;

                    matrizen[j + 4] = m.Row1.X;
                    matrizen[j + 5] = m.Row1.Y;
                    matrizen[j + 6] = m.Row1.Z;
                    matrizen[j + 7] = m.Row1.W;

                    matrizen[j + 8] = m.Row2.X;
                    matrizen[j + 9] = m.Row2.Y;
                    matrizen[j + 10] = m.Row2.Z;
                    matrizen[j + 11] = m.Row2.W;

                    matrizen[j + 12] = m.Row3.X;
                    matrizen[j + 13] = m.Row3.Y;
                    matrizen[j + 14] = m.Row3.Z;
                    matrizen[j + 15] = m.Row3.W;
                }

                GL.UniformMatrix4(defShadowMats, SHADOWMAPS, false, matrizen);
                GL.Uniform1(defShadowMaps, SHADOWMAPS);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, shadowTex);
                GL.Uniform1(defShadow, 4);
            } else {
                GL.Uniform1(defDoShadowMapping, 0);
            }

            GL.DepthMask(false);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, defBuf);
            GL.VertexAttribPointer(GL.GetAttribLocation(defProgram.ID, "inTexCoord"), 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), IntPtr.Zero);
            GL.VertexAttribPointer(GL.GetAttribLocation(defProgram.ID, "inVertex"), 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), new IntPtr(2 * sizeof(float)));

            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
            GL.DepthMask(true);
            GL.DisableVertexAttribArray(1);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        private void CalculateFrustum(ICamera camera, float minZ, float maxZ, out Matrix4 shadowProj, out Matrix4 shadowView) {
            Vector3[] frustumCornersWS = camera.Frustum.Points;
            Vector3[] frustumCornersVS = new Vector3[8];
            Vector3[] farFrustumCornersVS = new Vector3[4];
            Vector3[] splitFrustumCornersVS = new Vector3[8];
            Vector3[] frustumCornersLS = new Vector3[8];
            Matrix4 cameraView = camera.View;
            Matrix4 cameraMatrix = Matrix4.Invert(camera.View);
            for (int i = 0; i < 8; i++) {
                Vector3.Transform(ref frustumCornersWS[i], ref cameraView, out frustumCornersVS[i]);
            }

            for (int i = 0; i < 4; i++)
                farFrustumCornersVS[i] = frustumCornersVS[i + 4];

            for (int i = 0; i < 4; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i + 4] * (minZ / camera.Frustum.Far);

            for (int i = 4; i < 8; i++)
                splitFrustumCornersVS[i] = frustumCornersVS[i] * (maxZ / camera.Frustum.Far);

            for (int i = 0; i < 8; i++)
                Vector3.Transform(ref splitFrustumCornersVS[i], ref cameraMatrix, out frustumCornersWS[i]);

            Vector3 frustumCentroid = new Vector3(0, 0, 0);
            for (int i = 0; i < 8; i++)
                frustumCentroid += frustumCornersWS[i];
            frustumCentroid /= 8;

            //GameObject.Transform.Position = frustumCentroid;
            float distFromCentroid = Math.Max((maxZ - minZ), (splitFrustumCornersVS[4] - splitFrustumCornersVS[5]).Length) + 50.0f;
            distFromCentroid = (float)Math.Floor(distFromCentroid);
            frustumCentroid.X = (float)Math.Floor(frustumCentroid.X);
            frustumCentroid.Y = (float)Math.Floor(frustumCentroid.Y);
            frustumCentroid.Z = (float)Math.Floor(frustumCentroid.Z);
            shadowView = Matrix4.LookAt(frustumCentroid - (Direction * distFromCentroid), frustumCentroid, Vector3.UnitY);
            for (int i = 0; i < 8; i++)
                Vector3.Transform(ref frustumCornersWS[i], ref shadowView, out frustumCornersLS[i]);

            Vector3 mins = frustumCornersLS[0];
            Vector3 maxes = frustumCornersLS[0];
            for (int i = 0; i < 8; i++) {
                if (frustumCornersLS[i].X > maxes.X)
                    maxes.X = frustumCornersLS[i].X;
                else if (frustumCornersLS[i].X < mins.X)
                    mins.X = frustumCornersLS[i].X;
                if (frustumCornersLS[i].Y > maxes.Y)
                    maxes.Y = frustumCornersLS[i].Y;
                else if (frustumCornersLS[i].Y < mins.Y)
                    mins.Y = frustumCornersLS[i].Y;
                if (frustumCornersLS[i].Z > maxes.Z)
                    maxes.Z = frustumCornersLS[i].Z;
                else if (frustumCornersLS[i].Z < mins.Z)
                    mins.Z = frustumCornersLS[i].Z;
            }

            float radius = Math.Max((frustumCentroid - mins).Length, (frustumCentroid - maxes).Length);
            radius = (float)Math.Floor(radius);
            shadowProj = Matrix4.CreateOrthographic(radius, radius, 0, distFromCentroid + radius * 2);
        }

        private void RenderShadowMap(Matrix4 view, Matrix4 projection, IEnumerable<GameObject> Objects, int part) {
            GL.Viewport(SHADOWMAPSIZE * part, 0, SHADOWMAPSIZE, SHADOWMAPSIZE);
            shadowShader.Use();
            shadowShader.Projection = projection;

            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1, 4096);

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
                    rc.GetFinalViewMatrix(ref view, out mat);

                    shadowShader.ModelView = mat;

                    rc.Render();
                    lastObj = rc;
                }
            }
            if (lastObj != null) {
                lastObj.EndRenderGroup();
            }
            GL.Disable(EnableCap.PolygonOffsetFill);
        }

        #endregion
    }
}
