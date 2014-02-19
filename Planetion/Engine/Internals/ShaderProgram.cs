using System;
using System.IO;

#if PC
using OpenTK.Graphics.OpenGL;
#endif

#if ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Planetion.Internals {
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

#if DEBUG
            int compiled;
            GL.GetProgram(ID, ProgramParameter.LinkStatus, out compiled);
            if (compiled == 0) {
                throw new Exception("Shader compilation error!\r\n" + GL.GetProgramInfoLog(ID) + "\r\n" +
                                    GL.GetShaderInfoLog(vertexID) + "\r\n" + GL.GetShaderInfoLog(fragmentID));
            }
#endif
        }

        public void Relink() {
            if (disposed) throw new ObjectDisposedException("Shader");

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

            using (StreamReader rd = new StreamReader(file)) {
                string[] splitted = rd.ReadToEnd().Split(new string[] { "--------------------------------------------------------------------------------" }, StringSplitOptions.None);

                vertexShader = splitted[0];
                fragmentShader = splitted[1];
            }

            return new ShaderProgram(vertexShader, fragmentShader);
        }
    }
}
