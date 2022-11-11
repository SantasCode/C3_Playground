using C3.Loaders;

namespace C3
{
    public static class C3ModelLoader
    {
        public static C3Model? Load(BinaryReader br, bool verbose = false) 
        {
            var role = new C3Model();

            //Skip 16 bytes, file header.
            string header = br.ReadASCIIString(16);

            if (header != "MAXFILE C3 00001")
            {
                if(verbose)
                    Console.WriteLine("Invalid file header");
                return null;
            }
            string PreviousType = "";

            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                ChunkHeader chunkHeader = br.ReadChunkHeader();
                if(verbose)
                    Console.WriteLine($"Chunk Type {chunkHeader.Id}");

                switch (chunkHeader.Id)
                {
                    //case "PHY ": role.Meshs.Add(C3PhyLoader.Load(br, "PHY ")); break;
                    //case "PHY4": role.Meshs.Add(C3PhyLoader.Load(br, "PHY4")); break;
                    case "PHY ":
                    case "PHY2":
                    case "PHY3":
                    case "PHY4":
                        role.Meshs.Add(C3PhyLoader.Load(br, chunkHeader.Id));break;
                    case "MOTI": role.Animations.Add(C3MotionLoader.Load(br)); break;
                    case "PTCL": role.Effects.Add(C3ParticleLoader.Load(br)); break;
                    case "CAME": role.Cameras.Add(C3CameraLoader.Load(br)); break;
                    default:
                        if(verbose)
                            Console.WriteLine($"[C3ModelLoader] Unknown chunk type: {chunkHeader.Id} size: {chunkHeader.Size} PreviousType: {PreviousType}");
                        br.BaseStream.Seek(chunkHeader.Size, SeekOrigin.Current);
                        break;
                }
                PreviousType = chunkHeader.Id;
            }

            return role;
        }
    }
}
