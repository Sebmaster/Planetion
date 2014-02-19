using System;
#if PC
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
#endif

#if ANDROID
using Android.Graphics;
using OpenTK.Graphics.ES20;
#endif

namespace Planetion.Internals {
    public class Mesh : IDisposable {

        public ushort[] Indices;
        public float[] Vertices;
        public float[] Normals;
        public float[] TexCoords;
        public Bitmap DiffuseMap;

        protected int texId;
        protected int[] bufs = new int[4];

        public Mesh() { }

        public void Create() {
#if PC
            if (DiffuseMap != null) {
                texId = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, texId);
                BitmapData diffuseMapData = DiffuseMap.LockBits(new Rectangle(0, 0, DiffuseMap.Width, DiffuseMap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, diffuseMapData.Width, diffuseMapData.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, diffuseMapData.Scan0);
                DiffuseMap.UnlockBits(diffuseMapData);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
#endif

            GL.GenBuffers(4, bufs);
            GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * sizeof(float)), Vertices, BufferUsageHint.StaticDraw);

            if (Normals != null) {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[1]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Normals.Length * sizeof(float)), Normals, BufferUsageHint.StaticDraw);
            }
            if (TexCoords != null) {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[2]);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TexCoords.Length * sizeof(float)), TexCoords, BufferUsageHint.StaticDraw);
            }
            if (Indices != null) {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufs[3]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(ushort)), Indices, BufferUsageHint.StaticDraw);
            }
        }

        public void StartGroup() {
            if (DiffuseMap != null) {
                GL.BindTexture(TextureTarget.Texture2D, texId);
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);

            if (Normals != null) {
                GL.EnableVertexAttribArray(1);

                GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[1]);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);
            }

            if (TexCoords != null) {
                GL.EnableVertexAttribArray(2);

                GL.BindBuffer(BufferTarget.ArrayBuffer, bufs[2]);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), IntPtr.Zero);
            }

            if (Indices != null) {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufs[3]);
            }
        }

        internal void Render() {
            if (Indices != null) {
                GL.DrawElements(BeginMode.Triangles, Indices.Length, DrawElementsType.UnsignedShort, 0);
            } else {
                GL.DrawArrays(BeginMode.Triangles, 0, Vertices.Length / 3);
            }
        }

        internal void EndGroup() {
            if (Normals != null) {
                GL.DisableVertexAttribArray(1);
            }

            if (TexCoords != null) {
                GL.DisableVertexAttribArray(2);
            }
        }

        public void Dispose() {
            GL.DeleteBuffers(4, bufs);

            if (DiffuseMap != null) {
                GL.DeleteTexture(texId);
            }
        }
    }
}