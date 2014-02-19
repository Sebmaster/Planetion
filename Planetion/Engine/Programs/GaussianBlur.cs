using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Internals;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Planetion.Engine.Programs {
    public class GaussianBlur {
        private ShaderProgram _blurShader;


        private uint _TexLoc;
        public uint TextureLocation {
            get {
                return _TexLoc;
            }
            set {
                _blurShader.Use();
                GL.Uniform1(_TextureLocation, value);
                _TexLoc = value;
            }
        }

        private Vector2 _Scale;
        public Vector2 Scale {
            get {
                return _Scale;
            }
            set {
                _blurShader.Use();
                GL.Uniform2(_ScaleLocation, value);
                _Scale = value;
            }
        }

        private int _TextureLocation;
        private int _ScaleLocation;

        public GaussianBlur() {
            _blurShader = ShaderProgram.Load("Programs/GaussianBlur.fx");
            _ScaleLocation = GL.GetUniformLocation(_blurShader.ID, "uScale");
            _TextureLocation = GL.GetUniformLocation(_blurShader.ID, "uTexture");
        }

        public void Use() {
            _blurShader.Use();
        }
    }
}
