using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Drawing;
using System.Drawing.Imaging;
using Planetion.Utils;

namespace Planetion.Objects {

    [RequireComponent(typeof(RigidbodyComponent))]
    public class TerrainComponent : RenderableComponent, IDisposable {

        public Bitmap Texture;

        public int texId;

        public float[,] HeightMap = new float[0, 0];
        public bool[,] Disabled;

        private bool disposed = false;
        private int[] buffers;

        protected Vector3[] vertices;
        protected Vector3[] normals;
        protected Vector2[] texCoords;
        protected uint[] indices;

        internal override object Identifier {
            get {
                return HeightMap;
            }
        }

        public void Regenerate() {
            if (disposed) throw new ObjectDisposedException("Terrain");
            if (buffers != null) DeleteBuffers();

            CreateLists();
            RigidbodyComponent physics = this.GameObject.GetComponent<RigidbodyComponent>();
            physics.Offset = new Vector3(this.GameObject.Transform.Scale.X * this.HeightMap.GetLength(0) / 2, 0, this.GameObject.Transform.Scale.Z * this.HeightMap.GetLength(1) / 2);

            Vector3 offsetPos = this.GameObject.Transform.Position - physics.Offset;
            physics.Body = new Jitter.Dynamics.RigidBody(
                new Jitter.Collision.Shapes.TerrainShape(this.HeightMap, this.GameObject.Transform.Scale.X, this.GameObject.Transform.Scale.Z)
            ) {
                IsStatic = true,
                Position = new Jitter.LinearMath.JVector(offsetPos.X, offsetPos.Y, offsetPos.Z)
            };


            buffers = new int[4];
            GL.GenBuffers(4, buffers);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(3 * vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(3 * normals.Length * sizeof(float)), normals, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[2]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(2 * texCoords.Length * sizeof(float)), texCoords, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[3]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            texId = GL.GenTexture();

            if (this.Texture != null) {
                GL.BindTexture(TextureTarget.Texture2D, texId);
                BitmapData diffuseMapData = Texture.LockBits(new Rectangle(0, 0, Texture.Width, Texture.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, diffuseMapData.Width, diffuseMapData.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, diffuseMapData.Scan0);
                Texture.UnlockBits(diffuseMapData);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }
        }

        public override void BeginRenderGroup() {
            if (disposed) throw new ObjectDisposedException("Terrain");

            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[1]);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[2]);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[3]);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
        }

        public override void Render() {
            if (disposed) throw new ObjectDisposedException("Terrain");

            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public override void EndRenderGroup() {
            if (disposed) throw new ObjectDisposedException("Terrain");

            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(1);
        }

        protected void CreateLists() {
            if (disposed) throw new ObjectDisposedException("Terrain");

            uint width = (uint)HeightMap.GetLength(0);
            uint height = (uint)HeightMap.GetLength(1);
            uint indicCount = (width - 1) * (height - 1) * 6;
            uint vertiCount = width * height;
            vertices = new Vector3[vertiCount];
            normals = new Vector3[vertiCount];
            texCoords = new Vector2[vertiCount];
            Vector3 offsetToCenter = -new Vector3((width - 1) / 2f, 0, (height - 1) / 2f);

            for (int z = 0; z < height; z++) {
                for (int x = 0; x < width; x++) {
                    // Find position based on grid coordinates and height in
                    // heightmap
                    float the = HeightMap[x, z];
                    /*
                    int c = 1;
                    if (x % 2 == 0) {
                        if (x > 0) {
                            the += HeightMap[x - 1, z];
                            c++;
                        }
                        if (x < width - 1) {
                            the += HeightMap[x + 1, z];
                            c++;
                        }
                    }
                    if (z % 2 == 0) {
                        if (z > 0) {
                            the += HeightMap[x, z - 1];
                            c++;
                        }
                        if (z < width - 1) {
                            the += HeightMap[x, z + 1];
                            c++;
                        }
                    }*/
                    vertices[z * width + x] = new Vector3(x, the /*/ c*/, z) + offsetToCenter;
                    // UV coordinates range from (0, 0) at grid location (0, 0) to
                    // (1, 1) at grid location (width, length)
                    Vector2 uv = new Vector2((float)x / width, (float)z / height);

                    texCoords[z * width + x] = uv;
                }
            }

            uint i = 0;
            indices = new uint[indicCount];
            // For each cell
            for (uint z = 0; z < height - 1; z++) {
                for (uint x = 0; x < width - 1; x++) {
                    // Find the indices of the corners
                    uint upperLeft = z * width + x;
                    uint upperRight = upperLeft + 1;
                    uint lowerLeft = upperLeft + width;
                    uint lowerRight = lowerLeft + 1;

                    if (float.IsNaN(vertices[upperLeft].Y) || float.IsNaN(vertices[upperRight].Y) || float.IsNaN(vertices[lowerLeft].Y) || float.IsNaN(vertices[lowerRight].Y))
                        continue;
                    if (Disabled != null && Disabled[z, x])
                        continue;
                    // Specify upper triangle
                    indices[i++] = upperLeft;
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    // Specify lower triangle
                    indices[i++] = lowerLeft;
                    indices[i++] = lowerRight;
                    indices[i++] = upperRight;
                }
            }
            // For each triangle
            for (i = 0; i < indicCount; i += 3) {
                // Find the position of each corner of the triangle
                Vector3 v1 = vertices[indices[i]];
                Vector3 v2 = vertices[indices[i + 1]];
                Vector3 v3 = vertices[indices[i + 2]];
                // Cross the vectors between the corners to get the normal
                Vector3 normal = Vector3.Cross(v2 - v1,v3 - v1);
                normal.Normalize();
                //normal.Normalize();
                // Add the influence of the normal to each vertex in the
                // triangle
                Vector3.Add(ref normals[indices[i]], ref normal, out normals[indices[i]]);
                Vector3.Add(ref normals[indices[i + 1]], ref normal, out normals[indices[i + 1]]);
                Vector3.Add(ref normals[indices[i + 2]], ref normal, out normals[indices[i + 2]]);
                //normals[indices[i]] = normal;
                //normals[indices[i + 1]] = normal;
                //normals[indices[i + 2]] = normal;
            }
            // Average the influences of the triangles touching each
            // vertex
            for (i = 0; i < vertiCount; i++) {
                normals[i].Normalize();
            }
        }

        private void DeleteBuffers() {
            GL.DeleteBuffers(3, buffers);
            buffers = null;
        }

        public void Dispose() {
            if (disposed) return;

            if (buffers != null) {
                DeleteBuffers();
            }
            disposed = true;
        }
    }
}
