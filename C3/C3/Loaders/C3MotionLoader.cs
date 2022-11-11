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

            mot.Matrix = new Matrix[mot.BoneCount];

            for (int i = 0; i < mot.BoneCount; i++)
                mot.Matrix[i] = Core.Matrix.Identity;

            mot.Type = br.ReadASCIIString(4);

            switch (mot.Type)
            {
                case "KKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();

                        mot.KeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for (int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.KeyFrames[i] = new()
                            {
                                Pos = br.ReadUInt32(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };


                            for (int b = 0; b < mot.BoneCount; b++)
                                mot.KeyFrames[i].Matricies[b] = br.ReadMatrix();
                        }
                    }
                    break;

                case "ZKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();
                        mot.KeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for (int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.KeyFrames[i] = new()
                            {
                                Pos = br.ReadUInt16(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };

                            for(int b = 0; b < mot.BoneCount; b++)
                            {
                                mot.KeyFrames[i].Matricies[b] = Matrix.CreateFromQuaternion(br.ReadQuaternion());

                                mot.KeyFrames[i].Matricies[b].M41 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M42 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M43 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M44 = 1.0f;

                            }
                        }
                    }
                    break;

                case "XKEY":
                    {
                        mot.KeyFramesCount = br.ReadUInt32();

                        mot.KeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for(int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.KeyFrames[i] = new()
                            {
                                Pos = br.ReadUInt16(),
                                Matricies = new Core.Matrix[mot.BoneCount]
                            };

                            for (int b = 0; b < mot.BoneCount; b++)
                            {
                                mot.KeyFrames[i].Matricies[b] = new();

                                mot.KeyFrames[i].Matricies[b].M11 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M12 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M13 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M14 = 0.0f;
                                mot.KeyFrames[i].Matricies[b].M21 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M22 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M23 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M24 = 0.0f;
                                mot.KeyFrames[i].Matricies[b].M31 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M32 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M33 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M34 = 0.0f;
                                mot.KeyFrames[i].Matricies[b].M41 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M42 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M43 = br.ReadSingle();
                                mot.KeyFrames[i].Matricies[b].M44 = 1.0f;
                            }
                        }
                    }
                    break;

                default:
                    {
                        br.BaseStream.Seek(-4, SeekOrigin.Current);

                        mot.KeyFramesCount = mot.FrameCount;

                        mot.KeyFrames = new C3KeyFrame[mot.KeyFramesCount];

                        for(int i = 0; i < mot.KeyFramesCount; i++)
                        {
                            mot.KeyFrames[i] = new()
                            {
                                Pos = (uint)i,
                                Matricies = new Matrix[mot.BoneCount]
                            };
                        }
                        for(int b = 0; b < mot.BoneCount; b++)
                        {
                            for(int i = 0; i < mot.KeyFramesCount; i++)
                            {
                                mot.KeyFrames[i].Matricies[b] = br.ReadMatrix();
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
