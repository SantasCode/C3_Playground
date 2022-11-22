using C3.Core;
using C3.Elements;
using C3.Exports.GLTF;
using C3.Exports.GLTF.Schema;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace C3.Exports
{
    public static class GLTF2Export
    {
        public static void Export(C3Model model, StreamWriter sw)
        {

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            C3Phy c3mesh = model.Meshs[0];

            #region Indices
            ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(c3mesh.Indices.Reverse().ToArray()));

            GLTF.Schema.Buffer indicesBuffer = new()
            {
                ByteLength = indicesByteSpan.Length,
                Name = "Indices Bufffer",
                Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(indicesByteSpan)
            };

            BufferView indicesBuffView = new()
            {
                Buffer = indicesBuffer,
                ByteOffset = 0,
                ByteLength = c3mesh.Indices.Length * sizeof(ushort),
                Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER
            };

            Accessor indicesAccessor = new()
            {
                BufferView = indicesBuffView,
                ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = c3mesh.Indices.Length,
                Type = Accessor.TypeEnum.SCALAR,
                //Min = new() { (float)c3mesh.Indices.Min() },
                //Max = new() { (float)c3mesh.Indices.Max() }
            };
            #endregion Indices

            #region Vertices

            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector2 maxUV = new Vector2(float.MinValue, float.MinValue);
            Vector2 minUV = new Vector2(float.MaxValue, float.MaxValue);

            foreach (var phyVertex in c3mesh.Vertices)
            {
                var vertex = phyVertex.Position;
                if (vertex.X > max.X) max.X = vertex.X;
                if (vertex.Y > max.Y) max.Y = vertex.Y;
                if (vertex.Z > max.Z) max.Z = vertex.Z;

                if (vertex.X < min.X) min.X = vertex.X;
                if (vertex.Y < min.Y) min.Y = vertex.Y;
                if (vertex.Z < min.Z) min.Z = vertex.Z;

                if (phyVertex.U > maxUV.X) maxUV.X = phyVertex.U;
                if (phyVertex.V > maxUV.Y) maxUV.Y = phyVertex.V;

                if (phyVertex.U < minUV.X) minUV.X = phyVertex.U;
                if (phyVertex.V < minUV.Y) minUV.Y = phyVertex.V;
            }

            Span<float> c3vertices = new(new float[c3mesh.Vertices.Length * 5]);
            int idx = 0;
            foreach (var phyVertex in c3mesh.Vertices)
            {
                c3vertices[idx] = phyVertex.Position.X;
                c3vertices[idx + 1] = phyVertex.Position.Y;
                c3vertices[idx + 2] = phyVertex.Position.Z;
                c3vertices[idx + 3] = phyVertex.U;
                c3vertices[idx + 4] = phyVertex.V;
                idx += 5;
            }

            ReadOnlySpan<byte> verticesByteSpan = MemoryMarshal.Cast<float, byte>(c3vertices);

            GLTF.Schema.Buffer verticesBuffer = new()
            {
                ByteLength = verticesByteSpan.Length,
                Name = "Vertices Buffer",
                Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(verticesByteSpan)
            };

            BufferView verticesBuffView = new()
            {
                Buffer = verticesBuffer,
                ByteLength = c3mesh.Vertices.Length * sizeof(float) * 5, //3 floats per Vector3 + Vector2.
                Target = BufferView.TargetEnum.ARRAY_BUFFER,
                ByteStride = sizeof(float) * 5 // 1x Vector3 + 1x Vector2;
            };

            Accessor verticesAccessor = new()
            {
                BufferView = verticesBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = c3mesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC3,
                Min = new() { min.X, min.Y, min.Z },
                Max = new() { max.X, max.Y, max.Z }
            };

            Accessor uvAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = sizeof(float) * 3,//Offset for 1x Vector3
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = c3mesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC2,
                //Min = new() { minUV.X, minUV.Y },
                //Max = new() { maxUV.X, maxUV.Y }
            };
            #endregion Vertices

            #region Texture
            byte[] imBytes = File.ReadAllBytes(@"C:\Temp\Conquer\410285.png");
            Image image = new()
            {
                Uri = "data:image/png;base64," + Convert.ToBase64String(imBytes),
                Name = "Texture Image"
            };

            Texture texture = new()
            {
                Name = "Texture",
                Source = image
            };

            Material material = new()
            {
                PbrMetallicRoughness = new()
                {
                    BaseColorTexture = new()
                    {
                        Index = texture
                    },
                    MetallicFactor = 0
                }
            };
            #endregion Texture

            Mesh mesh = new()
            {
                Primitives = new()
                {
                    new()
                    {
                        Material = material,
                        Indices = indicesAccessor, //Index to indices accessor
                        Attributes = new()
                        {
                            { "POSITION", verticesAccessor },
                            { "TEXCOORD_0", uvAccessor }
                        }
                    }
                },
                Name = "Base"
            };

            //Ignoring "skin" (weights/joints) for weapons.

            Node primaryNode = new()
            {
                Mesh = mesh
            };

            Scene scene = new()
            {
                Nodes = new() { primaryNode }
            };

            //Build the glTF

            Gltf gltf = new Gltf()
            {
                Asset = new() { Version = "2.0" },
                Accessors = new()
                {
                    indicesAccessor,
                    verticesAccessor,
                    uvAccessor
                },
                Buffers = new()
                {
                    indicesBuffer,
                    verticesBuffer
                },
                BufferViews= new()
                {
                    indicesBuffView,
                    verticesBuffView
                },
                Images = new() { image },
                Textures = new() { texture },
                Materials = new() { material },
                Meshes = new() { mesh },
                Scenes = new() { scene },
                Scene = scene,
                Nodes = new () { primaryNode }
            };



            sw.Write(JsonSerializer.Serialize(gltf, jsonSerializerOptions));
        }
    //    public static void Export(C3Model model, StreamWriter tw)
    //    {
    //        //Only A Weapon for now. - only 2 weapons ahve a second mesh, not used.
    //        //Get first mesh.
    //        C3Phy c3mesh = model.Meshs[0];


    //        Gltf gltfModel = new();




    //        #region Indices
    //        Accessor indicesAccessor = new();
    //        indicesAccessor.BufferView = 0;
    //        indicesAccessor.ByteOffset = 0;
    //        indicesAccessor.ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT;
    //        indicesAccessor.Count = c3mesh.Indices.Length;
    //        indicesAccessor.Type = Accessor.TypeEnum.SCALAR;
    //        indicesAccessor.Min = new float[1];
    //        indicesAccessor.Min[0] = c3mesh.Indices.Min();
    //        indicesAccessor.Max = new float[1];
    //        indicesAccessor.Max[0] = c3mesh.Indices.Max();


    //        BufferView indicesBuffView = new();
    //        indicesBuffView.Buffer = 0;
    //        indicesBuffView.ByteOffset = 0;
    //        indicesBuffView.ByteLength = indicesAccessor.Count * 2; //2 bytes per ushort.
    //        indicesBuffView.Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER;

    //        glTFLoader.Schema.Buffer indicesBuffer = new();
    //        indicesBuffer.ByteLength = indicesBuffView.ByteLength;
    //        indicesBuffer.Name = "Indices Buffer";
    //        ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(c3mesh.Indices.Reverse().ToArray()));
    //        indicesBuffer.Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(indicesByteSpan);
    //        #endregion Indices

    //        #region Vertices

    //        //Find bounding box.
    //        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    //        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    //        Vector2 maxUV = new Vector2(float.MinValue, float.MinValue);
    //        Vector2 minUV = new Vector2(float.MaxValue, float.MaxValue);

    //        foreach (var phyVertex in c3mesh.Vertices)
    //        {
    //            var vertex = phyVertex.Position;
    //            if (vertex.X > max.X) max.X = vertex.X;
    //            if (vertex.Y > max.Y) max.Y = vertex.Y;
    //            if (vertex.Z > max.Z) max.Z = vertex.Z;

    //            if (vertex.X < min.X) min.X = vertex.X;
    //            if (vertex.Y < min.Y) min.Y = vertex.Y;
    //            if (vertex.Z < min.Z) min.Z = vertex.Z;

    //            if (phyVertex.U > maxUV.X) maxUV.X = phyVertex.U;
    //            if (phyVertex.V > maxUV.Y) maxUV.Y = phyVertex.V;
                
    //            if (phyVertex.U < minUV.X) minUV.X = phyVertex.U;
    //            if (phyVertex.V < minUV.Y) minUV.Y = phyVertex.V;
    //        }

    //        Accessor verticesAccessor = new();
    //        verticesAccessor.BufferView = 1;
    //        verticesAccessor.ByteOffset = 0;
    //        verticesAccessor.ComponentType = Accessor.ComponentTypeEnum.FLOAT;
    //        verticesAccessor.Count = c3mesh.Vertices.Length;
    //        verticesAccessor.Type = Accessor.TypeEnum.VEC3;
    //        verticesAccessor.Min = new float[3];
    //        verticesAccessor.Min[0] = min.X;
    //        verticesAccessor.Min[1] = min.Y;
    //        verticesAccessor.Min[2] = min.Z;
    //        verticesAccessor.Max = new float[3];
    //        verticesAccessor.Max[0] = max.X;
    //        verticesAccessor.Max[1] = max.Y;
    //        verticesAccessor.Max[2] = max.Z;

    //        Accessor uvAccessor = new();
    //        uvAccessor.BufferView = 1;
    //        uvAccessor.ByteOffset = sizeof(float) * 3;//Offset for 1x Vector3
    //        uvAccessor.ComponentType = Accessor.ComponentTypeEnum.FLOAT;
    //        uvAccessor.Count = c3mesh.Vertices.Length;
    //        uvAccessor.Type = Accessor.TypeEnum.VEC2;
    //        uvAccessor.Min = new float[2];
    //        uvAccessor.Min[0] = minUV.X;
    //        uvAccessor.Min[1] = minUV.Y;
    //        uvAccessor.Max = new float[2];
    //        uvAccessor.Max[0] = maxUV.X;
    //        uvAccessor.Max[1] = maxUV.Y;

    //        BufferView verticesBuffView = new();
    //        verticesBuffView.Buffer = 1;
    //        verticesBuffView.ByteOffset = 0;
    //        verticesBuffView.ByteLength = c3mesh.Vertices.Length * sizeof(float) * 5; //3 floats per Vector3 + Vector2.
    //        verticesBuffView.Target = BufferView.TargetEnum.ARRAY_BUFFER;
    //        verticesBuffView.ByteStride = sizeof(float) * 5; // 1x Vector3 + 1x Vector2;

    //        glTFLoader.Schema.Buffer verticesBuffer = new();
    //        verticesBuffer.ByteLength = verticesBuffView.ByteLength;
    //        verticesBuffer.Name = "Vertices Buffer";
    //        Span<float> c3vertices = new(new float[c3mesh.Vertices.Length * 5]);
    //        int idx = 0;
    //        foreach(var phyVertex in c3mesh.Vertices)
    //        {
    //            c3vertices[idx] = phyVertex.Position.X;
    //            c3vertices[idx + 1] = phyVertex.Position.Y;
    //            c3vertices[idx + 2] = phyVertex.Position.Z;
    //            c3vertices[idx + 3] = phyVertex.U;
    //            c3vertices[idx + 4] = phyVertex.V;
    //            idx += 5;
    //        }

    //        ReadOnlySpan<byte> verticesByteSpan = MemoryMarshal.Cast<float, byte>(c3vertices);
    //        verticesBuffer.Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(verticesByteSpan);
    //        #endregion Vertices

    //        #region Texture
    //        Image image = new();
    //        byte[] imBytes = File.ReadAllBytes(@"C:\Temp\Conquer\410285.png");
    //        image.Uri = "data:image/png;base64," + Convert.ToBase64String(imBytes);
    //        image.Name = "Texture Image";

    //        gltfModel.Images = new Image[1];
    //        gltfModel.Images[0] = image;

    //        Texture texture = new();
    //        texture.Name = "Texture";
    //        texture.Source = 0; //Image index.

    //        gltfModel.Textures = new Texture[1];
    //        gltfModel.Textures[0] = texture;

    //        Material material = new();
    //        //material.DoubleSided = true;
    //        material.PbrMetallicRoughness = new()
    //        {
    //            BaseColorTexture = new()
    //            {
    //                Index = 0
    //            },
    //            MetallicFactor = 0,
    //            RoughnessFactor = 1
    //        };

    //        gltfModel.Materials = new Material[1];
    //        gltfModel.Materials[0] = material;
    //        #endregion Texture




    //        gltfModel.Accessors = new Accessor[3];
    //        gltfModel.Accessors[0] = indicesAccessor;
    //        gltfModel.Accessors[1] = verticesAccessor;
    //        gltfModel.Accessors[2] = uvAccessor;

    //        gltfModel.BufferViews = new BufferView[2];
    //        gltfModel.BufferViews[0] = indicesBuffView;
    //        gltfModel.BufferViews[1] = verticesBuffView;

    //        gltfModel.Buffers = new glTFLoader.Schema.Buffer[2];
    //        gltfModel.Buffers[0] = indicesBuffer;
    //        gltfModel.Buffers[1] = verticesBuffer;

    //        MeshPrimitive primitive = new();
    //        primitive.Material = 0;
    //        primitive.Indices = 0; //Index to indices accessor
    //        primitive.Attributes = new();
    //        primitive.Attributes.Add("POSITION", 1);//Index to vertices accessor
    //        primitive.Attributes.Add("TEXCOORD_0", 2);

    //        Mesh mesh = new();
    //        mesh.Primitives = new MeshPrimitive[1];
    //        mesh.Primitives[0] = primitive;
    //        mesh.Name = "Base";

    //        gltfModel.Meshes = new Mesh[1];
    //        gltfModel.Meshes[0] = mesh;

    //        //Ignoring "skin" (weights/joints) for weapons.

    //        Node primaryNode = new();
    //        primaryNode.Mesh = 0;

    //        Scene scene = new();
    //        scene.Nodes = new int[1];
    //        scene.Nodes[0] = 0;

    //        gltfModel.Nodes = new Node[1];
    //        gltfModel.Nodes[0] = primaryNode;

    //        gltfModel.Scenes = new Scene[1];
    //        gltfModel.Scenes[0] = scene;

    //        gltfModel.Asset = new()
    //{
    //            Generator = "C3glTF",
    //            Version = "2.0"
    //        };

    //        gltfModel.SaveModel(tw.BaseStream);
    //    }

    }
}
