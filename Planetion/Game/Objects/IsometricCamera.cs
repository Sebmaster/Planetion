using OpenTK.Input;
using OpenTK.Platform;
using Planetion.Objects;
using Planetion.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Planetion.Game.Objects {
    [RequireComponent(typeof(CameraComponent))]
   public class IsometricCamera : Component {

#if PC // WINDOWS
        [DllImport("User32.dll")]
        private extern static IntPtr SetCapture(IntPtr hWnd);

        [DllImport("User32.dll")]
        private extern static bool ReleaseCapture();
#endif

        private const float CAM_SPEED = 10;

        private CameraComponent _camera;

        public override void Awake() {

            _camera = GameObject.GetComponent<CameraComponent>();

            Planetion.GameSurface.CurrentSurface.Window.FocusedChanged += FocusedChanged;
            FocusedChanged(this, new EventArgs());

            _camera.Direction = new OpenTK.Vector3(0, -0.5f, -0.5f);
        }

        public override void Update(double delta) {
            int maxX = Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width;
            int maxY = Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height;
            Point p = Planetion.GameSurface.CurrentSurface.Input.GetMousePostition();

            if (Planetion.GameSurface.CurrentSurface.Window.Focused) {
                int x = Math.Max(Math.Min(p.X, maxX), 0);
                int y = Math.Max(Math.Min(p.Y, maxY), 0);
                Planetion.GameSurface.CurrentSurface.Input.SetMousePostition(x, y);

                if (x == 0) {
                    _camera.GameObject.Transform.Position = new OpenTK.Vector3(_camera.Position.X - CAM_SPEED * (float)delta, _camera.Position.Y, _camera.Position.Z);
                } else if (x == maxX) {
                    _camera.GameObject.Transform.Position = new OpenTK.Vector3(_camera.Position.X + CAM_SPEED * (float)delta, _camera.Position.Y, _camera.Position.Z);
                }

                if (y == 0) {
                    _camera.GameObject.Transform.Position = new OpenTK.Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z - CAM_SPEED * (float)delta);
                } else if (y == maxY) {
                    _camera.GameObject.Transform.Position = new OpenTK.Vector3(_camera.Position.X, _camera.Position.Y, _camera.Position.Z + CAM_SPEED * (float)delta);
                }
            }
        }

        private void FocusedChanged(object o, EventArgs eventArgs) {
            IGameWindow win = Planetion.GameSurface.CurrentSurface.Window;
            /*if (win.Focused) {
                GameSurface.CurrentSurface.Input.HideMouse();
            } else {
                GameSurface.CurrentSurface.Input.ShowMouse();
            }*/

#if PC
            MouseState state = OpenTK.Input.Mouse.GetState(0);
            IWindowInfo ii = ((OpenTK.NativeWindow)win).WindowInfo;
            PropertyInfo pi = (ii.GetType()).GetProperty("WindowHandle");
            IntPtr hnd = ((IntPtr)pi.GetValue(ii, null));

            if (win.Focused) {
                SetCapture(hnd);
            } else {
                ReleaseCapture();
            }
#endif
        }
    }
}
