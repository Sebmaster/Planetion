using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Planetion.Objects;

namespace Demo {
    class TestComponent2 : Component {
        public override void Update(double delta) {
            //GameObject.Transform.LocalPosition += new OpenTK.Vector3((float)delta*5, (float)delta*10, (float)delta*15);
        }
    }
}
