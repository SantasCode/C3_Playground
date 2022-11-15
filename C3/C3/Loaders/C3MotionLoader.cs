using C3.Core;
using C3.Elements;

namespace C3.Loaders
{
    public static class C3MotionLoader
    {
        public static C3Motion Load(BinaryReader br)
        {
            var mot = new C3Motion();

            mot.BoneCount = br.ReadUInt32();
            mot.FrameCount = br.ReadUInt32();

            mot.Type = br.ReadASCIIString(4);

            switch (mot.Type)
            {
                case "KKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();

                        mot.BoneKeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for (int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.BoneKeyFrames[i] = new()
                            {
                                FrameNumber = br.ReadUInt32(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };


                            for (int b = 0; b < mot.BoneCount; b++)
                                mot.BoneKeyFrames[i].Matricies[b] = br.ReadMatrix();
                        }
                    }
                    break;

                case "ZKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();
                        mot.BoneKeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for (int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.BoneKeyFrames[i] = new()
                            {
                                FrameNumber = br.ReadUInt16(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };

                            for(int b = 0; b < mot.BoneCount; b++)
                            {
                                mot.BoneKeyFrames[i].Matricies[b] = Matrix.CreateFromQuaternion(br.ReadQuaternion());

                                mot.BoneKeyFrames[i].Matricies[b].M41 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M42 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M43 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M44 = 1.0f;

                            }
                        }
                    }
                    break;

                case "XKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();

                        mot.BoneKeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for(int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.BoneKeyFrames[i] = new()
                            {
                                FrameNumber = br.ReadUInt16(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };

                            for (int b = 0; b < mot.BoneCount; b++)
                            {
                                mot.BoneKeyFrames[i].Matricies[b] = new();

                                mot.BoneKeyFrames[i].Matricies[b].M11 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M12 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M13 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M14 = 0.0f;
                                mot.BoneKeyFrames[i].Matricies[b].M21 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M22 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M23 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M24 = 0.0f;
                                mot.BoneKeyFrames[i].Matricies[b].M31 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M32 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M33 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M34 = 0.0f;
                                mot.BoneKeyFrames[i].Matricies[b].M41 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M42 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M43 = br.ReadSingle();
                                mot.BoneKeyFrames[i].Matricies[b].M44 = 1.0f;
                            }
                        }
                    }
                    break;

                default:
                    {
                        br.BaseStream.Seek(-4, SeekOrigin.Current);

                        mot.KeyFramesCount = mot.FrameCount;

                        mot.BoneKeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for(int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.BoneKeyFrames[i] = new()
                            {
                                FrameNumber = (uint)i,
                                Matricies = new Matrix[mot.BoneCount]
                            };
                        }
                        for(int b = 0; b < mot.BoneCount; b++)
                        {
                            for(int i = 0; i < mot.KeyFramesCount; i++)
                            {
                                mot.BoneKeyFrames[i].Matricies[b] = br.ReadMatrix();
                            }
                        }
                    }
                    break;

            }

            //Morph
            mot.MorphCount = br.ReadUInt32();
            
            mot.morph = new float[mot.MorphCount];

            for (int i = 0; i < mot.MorphCount; i++)
                mot.morph[i] = br.ReadSingle();

            return mot;
        }
    }
}
