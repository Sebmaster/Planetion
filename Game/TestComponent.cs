using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Objects;
using OpenTK;
using Planetion.Engine.Objects;
using Planetion.Utils;

namespace Demo {

    [RequireComponent(typeof(DirectionalLight))]
    class TestComponent : Component {

        float angle = 0;
        public override void Update(double delta) {
            //Vector3 move = new Vector3((float)(rand.NextDouble() * 2 - 1), (float)(rand.NextDouble() * 2 - 1), (float)(rand.NextDouble() * 2 - 1));
            //GameObject.Transform.Rotation = new Quaternion(GameObject.Transform.Rotation.Xyz + move, 0);
            //GameObject.Transform.Rotate(Vector3.UnitX, (float)delta * 10);
            //GameObject.Transform.Rotate(Vector3.UnitY, (float)delta * 10);
            //GameObject.Transform.Rotate(Vector3.UnitZ, (float)delta * 10);
            //GameObject.Transform.Rotation += new Quaternion(Vector3.UnitY, f);
            DirectionalLight dl = GameObject.GetComponent<DirectionalLight>();

            //GameObject.Transform.LocalPosition = new Vector3((float)Math.Cos(angle) * 500, 100, (float)Math.Sin(angle) * 500);
            angle += 1f * (float)delta;
            //dl.Color = new OpenTK.Graphics.Color4((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble(), 1);
            //Vector3 direction = Vector3.Transform(dl.Direction, Matrix4.CreateRotationY(f * 0.1f));
            //GameObject.Transform.Rotation = Quaternion.FromAxisAngle(new Vector3(0.5f, 0.5f, 0), angle);

            
            //Vector3 position = Vector3.Transform(GameObject.Transform.LocalPosition, Matrix4.CreateRotationY(-f));
            //GameObject.Transform.Position = new Vector3(dl.Direction.X * -350, 100, dl.Direction.Z * -350);
        }
    }
}
