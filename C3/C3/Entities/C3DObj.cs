using C3.Elements;

using C3.Loaders;
namespace C3.Entities
{

    public class C3DObj
    {
        //Data
        public List<C3Phy?>? Phys;
        public float[]? X;
        public float[]? Y;
        public float[]? Z;

        public static uint PHY_MAX => 16;

        //System Functions
        bool IsValid() { throw new NotImplementedException(); }
        public bool Create(string filename) 
        {
            Phys = new List<C3Phy?>();
            using (BinaryReader br = new(File.OpenRead(filename)))
            {
                string header = br.ReadASCIIString(16);

                while(br.BaseStream.Position < br.BaseStream.Length)
                {
                    ChunkHeader chunkHeader = br.ReadChunkHeader();
                    switch (chunkHeader.Id)
                    {
                        case "PHY ":
                            Phys.Add(C3PhyLoader.Load(br, "PHY "));
                            break;
                        /*
                        case "PHY2":
                            Phys.Add(C3PhyLoader.Load(br, true, false, true));
                            break;
                        case "PHY3":
                            Phys.Add(C3PhyLoader.Load(br, true, false, false));
                            break;
                        */
                        case "PHY4":
                            Phys.Add(C3PhyLoader.Load(br, "PHY4"));
                            break;
                        
                        default:
                            Console.WriteLine($"Chunk Header: {chunkHeader.Id}");
                            br.BaseStream.Seek(chunkHeader.Size, SeekOrigin.Current);
                            break;

                    }
                }
            }
            return true;
        }
        void Destroy() { throw new NotImplementedException(); }
        void Move(float x, float y, float z) { throw new NotImplementedException(); }
        void Rotate(float x, float y, float z) { throw new NotImplementedException(); }
        void Scale(float x, float y, float z) { throw new NotImplementedException(); }
        void SetARGB(float alpha, float red, float green, float blue) { throw new NotImplementedException(); }

        int GetIndexByName(string lpName) { throw new NotImplementedException(); }

        //void SetMotion(C3DMotion* pMotion) { throw new NotImplementedException(); }

        void NextFrame(int nStep) { throw new NotImplementedException(); }

        void SetFrame(int dwFrame) { throw new NotImplementedException(); }

        void Draw(int type, float lightx, float lighty, float lightz, float sa, float sr, float sg, float sb) { throw new NotImplementedException(); }
        void DrawAlpha(int type, float lightx, float lighty, float lightz, float sa, float sr, float sg, float sb, float height) { throw new NotImplementedException(); }
        //void ChangeTexture(C3DTexture* pTexture, string? objname = null) { throw new NotImplementedException(); }

        static void Prepare() { throw new NotImplementedException(); }

        void ClearMatrix() { throw new NotImplementedException(); }
    }
}
