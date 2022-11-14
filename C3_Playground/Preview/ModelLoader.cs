
using C3;
using C3.Elements;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace C3_Playground.Preview
{
    //internal static class ModelLoader
    //{
    //    public static Model? Load(C3Model c3Model, GraphicsDevice graphiicDevice)
    //    {
    //        List<ModelMesh> meshes = new List<ModelMesh>();
    //        List<ModelBone> bones = new List<ModelBone>();
    //        List<ModelMeshPart> meshParts = new();

    //        foreach (C3Phy mesh in c3Model.Meshs)
    //        {
    //            //ModelMeshPart meshPart = new();
    //            //IndexBuffer indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, model.Meshs[meshnum].Indices.Count(), BufferUsage.WriteOnly);
    //            //indexBuffer.SetData<ushort>(model.Meshs[meshnum].Indices);

    //            //VertexPositionColor[] verticies = new VertexPositionColor[model.Meshs[meshnum].Vertices.Count()];
    //            //for (int i = 0; i < model.Meshs[meshnum].Vertices.Count(); i++)
    //            //{
    //            //    verticies[i] = new()
    //            //    {
    //            //        Color = Color.White,
    //            //        //TextureCoordinate = new Vector2(model.Meshs[0].Vertices[i].U, model.Meshs[0].Vertices[i].V),
    //            //        Position = new Vector3(model.Meshs[meshnum].Vertices[i].pos[0].X, model.Meshs[meshnum].Vertices[i].pos[0].Y, model.Meshs[meshnum].Vertices[i].pos[0].Z)
    //            //    };
    //            //}
    //            //vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), verticies.Length, BufferUsage.WriteOnly);
    //            //vertexBuffer.SetData<VertexPositionColor>(verticies);
    //            /*
    //            int meshnum = 3;
    //            if (model.Meshs[meshnum] != null && model.Meshs[meshnum].Indices != null)
    //            {
    //                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, model.Meshs[meshnum].Indices.Count(), BufferUsage.WriteOnly);
    //                indexBuffer.SetData<ushort>(model.Meshs[meshnum].Indices);

    //                VertexPositionColor[] verticies = new VertexPositionColor[model.Meshs[meshnum].Vertices.Count()];
    //                for(int i =0; i < model.Meshs[meshnum].Vertices.Count(); i++)
    //                {
    //                    verticies[i] = new()
    //                    {
    //                        Color = Color.White,
    //                        //TextureCoordinate = new Vector2(model.Meshs[0].Vertices[i].U, model.Meshs[0].Vertices[i].V),
    //                        Position = new Vector3(model.Meshs[meshnum].Vertices[i].pos[0].X, model.Meshs[meshnum].Vertices[i].pos[0].Y, model.Meshs[meshnum].Vertices[i].pos[0].Z)
    //                    };
    //                }
    //                vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), verticies.Length, BufferUsage.WriteOnly);
    //                vertexBuffer.SetData<VertexPositionColor>(verticies);
    //            }  
    //            */
    //        }
    //        return null;
    //    }
    //}
}
