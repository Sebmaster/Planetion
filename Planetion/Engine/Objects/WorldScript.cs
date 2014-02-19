
namespace Planetion.Objects {
    public abstract class WorldScript {

        public World World;

        public virtual void Awake() { }

        public virtual void Update(float delta) { }
    }
}
