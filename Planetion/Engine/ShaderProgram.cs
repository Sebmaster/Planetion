using OpenTK.Graphics.OpenGL;
using System;
using System.IO;
using OpenTK;

namespace Planetion {
    public class ShaderProgram : IDisposable {

        public readonly string VertexShader;
        public readonly string FragmentShader;

        public readonly int ID;
        private int vertexID;
        private int fragmentID;

        private bool disposed = false;

        public ShaderProgram(string vertexShader, string fragmentShader) {
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;

            vertexID = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexID, VertexShader);
            GL.CompileShader(vertexID);

            fragmentID = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentID, FragmentShader);
            GL.CompileShader(fragmentID);

            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertexID);
            GL.AttachShader(ID, fragmentID);
            GL.LinkProgram(ID);
        }

        public void Use() {
            if (disposed) throw new ObjectDisposedException("Shader");

            GL.UseProgram(ID);
        }

        public void Dispose() {
            if (disposed) return;

            GL.DeleteProgram(ID);
            GL.DeleteShader(fragmentID);
            GL.DeleteShader(vertexID);

            disposed = true;
        }

        public static ShaderProgram Load(string file) {
            string vertexShader = "";
            string fragmentShader = "";

            bool nowFragment = false;
            string line;

            using (StreamReader rd = new StreamReader(file)) {
                while ((line = rd.ReadLine()) != null) {
                    if (line == "--------------------------------------------------------------------------------") {
                        nowFragment = true;
                    } else {
                        if (nowFragment) {
                            fragmentShader += line + '\n';
                        } else {
                            vertexShader += line + '\n';
                        }
                    }
                }
            }

            return new ShaderProgram(vertexShader, fragmentShader);
        }
    }
}
