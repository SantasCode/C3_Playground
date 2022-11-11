using C3.Core;
using C3.Elements;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace C3.Loaders
{
    public static class C3PhyLoader
    {
        public static C3Phy Load(BinaryReader br, string Type)
        {
            Console.WriteLine(Type);
            C3Phy phy = new();

            phy.Name = br.ReadASCIIString(br.ReadInt32());

            phy.BlendCount = br.ReadUInt32();

            phy.AVectorCount = br.ReadUInt32();
            phy.NVectorCount = br.ReadUInt32();

            phy.Vertices = new PhyVertex[phy.AVectorCount + phy.NVectorCount];

            for (int i = 0; i < (phy.AVectorCount + phy.NVectorCount); i++)
            {
                phy.Vertices[i] = LoadPhyVertex(br, Type);

            }



            phy.NTriCount = br.ReadUInt32();
            phy.ATriCount = br.ReadUInt32();

            phy.Indices = new ushort[(phy.NTriCount + phy.ATriCount) * 3];

            for (int i = 0; i < (phy.NTriCount + phy.ATriCount) * 3; i++)
                phy.Indices[i] = br.ReadUInt16();

            phy.TextureName = br.ReadASCIIString(br.ReadInt32());

            phy.BoxMin = br.ReadVector3();
            phy.BoxMax = br.ReadVector3();

            phy.InitMatrix = br.ReadMatrix();

            phy.TextureRow = br.ReadUInt32();

            //Key
            phy.Key = new C3Key();
            phy.Key.AlphaFrames = new C3Frame[br.ReadUInt32()];
            for (int i = 0; i < phy.Key.AlphaFrames.Count(); i++)
                phy.Key.AlphaFrames[i] = new C3Frame()
                {
                    Frame = br.ReadInt32(),
                    floatParam = br.ReadSingle(),
                    boolParam = Convert.ToBoolean(br.ReadUInt32()),
                    intParam = br.ReadInt32()
                };

            phy.Key.DrawFrames = new C3Frame[br.ReadUInt32()];
            for (int i = 0; i < phy.Key.DrawFrames.Count(); i++)
                phy.Key.DrawFrames[i] = new C3Frame()
                {
                    Frame = br.ReadInt32(),
                    floatParam = br.ReadSingle(),
                    boolParam = Convert.ToBoolean(br.ReadUInt32()),
                    intParam = br.ReadInt32()
                };

            phy.Key.ChangeTextFrames = new C3Frame[br.ReadUInt32()];
            for (int i = 0; i < phy.Key.ChangeTextFrames.Count(); i++)
                phy.Key.ChangeTextFrames[i] = new C3Frame()
                {
                    Frame = br.ReadInt32(),
                    floatParam = br.ReadSingle(),
                    boolParam = Convert.ToBoolean(br.ReadUInt32()),
                    intParam = br.ReadInt32()
                };

            if (br.ReadASCIIString(4) == "STEP")
                phy.uvStep = br.ReadVector2();
            else
                br.BaseStream.Seek(-4, SeekOrigin.Current);

            if (br.ReadASCIIString(4) == "2SID")
            {
                string unk = br.ReadASCIIString(4);
                switch (unk)
                {
                    case "BILB":
                    case "BIB2":
                    case "BIB3":
                    case "BIB4":
                        //Sets a C3Phy property (0x51 in Mac 5579 to 1/2/3/4).
                        Console.WriteLine($"[C3PhyLoader] Unknown Element {unk}");
                        break;
                    default:
                        br.BaseStream.Seek(-4, SeekOrigin.Current);
                        break;
                }
            }
            else
                br.BaseStream.Seek(-4, SeekOrigin.Current);

            return phy;
        }

        private static PhyVertex LoadPhyVertex(BinaryReader br, string Type)
        {
            var vert = new PhyVertex()
            {
                pos = new Vector3[PhyVertex.MORPH_MAX],
                index = new uint[PhyVertex.BONE_MAX],
                weight = new float[PhyVertex.BONE_MAX]
            };

            vert.pos[0] = br.ReadVector3();
            for (int v = 1; v < PhyVertex.MORPH_MAX; v++)
                vert.pos[v] = Vector3.Zero;

            if (Type == "PHY " || Type == "PHY2")
                br.BaseStream.Seek(0x24, SeekOrigin.Current);

            vert.U = br.ReadSingle();
            vert.V = br.ReadSingle();

            vert.color = br.ReadUInt32();

            for (int b = 0; b < PhyVertex.BONE_MAX; b++)
                vert.index[b] = br.ReadUInt32();
            for (int b = 0; b < PhyVertex.BONE_MAX; b++)
                vert.weight[b] = br.ReadSingle();

            if (Type == "PHY2" || Type == "PHY3")
                vert.unknownV3 = br.ReadVector3();

            return vert;
        }
    }
}
