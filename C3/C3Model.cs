using C3.Elements;

namespace C3
{
    public class C3Model
    {
        public List<C3Phy> Meshs = new();
        public List<C3Motion> Animations = new();
        public List<C3Particle> Effects = new();
        public List<C3Camera> Cameras = new();
    }
}
