using C3.Core;
using C3.Elements;
using glTFLoader;
using glTFLoader.Schema;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace C3.Exports
{
    public static class GLTF2Export
    {
        public static void Export(C3Model model, StreamWriter tw)
        {
            //Only A Weapon for now. - only 2 weapons ahve a second mesh, not used.
            //Get first mesh.
            C3Phy c3mesh = model.Meshs[0];


            Gltf gltfModel = new();




            #region Indices
            Accessor indicesAccessor = new();
            indicesAccessor.BufferView = 0;
            indicesAccessor.ByteOffset = 0;
            indicesAccessor.ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT;
            indicesAccessor.Count = c3mesh.Indices.Length;
            indicesAccessor.Type = Accessor.TypeEnum.SCALAR;
            indicesAccessor.Min = new float[1];
            indicesAccessor.Min[0] = c3mesh.Indices.Min();
            indicesAccessor.Max = new float[1];
            indicesAccessor.Max[0] = c3mesh.Indices.Max();


            BufferView indicesBuffView = new();
            indicesBuffView.Buffer = 0;
            indicesBuffView.ByteOffset = 0;
            indicesBuffView.ByteLength = indicesAccessor.Count * 2; //2 bytes per ushort.
            indicesBuffView.Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER;

            glTFLoader.Schema.Buffer indicesBuffer = new();
            indicesBuffer.ByteLength = indicesBuffView.ByteLength;
            indicesBuffer.Name = "Indices Buffer";
            ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(c3mesh.Indices));
            indicesBuffer.Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(indicesByteSpan);
            #endregion Indices

            #region Vertices

            //Find bounding box.
            Vector3 max = Vector3.Zero;
            Vector3 min = Vector3.Zero;

            foreach (var phyVertex in c3mesh.Vertices)
            {
                var vertex = phyVertex.Position;
                if (vertex.X > max.X) max.X = vertex.X;
                if (vertex.Y > max.Y) max.Y = vertex.Y;
                if (vertex.Z > max.Z) max.Z = vertex.Z;

                if (vertex.X < min.X) min.X = vertex.X;
                if (vertex.Y < min.Y) min.Y = vertex.Y;
                if (vertex.Z < min.Z) min.Z = vertex.Z;
            }

            Accessor verticesAccessor = new();
            verticesAccessor.BufferView = 1;
            verticesAccessor.ByteOffset = 0;
            verticesAccessor.ComponentType = Accessor.ComponentTypeEnum.FLOAT;
            verticesAccessor.Count = c3mesh.Vertices.Length;
            verticesAccessor.Type = Accessor.TypeEnum.VEC3;
            verticesAccessor.Min = new float[3];
            verticesAccessor.Min[0] = min.X;
            verticesAccessor.Min[1] = min.Y;
            verticesAccessor.Min[2] = min.Z;
            verticesAccessor.Max = new float[3];
            verticesAccessor.Max[0] = max.X;
            verticesAccessor.Max[1] = max.Y;
            verticesAccessor.Max[2] = max.Z;

            BufferView verticesBuffView = new();
            verticesBuffView.Buffer = 1;
            verticesBuffView.ByteOffset = 0;
            verticesBuffView.ByteLength = c3mesh.Vertices.Length * sizeof(float) * 3; //3 floats per Vector3.
            verticesBuffView.Target = BufferView.TargetEnum.ARRAY_BUFFER;

            glTFLoader.Schema.Buffer verticesBuffer = new();
            verticesBuffer.ByteLength = verticesBuffView.ByteLength;
            verticesBuffer.Name = "Vertices Buffer";
            Span<float> c3vertices = new(new float[c3mesh.Vertices.Length * 3]);
            Span<float> c3UV = new(new float[c3mesh.Vertices.Length * 2]);
            int idx = 0;
            int uvIdx = 0;
            foreach(var phyVertex in c3mesh.Vertices)
            {
                c3vertices[idx] = phyVertex.Position.X;
                c3vertices[idx + 1] = phyVertex.Position.Y;
                c3vertices[idx + 2] = phyVertex.Position.Z;
                idx += 3;
                c3UV[uvIdx] = phyVertex.U;
                c3UV[uvIdx + 1] = phyVertex.V;
                uvIdx += 2;
            }

            ReadOnlySpan<byte> verticesByteSpan = MemoryMarshal.Cast<float, byte>(c3vertices);
            verticesBuffer.Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(verticesByteSpan);
            #endregion Vertices

            #region Texture

            #endregion Texture




            gltfModel.Accessors = new Accessor[2];
            gltfModel.Accessors[0] = indicesAccessor;
            gltfModel.Accessors[1] = verticesAccessor;

            gltfModel.BufferViews = new BufferView[2];
            gltfModel.BufferViews[0] = indicesBuffView;
            gltfModel.BufferViews[1] = verticesBuffView;

            gltfModel.Buffers = new glTFLoader.Schema.Buffer[2];
            gltfModel.Buffers[0] = indicesBuffer;
            gltfModel.Buffers[1] = verticesBuffer;

            MeshPrimitive primitive = new();
            primitive.Indices = 0; //Index to indices accessor
            primitive.Attributes = new();
            primitive.Attributes.Add("POSITION", 1);//Index to vertices accessor

            Mesh mesh = new();
            mesh.Primitives = new MeshPrimitive[1];
            mesh.Primitives[0] = primitive;
            mesh.Name = "Base";

            gltfModel.Meshes = new Mesh[1];
            gltfModel.Meshes[0] = mesh;

            //Ignoring "skin" (weights/joints) for weapons.

            Node primaryNode = new();
            primaryNode.Mesh = 0;

            Scene scene = new();
            scene.Nodes = new int[1];
            scene.Nodes[0] = 0;

            gltfModel.Nodes = new Node[1];
            gltfModel.Nodes[0] = primaryNode;

            gltfModel.Scenes = new Scene[1];
            gltfModel.Scenes[0] = scene;

            gltfModel.Asset = new()
            {
                Generator = "C3glTF",
                Version = "2.0"
            };

            gltfModel.SaveModel(tw.BaseStream);
        }
    }
}
