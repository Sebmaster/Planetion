using System;
using OpenTK;
using Planetion;
using Planetion.Objects;
using Planetion.Utils;

#if PC
using System.Drawing;
#endif

namespace Planetion.Game.Objects {

    [RequireComponent(typeof(CameraComponent))]
    public class FPSCamera : Component {

        CameraComponent _camera;
        float _yaw;
        float _pitch;

        public override void Awake() {
#if PC
            Planetion.GameSurface.CurrentSurface.Input.SetMousePostition(Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width / 2, Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height / 2);
#endif

            _camera = GameObject.GetComponent<CameraComponent>();
            _yaw = 0;
            _pitch = 0;

            Planetion.GameSurface.CurrentSurface.Window.FocusedChanged += FocusedChanged;
            FocusedChanged(this, new EventArgs());
        }

        private bool Reposition = false;
        private void FocusedChanged(object sender, EventArgs e) {
#if PC
            Input inp = Planetion.GameSurface.CurrentSurface.Input;
            if (!Planetion.GameSurface.CurrentSurface.Window.Focused) {
                inp.ShowMouse();
                Reposition = false;
            } else if (!Reposition) {
                Rectangle rect = new Rectangle(Planetion.GameSurface.CurrentSurface.Window.PointToScreen(new Point(0, 0)), Planetion.GameSurface.CurrentSurface.Window.ClientSize);
                Point p = inp.GetRawMousePostition();
                if (!rect.Contains(p)) {
                    Reposition = true;
                    return;
                }
                inp.HideMouse();
                inp.SetMousePostition(Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width / 2, Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height / 2);
            }
#endif
        }

        public override void Update(double delta) {
            if (!Planetion.GameSurface.CurrentSurface.Window.Focused) return;

            Input inp = Planetion.GameSurface.CurrentSurface.Input;

            #region Mouse Movement

            if (Reposition) {
                Rectangle rect = new Rectangle(Planetion.GameSurface.CurrentSurface.Window.PointToScreen(new Point(0, 0)), Planetion.GameSurface.CurrentSurface.Window.ClientSize);
                Point p = inp.GetRawMousePostition();
                if (!rect.Contains(p)) {
                    return;
                }

                inp.HideMouse();
                inp.SetMousePostition(Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width / 2, Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height / 2);
                Reposition = false;
            }

            Point newPosition = inp.GetMousePostition();

            float deltaX = Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width / 2 - newPosition.X;
            float deltaY = newPosition.Y - Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height / 2;

            Vector3 translation = new Vector3();

            if ((Planetion.GameSurface.CurrentSurface.Window as GameWindow).Keyboard[OpenTK.Input.Key.A]) {
                translation += new Vector3(1, 0, 0);
            } else if ((Planetion.GameSurface.CurrentSurface.Window as GameWindow).Keyboard[OpenTK.Input.Key.D]) {
                translation += new Vector3(-1, 0, 0);
            }
            if ((Planetion.GameSurface.CurrentSurface.Window as GameWindow).Keyboard[OpenTK.Input.Key.W]) {
                translation += new Vector3(0, 1, 0);
            } else if ((Planetion.GameSurface.CurrentSurface.Window as GameWindow).Keyboard[OpenTK.Input.Key.S]) {
                translation += new Vector3(0, -1, 0);
            }

            _yaw += deltaX / 200f;
            _pitch += deltaY / 200f;

            Matrix4 rotation = CameraComponent.CreateFromYawPitchRoll(_yaw, -_pitch, 0);
            //Matrix4 rotation = Matrix4.CreateRotationX(0) * Matrix4.CreateRotationY(_yaw) * Matrix4.CreateRotationZ(_pitch);
            GameObject.Transform.LocalPosition += Vector3.Transform(translation, rotation) * (float)delta * 100;
            _camera.Direction = Vector3.Transform(new Vector3(0, 1, 0), rotation);

            inp.SetMousePostition(Planetion.GameSurface.CurrentSurface.Window.ClientSize.Width / 2, Planetion.GameSurface.CurrentSurface.Window.ClientSize.Height / 2);
            #endregion
        }
    }
}
