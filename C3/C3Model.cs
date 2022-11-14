using C3.Elements;

namespace C3
{
    public class C3Model
    {
        public List<C3Phy> Meshs { get; set; } = new();
        public List<C3Motion> Animations { get; set; } = new();
        public List<C3Particle> Effects { get; set; } = new();
        public List<C3Camera> Cameras { get; set; } = new();
    }
}
