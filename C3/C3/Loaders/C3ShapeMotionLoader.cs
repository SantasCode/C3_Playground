using C3.Elements;

namespace C3.Loaders
{
    public static class C3ShapeMotionLoader
    {
        public static C3ShapeMotion Load(BinaryReader br)
        {
            C3ShapeMotion motion = new ();

            motion.Matrices = new((int)br.ReadUInt32());

            for(int i = 0; i < motion.Matrices.Capacity; i++)
            {
                motion.Matrices.Add(br.ReadMatrix());
            }

            return motion;
        }
    }
}
