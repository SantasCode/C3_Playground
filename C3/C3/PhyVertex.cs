using C3.Core;

namespace C3
{
    public class PhyVertex
    {
        public Vector3 Position { get; set; }
        public float U { get; set; }
        public float V { get; set; }

        public uint Color { get; set; }
        
        public JointWeight[] BoneWeights { get; set; }
        public Vector3? UnknownVector3 { get; set; }



        public static uint BONE_MAX => 2;
        public static uint MORPH_MAX => 4;


    }
    public class JointWeight
    {
        public uint Joint { get; set; }
        public float Weight { get; set; }
    }
}
