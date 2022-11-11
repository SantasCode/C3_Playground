using C3.Elements;


namespace C3.Loaders
{
    public static class C3ParticleLoader
    {
        public static C3Particle Load(BinaryReader br)
        {
            var particle = new C3Particle();

            particle.Name = br.ReadASCIIString(br.ReadUInt32());
            particle.TextureName = br.ReadASCIIString(br.ReadUInt32());

            particle.Row = br.ReadUInt32();

            particle.Count = br.ReadUInt32();

            particle.FrameCount = br.ReadUInt32();

            particle.Vertices = new ParticleVertex[particle.Count * 4];

            particle.Indicies = new ushort[particle.Count * 6];

            particle.ParticleFrames = new ParticleFrame[particle.FrameCount];

            for (int i = 0; i < particle.FrameCount; i++)
            {
                particle.ParticleFrames[i] = new ParticleFrame();

                particle.ParticleFrames[i].Count = br.ReadUInt32();

                if (particle.ParticleFrames[i].Count > 0)
                {
                    particle.ParticleFrames[i].Position = new Core.Vector3[particle.ParticleFrames[i].Count];
                    particle.ParticleFrames[i].Age = new float[particle.ParticleFrames[i].Count];
                    particle.ParticleFrames[i].Size = new float[particle.ParticleFrames[i].Count];

                    for (int p = 0; p < particle.ParticleFrames[i].Count; p++)
                        particle.ParticleFrames[i].Position[p] = br.ReadVector3();

                    for (int p = 0; p < particle.ParticleFrames[i].Count; p++)
                        particle.ParticleFrames[i].Age[p] = br.ReadSingle();

                    for (int p = 0; p < particle.ParticleFrames[i].Count; p++)
                        particle.ParticleFrames[i].Size[p] = br.ReadSingle();

                    particle.ParticleFrames[i].Matrix = br.ReadMatrix();
                }

            }

            return particle;
        }
    }
}
