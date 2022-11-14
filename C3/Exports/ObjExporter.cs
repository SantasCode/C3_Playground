using C3.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports
{
    public static class ObjExporter
    {
        public static void Export(C3Model model, TextWriter tw)
        {
            //Is each mesh an obj group (g) or object (o)?
            int vertIdx = 1;
            for(int mi = 0; mi < model.Meshs.Count(); mi++)
            {
                C3Phy mesh = model.Meshs[mi];
                tw.WriteLine($"g mesh {mesh.Name}");

                for(int vi = 0; vi < mesh.Vertices?.Count(); vi++)
                {
                    tw.WriteLine($"v {mesh.Vertices[vi].Position.X} {mesh.Vertices[vi].Position.Y} {mesh.Vertices[vi].Position.Z}");
                    tw.WriteLine($"vt {mesh.Vertices[vi].U} {mesh.Vertices[vi].V}");
                }
                tw.WriteLine();
                tw.WriteLine();
                for (int ii = 0; ii < mesh.Indices?.Count(); ii += 3)
                {
                    tw.WriteLine($"f {mesh.Indices[ii] + vertIdx}/{mesh.Indices[ii] + vertIdx} {mesh.Indices[ii+1] + vertIdx}/{mesh.Indices[ii+1] + vertIdx} {mesh.Indices[ii+2] + vertIdx}/{mesh.Indices[ii+2] + vertIdx}");
                }
                tw.WriteLine();
                vertIdx += mesh.Vertices?.Count() ?? 1;
            }
        }
    }
}
