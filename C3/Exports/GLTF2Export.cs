using C3.Core;
using C3.Elements;
using C3.Exports.GLTF;
using C3.Exports.GLTF.Schema;
using System;
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
            
            
            if (bodyMesh == null || model.Meshs.Count <= 2)
            {
                //ExportSimple(model, sw);
                return;
            }

            //named element to index map.
            Dictionary<string, int> namedElementIndexMap = new();
            for (int i = 0; i < model.Meshs.Count; i++)
                namedElementIndexMap.Add(model.Meshs[i].Name.ToLower(), i);

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
                Name = "v_body"
            };
            gltf.Nodes.Add(skinnedMeshNode);

            var bodyMeshIdx = model.Meshs.IndexOf(bodyMesh);
            var skinResults = BuildSkeletonSkin(bodyMesh, model.Animations[bodyMeshIdx]);
            //Add skin to main node.
            skinnedMeshNode.Skin = skinResults.Skin;

            //Add nodes for other parts (if exists).
            Dictionary<string, Node> namedBoneNodeMap = new();
            var otherParts = model.Meshs.Where(p => p.Name.ToLower() != "v_body").ToList();
            if (otherParts.Any())
            {
                foreach ( var part in otherParts)
                {
                    var partGeo = AddGeometry(part);
                    Mesh partMesh = new()
                    {
                        Primitives = new()
                        {
                            new()
                            {
                                Indices = partGeo.Indices, //Index to indices accessor
                                Attributes = new()
                                {
                                    { "POSITION", partGeo.Vertices }
                                }
                            }
                        },
                        Name = "Base"
                    };
                    Node partNode = new()
                    {
                        Name = part.Name,
                        Mesh = partMesh
                    };
                    namedBoneNodeMap.Add(part.Name, partNode);

                    skinResults.Skin.Skeleton.Children.Add(partNode);
                    gltf.Nodes.Add(partNode);
                    gltf.Meshes.Add(partMesh);
                }
            }

            #endregion Skin

            Node transformNode = new()
            {
                Name = "Z-up",
                Rotation = new Quaternion(1, 0, 0, 1).Normalize().ToArray(),
                Children = new() { skinResults.Skin.Skeleton }
            };
            gltf.Nodes.Add(transformNode);

            #region Animation
            
            AddAnimation("Pose 1", model, skinResults.JointNodeMap, new ReadOnlyDictionary<string, Node>(namedBoneNodeMap), new ReadOnlyDictionary<string, int>(namedElementIndexMap));
            foreach (var file in Directory.GetFiles(@"D:\Programming\Conquer\Clients\5165\c3\0001\410"))
            {
                C3Model newModel = new();
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                    newModel = C3ModelLoader.Load(br);
                string fileName = new FileInfo(file).Name;
                if (newModel != null)
                    AddAnimation(fileName, newModel, skinResults.JointNodeMap, new ReadOnlyDictionary<string, Node>(namedBoneNodeMap), new ReadOnlyDictionary<string, int>(namedElementIndexMap));
            }
            #endregion Animation

            #region Geometry
            var geoResults = AddGeometry(bodyMesh);
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
                        Indices = geoResults.Indices, //Index to indices accessor
                        Attributes = new()
                        {
                            { "POSITION", geoResults.Vertices },
                            { "TEXCOORD_0", geoResults.UVs },
                            { "JOINTS_0", geoResults.Joints },
                            { "WEIGHTS_0", geoResults.Weights }
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


            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            sw.Write(JsonSerializer.Serialize(gltf, jsonSerializerOptions));
        }

        private record AddGeometryResults(Accessor Vertices, Accessor Indices, Accessor UVs, Accessor Joints, Accessor Weights);
        private AddGeometryResults AddGeometry(C3Phy phy)
        {
            if (gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Accessors == null) gltf.Accessors = new();

            /* Vertex info struct
             * struct[44]{
             *  [0] Vector3 Position
             *  [12] Vector2 UV
             *  [20] ushort[4] Joint
             *  [28]float[4] Weight
             *}
             */
            var vectorBufferSize = (phy.Vertices.Length * 11 * 4);
            var geoBufferSize = vectorBufferSize + phy.Indices.Count() * sizeof(ushort);//Index array size

            DynamicByteBuffer c3geometryBuffer = new(geoBufferSize);


            //Adjust vertices for initial matrix.
            foreach (var vertex in phy.Vertices)
            {
                vertex.Position = vertex.Position.Transform(phy.InitMatrix);
            }

            //Calculate the new bounding box.
            (var max, var min, var maxUV, var minUV) = GetBoundingBox(phy.Vertices);

            //Copy vertices to buffer.
            foreach (var phyVertex in phy.Vertices)
            {
                c3geometryBuffer.Write(phyVertex.Position);
                c3geometryBuffer.Write(phyVertex.U);
                c3geometryBuffer.Write(phyVertex.V);
                c3geometryBuffer.Write(new ushort[] { (ushort)phyVertex.BoneWeights[0].Joint, (ushort)phyVertex.BoneWeights[1].Joint, 0, 0 });
                c3geometryBuffer.Write(new float[] { phyVertex.BoneWeights[0].Weight, phyVertex.BoneWeights[1].Weight, 0, 0 });
            }

            //Copy indices to buffer.
            //Reverse the indices array for winding direction correction.
            ReadOnlySpan<byte> indicesByteSpan = MemoryMarshal.Cast<ushort, byte>(new ReadOnlySpan<ushort>(phy.Indices.Reverse().ToArray()));

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
                Count = phy.Vertices.Length,
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
                Count = phy.Vertices.Length,
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
                Count = phy.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "joint accessor"
            };

            Accessor weightAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 28,//Offset for 1x Vector3 + 1x Vector2 + 1x Vector4
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = phy.Vertices.Length,
                Type = Accessor.TypeEnum.VEC4,
                Name = "weight accessor"
            };

            //Setup index bufferviews and accessors.
            BufferView indicesBuffView = new()
            {
                Buffer = geometryBuffer,
                ByteOffset = vectorBufferSize,
                ByteLength = phy.Indices.Length * sizeof(ushort),
                Target = BufferView.TargetEnum.ELEMENT_ARRAY_BUFFER
            };

            Accessor indicesAccessor = new()
            {
                BufferView = indicesBuffView,
                ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                Count = phy.Indices.Length,
                Type = Accessor.TypeEnum.SCALAR,
                Min = new() { (float)phy.Indices.Min() },
                Max = new() { (float)phy.Indices.Max() },
                Name = "indices accessor"
            };

            //Add components to top level collections.
            gltf.Buffers.AddRange(new List<Buffer> { geometryBuffer });
            gltf.BufferViews.AddRange(new List<BufferView> { indicesBuffView, verticesBuffView });
            gltf.Accessors.AddRange(new List<Accessor> { indicesAccessor, verticesAccessor, uvAccessor, jointAccessor, weightAccessor });
            
            return new AddGeometryResults(verticesAccessor, indicesAccessor, uvAccessor, jointAccessor, weightAccessor);
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
        
        private void AddAnimation(string name, C3Model model, ReadOnlyDictionary<string, Node> boneNodeMap, ReadOnlyDictionary<string, Node> namedBoneJointMap, ReadOnlyDictionary<string, int> nameElementIndexMap)
        {
            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Accessors == null) gltf.Accessors = new();
            if (gltf.Animations == null) gltf.Animations = new();
            if(gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();

            var bodyIdx = nameElementIndexMap["v_body"];
            
            var vbodyMotion = model.Animations[bodyIdx];

            var numOtherElements = nameElementIndexMap.Count() - 1;//Subtract one for v_body.

            int byteStride =12 + 12 + 16;//vec3, vec3, vec 4

            int totalBytes = (byteStride * (int)(vbodyMotion.BoneCount + numOtherElements) + 4)* vbodyMotion.BoneKeyFrames.Count(); //+ 4 for the size of a float (time)

            //Single Buffer
            //One BuffferView Per Node.
            //Six Accessors Per Node.
            DynamicByteBuffer animBuffer = new(totalBytes);

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
            foreach (var keyFrame in vbodyMotion.BoneKeyFrames)
            {
                //Write the Time.
                float time = keyFrame.FrameNumber * timePerFrame;
                
                if(time > maxTime) maxTime = time;
                if(time < minTime) minTime = time;

                animBuffer.Write(time);
            }

            BufferView timeBuffView = new()
            {
                Buffer = buffer,
                Name = $"frame time",
                ByteLength = 4 * vbodyMotion.BoneKeyFrames.Count()
            };
            gltf.BufferViews.Add(timeBuffView);

            Accessor timeAccessor = new()
                {
                    BufferView = timeBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.SCALAR,
                    Name = $"frame time",
                    Count = (int)vbodyMotion.BoneKeyFrames.Count(),
                    Min = new() { minTime},
                    Max = new() { maxTime}
            };
            gltf.Accessors.Add(timeAccessor);


            for (int boneIdx = 0; boneIdx < vbodyMotion.BoneCount; boneIdx++) 
            {
                AddBone(vbodyMotion, animBuffer, animation, buffer, timeAccessor, boneIdx, vbodyMotion.BoneKeyFrames.Count(), $"bone{boneIdx}", boneNodeMap[$"bone{boneIdx}"]);
            }

            //Add motion for named bones.
            for (int i = 0; i < model.Animations.Count; i++)
            {
                if (i == nameElementIndexMap["v_body"]) continue;

                var n = nameElementIndexMap.Where(p => p.Value == i).Select(s => s.Key).FirstOrDefault();

                if (n == null) continue;

                if (model.Animations[i].BoneKeyFrames.Count() != vbodyMotion.BoneKeyFrames.Count()) Console.WriteLine("Mismatch");

                AddBone(model.Animations[i], animBuffer, animation, buffer, timeAccessor, 0, model.Animations[i].BoneKeyFrames.Count(), n, namedBoneJointMap[n]);
            }
            Console.WriteLine($"Estimated Buffer Length: {totalBytes} - Actual buffer length: {animBuffer.Count}, Key frame Count: {vbodyMotion.BoneKeyFrames.Count()}");
            buffer.ByteLength = animBuffer.Count;
            buffer.Uri = "data:application/gltf-buffer;base64," + animBuffer.ToBase64();
            buffer.Name = "Animation " + name;
        }
        private void AddBone(C3Motion vbodyMotion, DynamicByteBuffer animBuffer, Animation animation, Buffer buffer, Accessor timeAccessor, int boneIdx, int numKeyFrames, string BoneName, Node boneNode)
        {
            BufferView scaleBuffView = new()
            {
                Buffer = buffer,
                Name = $"{BoneName} scale",
                ByteLength = 12 * numKeyFrames,
                ByteOffset = animBuffer.Count,
            };
            BufferView translationBuffView = new()
            {
                Buffer = buffer,
                Name = $"{BoneName} trans",
                ByteLength = 12 * numKeyFrames,
                ByteOffset = animBuffer.Count + 12 * numKeyFrames,
            };
            BufferView rotationBuffView = new()
            {
                Buffer = buffer,
                Name = $"{BoneName} rot",
                ByteLength = 16 * numKeyFrames,
                ByteOffset = animBuffer.Count + 12 * numKeyFrames + 12 * numKeyFrames,
            };
            gltf.BufferViews.AddRange(new List<BufferView> { scaleBuffView, translationBuffView, rotationBuffView });

            Accessor scaleMatrixAccessor = new()
            {
                BufferView = scaleBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Type = Accessor.TypeEnum.VEC3,
                Name = $"{BoneName} scale",
                Count = numKeyFrames
            };
            Accessor translationMatrixAccessor = new()
            {
                BufferView = translationBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Type = Accessor.TypeEnum.VEC3,
                Name = $"{BoneName} trans",
                Count = numKeyFrames
            };
            Accessor rotationMatrixAccessor = new()
            {
                BufferView = rotationBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Type = Accessor.TypeEnum.VEC4,
                Name = $"{BoneName} rot",
                Count = numKeyFrames
            };
            gltf.Accessors.AddRange(new List<Accessor> { scaleMatrixAccessor, translationMatrixAccessor, rotationMatrixAccessor });

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
            animation.Samplers.AddRange(new List<AnimationSampler> { scaleSampler, translationSampler, rotationSampler });

            //Three channels per bone.
            AnimationChannel scaleChannel = new()
            {
                Sampler = scaleSampler,
                Target = new()
                {
                    Path = AnimationChannelTarget.PathEnum.scale,
                    Node = boneNode
                }
            };
            AnimationChannel translationChannel = new()
            {
                Sampler = translationSampler,
                Target = new()
                {
                    Path = AnimationChannelTarget.PathEnum.translation,
                    Node = boneNode
                }
            };
            AnimationChannel rotationChannel = new()
            {
                Sampler = rotationSampler,
                Target = new()
                {
                    Path = AnimationChannelTarget.PathEnum.rotation,
                    Node = boneNode
                }
            };

            animation.Channels.AddRange(new List<AnimationChannel> { scaleChannel, translationChannel, rotationChannel });

            DynamicByteBuffer transDynamicBuffer = new(12 * numKeyFrames);
            DynamicByteBuffer rotationDynamicBuffer = new(16 * numKeyFrames);

            foreach (var keyFrame in vbodyMotion.BoneKeyFrames)
            {
                Matrix m = keyFrame.Matricies[boneIdx];
                m.Transpose().DecomposeCM(out var translation, out var rotation, out var scale);

                animBuffer.Write(scale);
                transDynamicBuffer.Write(translation);
                rotationDynamicBuffer.Write(rotation);
            }
            animBuffer.Write(transDynamicBuffer.ToArray());
            animBuffer.Write(rotationDynamicBuffer.ToArray());
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
