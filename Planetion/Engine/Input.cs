using System.Drawing;
using OpenTK.Platform;
using System;

namespace Planetion {
    public class Input {

        protected IGameWindow Window;

        internal Input(IGameWindow window) {
            this.Window = window;
        }

        public void HideMouse() {
#if PC
            System.Windows.Forms.Cursor.Hide();
#endif
        }

        public void ShowMouse() {
#if PC
            System.Windows.Forms.Cursor.Show();
#endif
        }

        public void SetMousePostition(int x, int y) {
#if PC
            System.Windows.Forms.Cursor.Position = Window.PointToScreen(new Point(x, y));
#endif
        }

        public Point GetMousePostition() {
#if PC
            return Window.PointToClient(System.Windows.Forms.Cursor.Position);
#endif
            throw new NotImplementedException();
        }

        public Point GetRawMousePostition() {
#if PC
            return System.Windows.Forms.Cursor.Position;
#endif
            throw new NotImplementedException();
        }
    }
}
