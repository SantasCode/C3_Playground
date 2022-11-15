using C3.Elements;

namespace C3.Loaders
{
    public static class C3ShapeLoader
    {
        public static C3Shape Load(BinaryReader br)
        {
            C3Shape result = new();

            result.Name = br.ReadASCIIString(br.ReadUInt32());

            int lineCount = (int)br.ReadUInt32();
            result.Lines = new(lineCount);

            for(int i = 0; i < result.Lines.Capacity; i++)
            {
                int vectorCount = (int)br.ReadUInt32();
                result.Lines.Add(new());
                result.Lines[i].LineVertices = new Core.Vector3[vectorCount];
                for(int v = 0; v < result.Lines[i].LineVertices.Length; v++)
                {
                    result.Lines[i].LineVertices[v] = br.ReadVector3();
                }
            }
            
            result.TextureName = br.ReadASCIIString(br.ReadUInt32());
            
            result.Segment = br.ReadUInt32();
            
            return result;
        }
    }
}
