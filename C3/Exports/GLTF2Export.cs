using C3.Core;
using C3.Elements;
using C3.Exports.GLTF.Schema;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buffer = C3.Exports.GLTF.Schema.Buffer;

namespace C3.Exports
{
    public static class GLTF2Export
    {
        public static void ExportSimple(C3Model model, StreamWriter sw)
        {
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
                Min = new() { (float)c3mesh.Indices.Min() },
                Max = new() { (float)c3mesh.Indices.Max() }
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


            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            sw.Write(JsonSerializer.Serialize(gltf, jsonSerializerOptions));
        }
        public static void Export(C3Model model, StreamWriter sw)
        {
            //If it doesn't have v_body, just export as a simple weapon/item.
            var bodyMesh = model.Meshs.Where(p => p.Name == "v_body").FirstOrDefault();
            if (bodyMesh == null || model.Meshs.Count <= 2) //Two items have 2 PHY objects or two "meshes" Others have more.
            {
                ExportSimple(model, sw);
                return;
            }

            Gltf gltf = new()
            {
                Asset = new()
                {
                    Version = "2.0"
                },
                Nodes = new(),
                Accessors = new(),
                Buffers = new(),
                BufferViews = new(),
                Meshes = new(),
                Images = new(),
                Materials = new(),
                Textures = new(),
                Scenes = new()
            };

            Node skinnedMeshNode = new()
            {
                Name = "Player"
            };
            gltf.Nodes.Add(skinnedMeshNode);


            #region Skin

            var skinResults = BuildSkin(bodyMesh, model.Animations[0], ref gltf);
            //Add skin to main node.
            skinnedMeshNode.Skin = skinResults.Skin;

            #endregion Skin

            Node transformNode = new()
            {
                //Matrix = bodyMesh.InitMatrix.ToArray(),
                Name = "initial transform",
                Children = new() { skinResults.Skin.Skeleton }
            };
            gltf.Nodes.Add(transformNode);

            #region Animation
            BuildAnimation(bodyMesh.InitMatrix, "Pose 1", model.Animations[0], skinResults.JointNodeMap, ref gltf);
            foreach (var file in Directory.GetFiles(@"D:\Programming\Conquer\Clients\5165\c3\0002\000"))
            {
                C3Model newModel = new();
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                    newModel = C3ModelLoader.Load(br);
                string fileNAme = new FileInfo(file).Name;
                if (newModel != null)
                    BuildAnimation(bodyMesh.InitMatrix, fileNAme, newModel.Animations[0], skinResults.JointNodeMap, ref gltf);
            }
            #endregion Animation

            #region Indices
            //Reverse the indices array for winding direction correction.
            ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(bodyMesh.Indices.Reverse().ToArray()));

            Buffer indicesBuffer = new()
            {
                ByteLength = indicesByteSpan.Length,
                Name = "Indices Bufffer",
                Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(indicesByteSpan)
            };

            BufferView indicesBuffView = new()
            {
                Buffer = indicesBuffer,
                ByteOffset = 0,
                ByteLength = bodyMesh.Indices.Length * sizeof(ushort),
                Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER
            };

            Accessor indicesAccessor = new()
            {
                BufferView = indicesBuffView,
                ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = bodyMesh.Indices.Length,
                Type = Accessor.TypeEnum.SCALAR,
                Min = new() { (float)bodyMesh.Indices.Min() },
                Max = new() { (float)bodyMesh.Indices.Max() },
                Name = "indices accessor"
            };
            #endregion Indices

            #region Vertices

            (var max, var min, var maxUV, var minUV) = GetBoundingBox(bodyMesh.Vertices);
            /*
             * struct[44]{
             *  [0] Vector3 Position
             *  [12] Vector2 UV
             *  [20] ushort[4] Joint
             *  [28]float[4] Weight
             *}
             */

            Span<byte> c3verticesBuffer = new(new byte[bodyMesh.Vertices.Length * 11 * 4]);
            
            int idx = 0;

