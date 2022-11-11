using C3.Core;

namespace C3
{
    public class PhyVertex
    {
        public Vector3[]? pos;//Do we really need more than one. eu source shows size 4, 5579 apperas to only read 1.
        public float U, V;

        public uint color;
        public uint[]? index;
        public float[]? weight;

        public static uint BONE_MAX => 2;
        public static uint MORPH_MAX => 4;

        public Vector3? unknownV3;

    }
}
