using Planetion.Objects;
using Planetion.Internals;

namespace Planetion.Game.Objects {

    public class BoxMesh : Mesh {

        public BoxMesh() {
            Vertices = new float[] {
				-0.5f,-0.5f,-0.5f,
				0.5f,-0.5f,-0.5f,
				-0.5f,0.5f,-0.5f,
				0.5f,0.5f,-0.5f,
				-0.5f,-0.5f,0.5f,
				0.5f,-0.5f,0.5f,
				-0.5f,0.5f,0.5f,
				0.5f,0.5f,0.5f};
            Indices = new ushort[] {
				2,1,0, // Front
				1,2,3, // Front
				2,6,3, // Top
				3,6,7, // Top
				7,1,3, // Left
				7,5,1, // Left
				5,4,0, // Down
				1,5,0, // Down
				4,6,0, // Right
				0,6,2, // Right
				6,5,7, // Back
				4,5,6 // Back
			};

            Create();
        }
    }
}

