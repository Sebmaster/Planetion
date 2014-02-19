
namespace Planetion.Objects {
    public abstract class Component {

        public GameObject GameObject { get; internal set; }

        public virtual void Awake() { }

        public virtual void Update(double delta) { }
	}
}