using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Internals;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Planetion.Engine.Programs {
    class StoreDepth {
        private ShaderProgram _StoreDepthShader;


        private Matrix4 _ModelView;
        public Matrix4 ModelView {
            get {
                return _ModelView;
            }
            set {
                _StoreDepthShader.Use();
                GL.UniformMatrix4(_ModelViewLocation,false,  ref value);
                _ModelView = value;
            }
        }

        private Matrix4 _Projection;
        public Matrix4 Projection {
            get {
                return _Projection;
            }
            set {
                _StoreDepthShader.Use();
                GL.UniformMatrix4(_ProjectionLocation,false, ref value);
                _Projection = value;
            }
        }

        private int _ModelViewLocation;
        private int _ProjectionLocation;

        public StoreDepth() {
            _StoreDepthShader = ShaderProgram.Load("Programs/StoreDepth.fx");
            _ProjectionLocation = GL.GetUniformLocation(_StoreDepthShader.ID, "projection");
            _ModelViewLocation = GL.GetUniformLocation(_StoreDepthShader.ID, "modelView");
        }

        public void Use() {
            _StoreDepthShader.Use();
        }

    }
}
