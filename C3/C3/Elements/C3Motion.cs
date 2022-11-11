using C3.Core;


namespace C3.Elements
{
    public class C3Motion
    {
        public uint BoneCount;
        public uint FrameCount;

        public uint KeyFramesCount;
        public C3KeyFrame[]? KeyFrames;

        public Matrix[]? Matrix;

        public uint MorphCount;
        public float[] morph;
        public int Frame;

        //Additional, not in file.
        public string? Type;
    }
}
