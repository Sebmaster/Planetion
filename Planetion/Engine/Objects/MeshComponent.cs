using Planetion.Internals;

namespace Planetion.Objects {

	/// <summary>
	/// A mesh component represents a link between a game object and a mesh.
	/// Since components always link to a game object and we want to reuse meshes an additional layer to link between them is required.
	/// </summary>
	public class MeshComponent : RenderableComponent {

		public Mesh MeshObject { get; set; }

		internal override object Identifier {
			get {
				return MeshObject;
			}
        }

        public override void BeginRenderGroup() {
            MeshObject.StartGroup();
		}

        public override void EndRenderGroup() {
            MeshObject.EndGroup();
		}

		/// <summary>
		/// Delegates the rendering work to the mesh object.
		/// </summary>
		public override void Render() {
            MeshObject.Render();
		}
	}
}

