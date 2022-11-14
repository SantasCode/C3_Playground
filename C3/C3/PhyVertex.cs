using C3.Core;

namespace C3
{
    public class PhyVertex
    {
        public Vector3 Position { get; set; }//Do we really need more than one. eu source shows size 4, 5579 apperas to only read 1.
        public float U { get; set; }
        public float V { get; set; }

        public uint Color { get; set; }
        public (uint, float)[] BoneWeights { get; set; }
        public Vector3? UnknownVector3 { get; set; }



        public static uint BONE_MAX => 2;
        public static uint MORPH_MAX => 4;


    }
}