            foreach (var phyVertex in bodyMesh.Vertices)
            {
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), phyVertex.Position.X);
                idx += 4;
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), phyVertex.Position.Y);
                idx += 4;
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), phyVertex.Position.Z);
                idx += 4;
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), phyVertex.U);
                idx += 4;
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), phyVertex.V);
                idx += 4;

                //Remap Joint to new node index.
                var bone1Weight = phyVertex.BoneWeights[0].Weight;
                var bone2Weight = phyVertex.BoneWeights[1].Weight;
                var bone1Joint = (ushort)phyVertex.BoneWeights[0].Joint;
                var bone2Joint = (ushort)phyVertex.BoneWeights[1].Joint;
                                                
                //Write Joints
                BinaryPrimitives.WriteUInt16LittleEndian(c3verticesBuffer.Slice(idx), (ushort)bone1Joint);
                idx += 2;
                BinaryPrimitives.WriteUInt16LittleEndian(c3verticesBuffer.Slice(idx), (ushort)bone2Joint);
                idx += 2;
                idx += 2;//Two 0 ushorts
                idx += 2;

                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), bone1Weight);
                idx += 4;
                BinaryPrimitives.WriteSingleLittleEndian(c3verticesBuffer.Slice(idx), bone2Weight);
                idx += 4;
                idx += 4;//Two 0 floats.
                idx += 4;

            }

            Buffer verticesBuffer = new()
            {
                ByteLength = c3verticesBuffer.Length,
                Name = "Vertices Buffer",
                Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(c3verticesBuffer)
            };

            BufferView verticesBuffView = new()
            {
                Buffer = verticesBuffer,
                ByteLength = bodyMesh.Vertices.Length * 44, //3 floats per Vector3 + Vector2.
                Target = BufferView.TargetEnum.ARRAY_BUFFER,
                ByteStride = 44
            };

            Accessor verticesAccessor = new()
            {
                BufferView = verticesBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC3,
                Min = new() { min.X, min.Y, min.Z },
                Max = new() { max.X, max.Y, max.Z },
                Name = "vertices accessor"
            };

            Accessor uvAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 12,//Offset for 1x Vector3
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC2,
                Min = new() { minUV.X, minUV.Y },
                Max = new() { maxUV.X, maxUV.Y },
                Name = "UV accessor"
            };

            //Joint Accessor
            Accessor jointAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 20,//Offset for 1x Vector3 + 1x Vector2
                ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "joint accessor"
            };

            //Weight Accessor
            Accessor weightAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 28,//Offset for 1x Vector3 + 1x Vector2 + 1x Vector4
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "weight accessor"
            };
            #endregion Vertices

            #region Texture
            byte[] imBytes = File.ReadAllBytes(@"C:\Temp\Conquer\002000000.png");
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

            gltf.Images.Add(image);
            gltf.Textures.Add(texture);
            gltf.Materials.Add(material);
            #endregion Texture

            #region Mesh
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
                            { "TEXCOORD_0", uvAccessor },
                            { "JOINTS_0", jointAccessor },
                            { "WEIGHTS_0", weightAccessor }
                        }
                    }
                },
                Name = "Base"
            };
            //Add mesh to node.
            skinnedMeshNode.Mesh = mesh;

            gltf.Meshes.Add(mesh);
            #endregion Mesh

            #region Scene
            Scene scene = new()
            {
                Nodes = new() { skinnedMeshNode, transformNode }//skinResults.Skin.Skeleton }
            };
            gltf.Scenes.Add(scene);
            gltf.Scene = scene;
            #endregion Scene

            //Add components to top level collections.

            gltf.Buffers.AddRange(new List<Buffer> { indicesBuffer, verticesBuffer });
            gltf.BufferViews.AddRange(new List<BufferView> { indicesBuffView, verticesBuffView });
            gltf.Accessors.AddRange(new List<Accessor> { indicesAccessor, verticesAccessor, uvAccessor, jointAccessor, weightAccessor });


            /*
            //Loop through children mesh and create child nodes.
            foreach (var c3mesh in model.Meshs.Where(p => p.Name != "v_body"))
            {
                Node childNode = new()
                {
                    Name = c3mesh.Name
                };

                #region Indices
                ReadOnlySpan<byte> childIndicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(c3mesh.Indices.Reverse().ToArray()));

                Buffer childIndicesBuffer = new()
                {
                    ByteLength = childIndicesByteSpan.Length,
                    Name = "Indices Bufffer",
                    Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(childIndicesByteSpan)
                };

                BufferView childIndicesBuffView = new()
                {
                    Buffer = childIndicesBuffer,
                    ByteOffset = 0,
                    ByteLength = c3mesh.Indices.Length * sizeof(ushort),
                    Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER
                };

                Accessor childIndicesAccessor = new()
                {
                    BufferView = childIndicesBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                    Count = c3mesh.Indices.Length,
                    Type = Accessor.TypeEnum.SCALAR,
                    Min = new() { (float)c3mesh.Indices.Min() },
                    Max = new() { (float)c3mesh.Indices.Max() }
                };
                #endregion Indices

                #region Vertices

                (var cmax, var cmin, var cmaxUV, var cminUV) = GetBoundingBox(c3mesh.Vertices);

                Span<float> childC3vertices = new(new float[c3mesh.Vertices.Length * 5]);
                int childIdx = 0;
                foreach (var phyVertex in c3mesh.Vertices)
                {
                    childC3vertices[childIdx] = phyVertex.Position.X;
                    childC3vertices[childIdx + 1] = phyVertex.Position.Y;
                    childC3vertices[childIdx + 2] = phyVertex.Position.Z;
                    childC3vertices[childIdx + 3] = phyVertex.U;
                    childC3vertices[childIdx + 4] = phyVertex.V;
                    childIdx += 5;
                }

                ReadOnlySpan<byte> childVerticesByteSpan = MemoryMarshal.Cast<float, byte>(childC3vertices);

                Buffer childVerticesBuffer = new()
                {
                    ByteLength = childVerticesByteSpan.Length,
                    Name = "Vertices Buffer",
                    Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(childVerticesByteSpan)
                };

                BufferView childVerticesBuffView = new()
                {
                    Buffer = childVerticesBuffer,
                    ByteLength = c3mesh.Vertices.Length * sizeof(float) * 5, //3 floats per Vector3 + Vector2.
                    Target = BufferView.TargetEnum.ARRAY_BUFFER,
                    ByteStride = sizeof(float) * 5 // 1x Vector3 + 1x Vector2;
                };

                Accessor childVerticesAccessor = new()
                {
                    BufferView = childVerticesBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Count = c3mesh.Vertices.Length,
                    Type = Accessor.TypeEnum.VEC3,
                    Min = new() { cmin.X, cmin.Y, cmin.Z },
                    Max = new() { cmax.X, cmax.Y, cmax.Z }
                };

                Accessor childUvAccessor = new()
                {
                    BufferView = childVerticesBuffView,
                    ByteOffset = sizeof(float) * 3,//Offset for 1x Vector3
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Count = c3mesh.Vertices.Length,
                    Type = Accessor.TypeEnum.VEC2,
                    //Min = new() { minUV.X, minUV.Y },
                    //Max = new() { maxUV.X, maxUV.Y }
                };
                #endregion Vertices

                //Child nodes don't have textures/materials

                #region Mesh
                Mesh childMesh = new()
                {
                    Primitives = new()
                    {
                        new()
                        {
                            Indices = childIndicesAccessor, //Index to indices accessor
                            Attributes = new()
                            {
                                { "POSITION", childVerticesAccessor },
                                { "TEXCOORD_0", childUvAccessor }
                            }
                        }
                    },
                    Name = "Base"
                };

                gltf.Meshes.Add(childMesh);

                //Add mesh to node
                childNode.Mesh = childMesh;
                #endregion Mesh

                //Add components to top level collections.
                gltf.Nodes.Add(childNode);
                gltf.Buffers.AddRange(new List<Buffer> { childIndicesBuffer, childVerticesBuffer });
                gltf.BufferViews.AddRange(new List<BufferView> { childIndicesBuffView, childVerticesBuffView });
                gltf.Accessors.AddRange(new List<Accessor> { childUvAccessor, childIndicesAccessor, childVerticesAccessor });

                //Add this child node to the parent node.
                topLevelNode.Children.Add(childNode);
            }
            */

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            sw.Write(JsonSerializer.Serialize(gltf, jsonSerializerOptions));
        }
        private class BuildSkinResults
        {
            public required Skin Skin { get; init; }
            public required ReadOnlyDictionary<string, Node> JointNodeMap { get; init; }
        }
        private static BuildSkinResults BuildSkin(C3Phy mesh, C3Motion motion, ref Gltf gltf)
        {
            if (gltf.Skins == null) gltf.Skins = new();
            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Accessors == null) gltf.Accessors = new();

            Node commonRoot = new()
            {
                Name = "skeleton",
                Children = new(),
                //Matrix = mesh.InitMatrix.ToArray()
            };
            gltf.Nodes.Add(commonRoot);


            Dictionary<string, Node> jointNodeMap = new();

            #region Skin
            Skin skin = new() { Joints = new(), Skeleton = commonRoot };
            gltf.Skins.Add(skin);

            //skin.Joints.Add(vbodyNode);

            //Build the buffer containing all the skin matricies
            int numberBones = (int)motion.BoneCount;

            int matricesByteLength = numberBones * 16 * sizeof(float);//Each matrix is 16 floating point values.

            Span<byte> ibm = new Span<byte>(new byte[matricesByteLength]);
            int ibmIdx = 0;

            //Populate the ibm with the matricies from unnamed bones.
            var vBody = mesh;

            if (vBody == null) throw new NotSupportedException("Expecting v_body to exist");

            var vbodyMoti = motion;

            if (vbodyMoti.KeyFramesCount != 1) throw new NotSupportedException("v_body is expected to have 1 key frame");

            var keyFrame = vbodyMoti.BoneKeyFrames[0];

            for (int i = 0; i < vbodyMoti.BoneCount; i++)
            {
                //Create a joint node
                Node jNode = new() { Name = $"bone{i}" };
                gltf.Nodes.Add(jNode);


                //Add to skin joints
                skin.Joints.Add(jNode);
                //Add node to base skin node.
                commonRoot.Children.Add(jNode);

                //Track this node for later.
                jointNodeMap.Add(jNode.Name, jNode);

                //Multiply By the mesh initial matrix of the mesh
                Matrix cm = Matrix.Multiply(vBody.InitMatrix, keyFrame.Matricies[i]);
                //Matrix cm = keyFrame.Matricies[i];
                
                
                Matrix cmT = cm.Transpose();

                if (!cm.Decompose(out var scale, out var rotation, out var translation)) throw new NotSupportedException("Unable to decompose matrix into transform, scale, and rotation compoenents");

                cmT.DecomposeCM(out var t2, out var r2, out var s2);
                
                //jNode.Matrix = cm.ToArray();

                //Matrix m2 = Matrix.Compose(t2, r2, s2);

                //jNode.Scale = s2.ToArray();
                //jNode.Rotation = r2.ToArray();
                //jNode.Translation = t2.ToArray();

                var scaleMatrix = Matrix.CreateFromScale(scale);//.Transpose();
                var rotatMatrix = Matrix.CreateFromQuaternion(rotation);//.Transpose();
                var transMatrix = Matrix.CreateFromTranslation(translation);//.Transpose();

                var composed = Matrix.Multiply(Matrix.Multiply(transMatrix, rotatMatrix),  scaleMatrix).Transpose();

                Span<float> matrixFloatSpan = new Span<float>(cm.ToArray());
                //Convert float span to byte span
                Span<byte> matrixByteSpan = MemoryMarshal.Cast<float, byte>(matrixFloatSpan);
                //Copy byte span to main matrix span buffer.
                matrixByteSpan.CopyTo(ibm.Slice(ibmIdx));

                ibmIdx += 64;
            }

            //Serialize the ibm into a buffer/bufferView/accessor and add to skin.
            Buffer skinIbmBufffer = new()
            {
                ByteLength = ibm.Length,
                Name = "Inverse Bind Matrices Buffer",
                Uri = "data:application/gltf-buffer;base64," + Convert.ToBase64String(ibm)
            };

            BufferView skinIbmBuffView = new()
            {
                Buffer = skinIbmBufffer,
                ByteLength = ibm.Length
            };

            Accessor skinIbmAccessor = new()
            {
                BufferView = skinIbmBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = numberBones,
                Type = Accessor.TypeEnum.MAT4,
                Name = "Skin IBM accessor",
            };

            gltf.Buffers.Add(skinIbmBufffer);
            gltf.BufferViews.Add(skinIbmBuffView);
            gltf.Accessors.Add(skinIbmAccessor);

            //skin.InverseBindMatrices = skinIbmAccessor;
            #endregion Skin

            return new BuildSkinResults() { Skin = skin, JointNodeMap = new(jointNodeMap) };
        }
        
        private static void BuildAnimation(Matrix initMatrix, string name, C3Motion motion, ReadOnlyDictionary<string, Node> boneNodeMap, ref Gltf gltf)
        {
            if(gltf.Buffers == null) gltf.Buffers = new();
            if(gltf.Nodes == null) gltf.Nodes = new();
            if(gltf.Accessors == null) gltf.Accessors = new();
            if(gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Animations == null) gltf.Animations = new();
                
            int byteStride =12 + 12 + 16;//float, vec3, vec3, vec 4
            int totalBytes = (byteStride * (int)motion.BoneCount + 4)* motion.BoneKeyFrames.Count() ;

            //Single Buffer
            //One BuffferView Per Node.
            //Six Accessors Per Node.
            DynamicByteBuffer animBufffer = new(totalBytes);

            Buffer buffer = new() { ByteLength = totalBytes };
            gltf.Buffers.Add(buffer);

            Animation animation = new() 
            {
                Channels = new(),
                Samplers = new(),
                Name = name,
            };
            gltf.Animations.Add(animation);

            //Assumptions - need to test before reducing footprint of bytebuffer.
            //KKEY can have scale, rotation, and translation
            //ZKEY can have rotation and translation.
            //XKEY can have scale and rotation

            //Time per frame in seconds
            float timePerFrame = 33f / 1000f;//Assumptions - 33ms per frame.

            //Build timeBufferView, time is same for all bones
            float minTime = float.MaxValue;
            float maxTime = float.MinValue;
            foreach (var keyFrame in motion.BoneKeyFrames)
            {
                //Write the Time.
                float time = keyFrame.FrameNumber * timePerFrame;
                
                if(time > maxTime) maxTime = time;
                if(time < minTime) minTime = time;

                animBufffer.Write(time);
            }

            BufferView timeBuffView = new()
            {
                Buffer = buffer,
                Name = $"frame time",
                ByteLength = 4 * motion.BoneKeyFrames.Count()
            };
            gltf.BufferViews.Add(timeBuffView);

            Accessor timeAccessor = new()
                {
                    BufferView = timeBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.SCALAR,
                    Name = $"frame time",
                    Count = (int)motion.BoneKeyFrames.Count(),
                    Min = new() { minTime},
                    Max = new() { maxTime}
            };
            gltf.Accessors.Add(timeAccessor);


            for (int boneIdx = 0; boneIdx < motion.BoneCount; boneIdx++) 
            {
                BufferView scaleBuffView = new()
                {
                    Buffer = buffer,
                    Name = $"bone{boneIdx} scale",
                    ByteLength = 12 * motion.BoneKeyFrames.Count(),
                    ByteOffset = animBufffer.Count,
                };
                BufferView translationBuffView = new()
                {
                    Buffer = buffer,
                    Name = $"bone{boneIdx} trans",
                    ByteLength = 12 * motion.BoneKeyFrames.Count(),
                    ByteOffset = animBufffer.Count + 12 * motion.BoneKeyFrames.Count(),
                };
                BufferView rotationBuffView = new()
                {
                    Buffer = buffer,
                    Name = $"bone{boneIdx} rot",
                    ByteLength = 16 * motion.BoneKeyFrames.Count(),
                    ByteOffset = animBufffer.Count + 12 * motion.BoneKeyFrames.Count() + 12 * motion.BoneKeyFrames.Count(),
                };
                gltf.BufferViews.AddRange(new List<BufferView> { scaleBuffView, translationBuffView, rotationBuffView });

                Accessor scaleMatrixAccessor = new()
                {
                    BufferView = scaleBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC3,
                    Name = $"bone{boneIdx} scale",
                    Count = (int)motion.BoneKeyFrames.Count()
                };
                Accessor translationMatrixAccessor = new()
                {
                    BufferView = translationBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC3,
                    Name = $"bone{boneIdx} trans",
                    Count = (int)motion.BoneKeyFrames.Count()
                };
                Accessor rotationMatrixAccessor = new()
                {
                    BufferView = rotationBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC4,
                    Name = $"bone{boneIdx} rot",
                    Count = (int)motion.BoneKeyFrames.Count()
                };
                gltf.Accessors.AddRange( new List<Accessor> { scaleMatrixAccessor, translationMatrixAccessor, rotationMatrixAccessor });

                //Three samplers per bone.
                AnimationSampler scaleSampler = new()
                {
                    Input = timeAccessor,
                    Output = scaleMatrixAccessor
                };
                AnimationSampler translationSampler = new()
                {
                    Input = timeAccessor,
                    Output = translationMatrixAccessor
                };
                AnimationSampler rotationSampler = new()
                {
                    Input = timeAccessor,
                    Output = rotationMatrixAccessor
                };
                animation.Samplers.AddRange( new List<AnimationSampler> { scaleSampler, translationSampler, rotationSampler });

                //Three channels per bone.
                AnimationChannel scaleChannel = new()
                {
                    Sampler = scaleSampler,
                    Target = new() 
                    { 
                        Path = AnimationChannelTarget.PathEnum.scale,
                        Node = boneNodeMap[$"bone{boneIdx}"]
                    }
                };
                AnimationChannel translationChannel = new()
                {
                    Sampler = translationSampler,
                    Target = new()
                    {
                        Path = AnimationChannelTarget.PathEnum.translation,
                        Node = boneNodeMap[$"bone{boneIdx}"]
                    }
                };
                AnimationChannel rotationChannel = new()
                {
                    Sampler = rotationSampler,
                    Target = new()
                    {
                        Path = AnimationChannelTarget.PathEnum.rotation,
                        Node = boneNodeMap[$"bone{boneIdx}"]
                    }
                };

                animation.Channels.AddRange(new List<AnimationChannel> { scaleChannel, translationChannel, rotationChannel });

                DynamicByteBuffer transDynamicBuffer = new(12 * motion.BoneKeyFrames.Count());
                DynamicByteBuffer rotationDynamicBuffer = new(16 * motion.BoneKeyFrames.Count());

                foreach (var keyFrame in motion.BoneKeyFrames)
                {
                    Matrix m = Matrix.Multiply(initMatrix, keyFrame.Matricies[boneIdx]);
                    //Matrix m = keyFrame.Matricies[boneIdx];
                    m.Transpose().DecomposeCM(out var translation, out var rotation, out var scale);

                    animBufffer.Write(scale);
                    transDynamicBuffer.Write(translation);
                    rotationDynamicBuffer.Write(rotation);
                }
                animBufffer.Write(transDynamicBuffer.ToArray());
                animBufffer.Write(rotationDynamicBuffer.ToArray());
            }
            buffer.Uri = "data:application/gltf-buffer;base64," + animBufffer.ToBase64();
            buffer.Name = "Animation " + name;
        }

        private static (Vector3 BoxMin, Vector3 BoxMax, Vector2 UVMin, Vector2 UVMax) GetBoundingBox(PhyVertex[] vertices)
        {
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector2 maxUV = new Vector2(float.MinValue, float.MinValue);
            Vector2 minUV = new Vector2(float.MaxValue, float.MaxValue);

            foreach (var phyVertex in vertices)
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
            return (max, min, maxUV, minUV);
        }
    }
}
