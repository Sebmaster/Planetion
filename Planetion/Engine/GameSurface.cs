using System;
using OpenTK;
using OpenTK.Platform;
using Planetion.Internals;
using Planetion.Objects;

#if PC
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
#endif

#if ANDROID
using OpenTK.Graphics.ES20;
#endif

namespace Planetion {

    public delegate void OnLoadDelegate();

    public class GameSurface {

        public event OnLoadDelegate Load;

        public static Random Random = new Random();
        public World World = new World();
        public Input Input;
        public IGameWindow Window;

        public static GameSurface CurrentSurface;

        public GameSurface(int width, int height) :this( new OpenTK.GameWindow(width, height, new GraphicsMode(new ColorFormat(32), 24, 0, 0), "Testgame", GameWindowFlags.Default, DisplayDevice.GetDisplay(DisplayIndex.Default), 3, 2,
#if DEBUG
 GraphicsContextFlags.Debug
#else
                GraphicsContextFlags.ForwardCompatible
#endif
)) {
        }

        public GameSurface(IGameWindow window) {
            CurrentSurface = this;
            Window = window;
            Input = new Input(Window);

            Window.Load += OnLoad;
            Window.UpdateFrame += OnUpdateFrame;
            Window.RenderFrame += OnRenderFrame;
        }

        public void Start() {
            this.Window.Run();
        }

        #region Event overrides
        private void OnLoad(object sender, EventArgs e) {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less);

            int arr;
            GL.GenVertexArrays(1, out arr);
            GL.BindVertexArray(arr);

            if (Load != null) {
                Load();
            }
        }

        private void OnUpdateFrame(object sender, FrameEventArgs e) {
            CurrentSurface = this;

            World.Update(e.Time);
        }

        private void OnRenderFrame(object sender, FrameEventArgs e) {
            CurrentSurface = this;

            GL.Clear(ClearBufferMask.DepthBufferBit);

            foreach (GameObject go in World.Objects) {
                foreach (Component c in go.Components.Values) {
                    if (c is ICamera) {
                        ((ICamera)c).Render();
                    }
                }
            }

#if DEBUG
            ErrorCode code = GL.GetError();
            if (code != ErrorCode.NoError) {
                throw new Exception("Render error: " + code);
            }
#endif

            Window.SwapBuffers();
        }
        #endregion
    }
}

