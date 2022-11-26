using C3.Core;
using C3.Elements;
using C3.Exports.GLTF;
using C3.Exports.GLTF.Schema;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buffer = C3.Exports.GLTF.Schema.Buffer;

namespace C3.Exports
{
    public class GLTF2Export
    {
        private Gltf gltf;
        public GLTF2Export()
        {
            gltf = new()
            {
                Asset = new() { Version = "2.0", Generator = "C3 GLTF Exporter" }
            };
        }
       
        public void Export(C3Model model, string texturePath, StreamWriter sw)
        {
            //If it doesn't have v_body, just export as a simple weapon/item.
            var bodyMesh = model.Meshs.Where(p => p.Name == "v_body").FirstOrDefault();
            if (bodyMesh == null || model.Meshs.Count <= 2) //Two items have 2 PHY objects or two "meshes" Others have more.
            {
                //ExportSimple(model, sw);
                return;
            }

            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Accessors == null) gltf.Accessors = new();
            if (gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Meshes == null) gltf.Meshes = new();
            if (gltf.Materials == null) gltf.Materials = new();
            if (gltf.Textures == null) gltf.Textures = new();
            if (gltf.Images == null) gltf.Images = new();
            if (gltf.Scenes == null) gltf.Scenes = new();

            #region Skin
            //The Skin is the same skin used for all the different meshes of this model. Multiple nodes will refer to this skin/skeleton

            Node skinnedMeshNode = new()
            {
                Name = "Player"
            };
            gltf.Nodes.Add(skinnedMeshNode);

            var bodyMeshIdx = model.Meshs.IndexOf(bodyMesh);
            var skinResults = BuildSkeletonSkin(bodyMesh, model.Animations[bodyMeshIdx]);
            //Add skin to main node.
            skinnedMeshNode.Skin = skinResults.Skin;

            #endregion Skin

            Node transformNode = new()
            {
                Name = "Z-up",
                Rotation = new Quaternion(1, 0, 0, 1).Normalize().ToArray(),
                Children = new() { skinResults.Skin.Skeleton }
            };
            gltf.Nodes.Add(transformNode);

            #region Animation
            AddAnimation("Pose 1", model.Animations[0], skinResults.JointNodeMap);
            //foreach (var file in Directory.GetFiles(@"D:\Programming\Conquer\Clients\5165\c3\0002\000"))
            //{
            //    C3Model newModel = new();
            //    using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
            //        newModel = C3ModelLoader.Load(br);
            //    string fileName = new FileInfo(file).Name;
            //    if (newModel != null)
            //        AddAnimation(fileName, newModel.Animations[bodyMeshIdx], skinResults.JointNodeMap);
            //}
            #endregion Animation

            #region Geometry
            var vectorBufferSize = (bodyMesh.Vertices.Length * 11 * 4);
            var geoBufferSize = vectorBufferSize + bodyMesh.Indices.Count() * sizeof(ushort);//Index array size

            DynamicByteBuffer c3geometryBuffer = new(geoBufferSize);


            //Adjust vertices for initial matrix.
            foreach (var vertex in bodyMesh.Vertices)
            {
                vertex.Position = vertex.Position.Transform(bodyMesh.InitMatrix);
            }

            //Calculate the new bounding box.
            (var max, var min, var maxUV, var minUV) = GetBoundingBox(bodyMesh.Vertices);
            

            /* Vertex info struct
             * struct[44]{
             *  [0] Vector3 Position
             *  [12] Vector2 UV
             *  [20] ushort[4] Joint
             *  [28]float[4] Weight
             *}
             */
            DynamicByteBuffer c3vertBuff = new(bodyMesh.Vertices.Length * 11 * 4);

            //Copy vertices to buffer.
            foreach (var phyVertex in bodyMesh.Vertices)
            {
                c3geometryBuffer.Write(phyVertex.Position);
                c3geometryBuffer.Write(phyVertex.U);
                c3geometryBuffer.Write(phyVertex.V);
                c3geometryBuffer.Write(new ushort[] { (ushort)phyVertex.BoneWeights[0].Joint, (ushort)phyVertex.BoneWeights[1].Joint, 0, 0 });
                c3geometryBuffer.Write(new float[] { phyVertex.BoneWeights[0].Weight, phyVertex.BoneWeights[1].Weight, 0, 0 });
            }

            //Copy indices to buffer.
            //Reverse the indices array for winding direction correction.
            ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(bodyMesh.Indices.Reverse().ToArray()));

            c3geometryBuffer.Write(indicesByteSpan.ToArray());

            //Setup geometry buffer
            Buffer geometryBuffer = new()
            {
                ByteLength = c3geometryBuffer.Count,
                Name = "Geometry Buffer",
                Uri = "data:application/gltf-buffer;base64," + c3geometryBuffer.ToBase64()
            };

            //Setup vertex bufferviews and accessors.
            BufferView verticesBuffView = new()
            {
                Buffer = geometryBuffer,
                ByteLength = vectorBufferSize,
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

            Accessor jointAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 20,//Offset for 1x Vector3 + 1x Vector2
                ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "joint accessor"
            };

            Accessor weightAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 28,//Offset for 1x Vector3 + 1x Vector2 + 1x Vector4
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = bodyMesh.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "weight accessor"
            };

            //Setup index bufferviews and accessors.
            BufferView indicesBuffView = new()
            {
                Buffer = geometryBuffer,
                ByteOffset = vectorBufferSize,
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
            #endregion Geometry

            #region Texture
            byte[] imBytes = File.ReadAllBytes(texturePath);
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

            gltf.Buffers.AddRange(new List<Buffer> { geometryBuffer });
            gltf.BufferViews.AddRange(new List<BufferView> { indicesBuffView, verticesBuffView });
            gltf.Accessors.AddRange(new List<Accessor> { indicesAccessor, verticesAccessor, uvAccessor, jointAccessor, weightAccessor });

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
        private BuildSkinResults BuildSkeletonSkin(C3Phy mesh, C3Motion motion)
        {
            if(gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Skins == null) gltf.Skins = new();

            Node commonRoot = new()
            {
                Name = "skeleton",
                Children = new(),
            };
            gltf.Nodes.Add(commonRoot);


            Dictionary<string, Node> jointNodeMap = new();

            #region Skin
            Skin skin = new() { Joints = new(), Skeleton = commonRoot };
            gltf.Skins.Add(skin);

            
            //Populate the ibm with the matricies from unnamed bones.
            var vBody = mesh;

            if (vBody == null) throw new NotSupportedException("Expecting v_body to exist");

            var vbodyMoti = motion;

            if (vbodyMoti.KeyFramesCount != 1) throw new NotSupportedException("v_body is expected to have 1 key frame");


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

            }

            #endregion Skin

            return new BuildSkinResults() { Skin = skin, JointNodeMap = new(jointNodeMap) };
        }
        
        private void AddAnimation(string name, C3Motion motion, ReadOnlyDictionary<string, Node> boneNodeMap)
        {
            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Accessors == null) gltf.Accessors = new();
            if (gltf.Animations == null) gltf.Animations = new();
            if(gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
                
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
                    Matrix m = keyFrame.Matricies[boneIdx];
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
