using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using Planetion.Utils;

#if ANDROID
using OpenTK.Graphics.ES20;
using Android.Graphics;
#endif
#if PC
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
#endif

namespace Planetion.Objects {
    public class BillboardComponent : RenderableComponent {

        [Flags]
        public enum RotationModes : byte {
            NONE = 0,
            VERTICAL_ROTATION = 1,
            HORIZONTAL_ROTATION = 2,
            FACE_CAMERA = 3
        }

        public enum WrapModes : byte {
            REPEAT,
            CLAMP
        }

        internal override object Identifier {
            get {
                return File + (byte)WrapMode + (byte)RotationMode;
            }
        }

        public string File { get; private set; }
        public RotationModes RotationMode = RotationModes.FACE_CAMERA;
        public WrapModes WrapMode = WrapModes.REPEAT;
        private Int32 TexId;

        private static uint buf;
        private static Dictionary<string, WeakReference> textures = new Dictionary<string, WeakReference>(); //FIXME: Dispose of unreferenced textures

        public void Create(string file) {
            File = file;

            WeakReference weakTex;
            textures.TryGetValue(File, out weakTex);
            if (weakTex == null) {
                TexId = GL.GenTexture();

                Bitmap DiffuseMap = (Bitmap)Bitmap.FromFile(File);

                GL.BindTexture(TextureTarget.Texture2D, TexId);
                BitmapData diffuseMapData = DiffuseMap.LockBits(new Rectangle(0, 0, DiffuseMap.Width, DiffuseMap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, diffuseMapData.Width, diffuseMapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, diffuseMapData.Scan0);
                DiffuseMap.UnlockBits(diffuseMapData);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                if (WrapMode == WrapModes.CLAMP) {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
                }

                textures.Add(File, new WeakReference(TexId));
            } else {
                TexId = (Int32)weakTex.Target;
            }

            if (buf == 0) {
                GL.GenBuffers(1, out buf);
                GL.BindBuffer(BufferTarget.ArrayBuffer, buf);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(28 * sizeof(float)),
                             new float[] { 0.5f, 0.5f, 0, 0, 1, 1, 1, 
                                           -0.5f, 0.5f, 0, 0, 1, 0, 1, 
                                           0.5f, -0.5f, 0, 0, 1, 1, 0, 
                                           -0.5f, -0.5f, 0, 0, 1, 0, 0}, BufferUsageHint.StaticDraw);
            }
        }

        public override void BeginRenderGroup() {
            GL.BindTexture(TextureTarget.Texture2D, TexId);

            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buf);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 7 * sizeof(float), IntPtr.Zero );
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 2 * sizeof(float));
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 7 * sizeof(float), 5 * sizeof(float));

            if ((RotationMode & RotationModes.FACE_CAMERA) == 0) {
                GL.Disable(EnableCap.CullFace);
            }
        }

        public override void Render() {
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
        }

        public override void EndRenderGroup() {
            if ((RotationMode & RotationModes.FACE_CAMERA) != RotationModes.FACE_CAMERA) {
                GL.Enable(EnableCap.CullFace);
            }

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);

            base.EndRenderGroup();
        }

        public override void GetFinalViewMatrix(ref Matrix4 modelView, out Matrix4 resMatrix) {
            if (RotationMode != RotationModes.NONE) {
                GetModelMatrix(out resMatrix, GeometryHelpers.TransformSetters.TRANSLATE | GeometryHelpers.TransformSetters.ROTATE);
                Matrix4.Mult(ref resMatrix, ref modelView, out resMatrix);

                if ((RotationMode & RotationModes.HORIZONTAL_ROTATION) == RotationModes.HORIZONTAL_ROTATION) {
                    resMatrix.M11 = 1;
                    resMatrix.M12 = 0;
                    resMatrix.M13 = 0;
                }
                if ((RotationMode & RotationModes.VERTICAL_ROTATION) == RotationModes.VERTICAL_ROTATION) {
                    resMatrix.M21 = 0;
                    resMatrix.M22 = 1;
                    resMatrix.M23 = 0;
                }
                if ((RotationMode & RotationModes.HORIZONTAL_ROTATION) == RotationModes.HORIZONTAL_ROTATION) {
                    resMatrix.M31 = 0;
                    resMatrix.M32 = 0;
                    resMatrix.M33 = 1;
                }

                Matrix4 tempMat;
                GetModelMatrix(out tempMat, GeometryHelpers.TransformSetters.SCALE);
                Matrix4.Mult(ref tempMat, ref resMatrix, out resMatrix);
            } else {
                base.GetFinalViewMatrix(ref modelView, out resMatrix);
            }
        }
    }
}

