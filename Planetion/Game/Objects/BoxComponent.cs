using Planetion.Objects;
using Planetion.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Planetion.Game.Objects {

    [RequireComponent(typeof(MeshComponent))]
    public class BoxComponent : Component{

        public  override void Awake() {
            this.GameObject.GetComponent<MeshComponent>().MeshObject = new BoxMesh();
        }
    }
}
