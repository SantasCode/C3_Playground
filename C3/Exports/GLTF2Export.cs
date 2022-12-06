using C3.Core;
using C3.Elements;
using C3.Exports.GLTF.Schema;
using IniParser.Format;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buffer = C3.Exports.GLTF.Schema.Buffer;

namespace C3.Exports
{
    public class GLTF2Export
    {
        private readonly ILogger _logger;
        private Gltf gltf;

        //Do I need this or can I just get from the gltf.nodes collection by name?
        private Dictionary<string, Node> socketNodes;
        private Dictionary<int, string> elementIndices;

        public GLTF2Export(ILogger logger)
        {
            _logger = logger;
            gltf = new()
            {
                Asset = new() { Version = "2.0", Generator = "C3 GLTF Exporter" }
            };
            socketNodes = new();
        }
        /// <summary>
        /// Creates a new scene with the meshed node for v_body, and socket nodes for other elements.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="bodyTexturePath"></param>
        public void AddBody(C3Model model, string? bodyTexturePath = null)
        {
            if (gltf.Nodes == null) gltf.Nodes = new();
            if(gltf.Scenes == null) gltf.Scenes = new();

            if (model.Meshs.Count != model.Animations.Count) _logger.LogWarning("Body model provided doesn't have the same number of motions as meshes");

            //Make phy/moti pairings.
            Dictionary<string, (C3Phy, C3Motion)> Elements = new();
            elementIndices = new();

            for (int i = 0; i < model.Meshs.Count; i++)
            {
                Elements.Add(model.Meshs[i].Name.ToLower(), (model.Meshs[i], model.Animations[i]));
                elementIndices.Add(i, model.Meshs[i].Name.ToLower());
            }

            if (!Elements.ContainsKey("v_body"))
            {
                _logger.LogError("Body model provided doesn't have a v_body element");
                return;
            }

            //Create an overall container Node, a new scene and add the node to the scene.
            Node topLevel = new()
            {
                Name = "sockets",
                Children = new(),
                Rotation = new Quaternion(1, 0, 0, 1).Normalize().ToArray(),
            };
            gltf.Nodes.Add(topLevel);

            Scene scene = new()
            {
                Nodes = new() { topLevel }
            };
            gltf.Scenes.Add(scene);

            //Set this scene as the default scene.
            gltf.Scene = scene;

            //Build up the v_body skeleton, if undefined. Skeleton/mesh nodes should always be first.
            //Handle v_body, its the only skinned mesh (i think)
            (C3Phy vbodyPhy, C3Motion vbodyMotion) = Elements["v_body"];

            var skinResults = BuildSkeletonSkin(vbodyPhy, vbodyMotion);

            //Skeleton needs to be a child of topLevel per glTF spec
            topLevel.Children.Add(skinResults.Skin.Skeleton);

            Node vbodyNode = new()
            {
                Skin = skinResults.Skin,
                Name = "v_body"
            };
            //Add this node to the scene, because its skinned.
            gltf.Scene.Nodes.Add(vbodyNode);

            gltf.Nodes.Add(vbodyNode);
            socketNodes.Add(vbodyNode.Name, vbodyNode);

            //Add the base mesh for vbody.
            AddToSocket("v_body", vbodyPhy, bodyTexturePath);


            //Add nodes for other elements
            foreach(var element in Elements)
            {
                if (element.Key == "v_body") continue;

                Node velementNode = new()
                {
                    Name = element.Key
                };

                var m = element.Value.Item2.BoneKeyFrames[0].Matricies[0];
                m.Decompose(out var scale, out var rotation, out var translation);

                velementNode.Scale = scale.ToArray();
                velementNode.Rotation = rotation.ToArray();
                velementNode.Translation = translation.ToArray();

                gltf.Nodes.Add(velementNode);
                topLevel.Children.Add(velementNode);
                socketNodes.Add(velementNode.Name, velementNode);
            }
        }
        public void AddSimple(string name, C3Phy mesh, string? texturePath = null, bool skinned = false)
        {
            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Scenes == null) gltf.Scenes = new();

            Node node = new()
            {
                Name = name
            };

            if (gltf.Scene == null)
            {
                Scene scene = new()
                {
                    Nodes = new()
                    {
                        node
                    }
                };
                gltf.Scenes.Add(scene);
                gltf.Scene = scene;
            }
            else if(gltf.Scene.Nodes == null)
                gltf.Scene.Nodes = new() { node };
            else
                gltf.Scene.Nodes.Add(node);

            gltf.Nodes.Add(node);  

            AddSimple(node, mesh, texturePath, skinned);
        }
        private void AddSimple(Node socketNode, C3Phy mesh, string? texturePath = null, bool skinned = false)
        {
            if (gltf.Meshes == null) gltf.Meshes = new();

            //If its v_body, it has a skin and will have joints/weights.
            var geoResults = AddGeometry(mesh, skinned);

            MeshPrimitive meshPrimitive = new()
            {
                Attributes = new()
                {
                    { "POSITION", geoResults.Vertices },
                    { "TEXCOORD_0", geoResults.UVs }
                },
                Indices = geoResults.Indices
            };

            if (geoResults.Joints != null)
                meshPrimitive.Attributes.Add("JOINTS_0", geoResults.Joints);
            if (geoResults.Weights != null)
                meshPrimitive.Attributes.Add("WEIGHTS_0", geoResults.Weights);

            Mesh gltfMesh = new() { Primitives = new() { meshPrimitive }, Name = socketNode.Name };

            var existingMesh = gltf.Meshes.Where(p => p.Name == socketNode.Name).FirstOrDefault();
            if (existingMesh != null)
            {
                //Replace the existing mesh.
                existingMesh.Primitives = gltfMesh.Primitives;
            }
            else
            {
                gltf.Meshes.Add(gltfMesh);
                socketNode.Mesh = gltfMesh;
            }

            #region Texture
            //TODO - Remove existing texture/image/material when replacing a socket
            if (texturePath != null)
            {
                if (gltf.Images == null) gltf.Images = new();
                if (gltf.Textures == null) gltf.Textures = new();
                if (gltf.Materials == null) gltf.Materials = new();

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

                meshPrimitive.Material = material;
            }
            #endregion Texture
        }
        public void AddToSocket(string socket, C3Phy mesh, string? texturePath = null)
        {
            if (!socketNodes.ContainsKey(socket))
            {
                _logger.LogError("{socket} is not a valid socket name or no body has been set.", socket);
                return;
            }

            var socketNode = socketNodes[socket];

            //If its v_body, it has a skin and will have joints/weights.
            bool skinned = socket == "v_body";

            AddSimple(socketNode, mesh, texturePath, skinned);
        }

        public void AddAnimation(string name, C3Model model)
        {
            if(gltf.Buffers == null) gltf.Buffers = new();
            if(gltf.Animations == null) gltf.Animations = new();
            if(gltf.BufferViews == null) gltf.BufferViews = new();
            if(gltf.Accessors == null) gltf.Accessors = new();

            Dictionary<string, C3Motion> Elements = new();

            for (int i = 0; i < model.Animations.Count; i++)
            {
                if(!elementIndices.TryGetValue(i, out var elementName))
                {
                    _logger.LogWarning("Body doesn't contain an element at index {index}", i);
                }
                else
                    Elements.Add(elementName, model.Animations[i]);
            }

            //Verify there is a motion for each socket.
            foreach(var socket in socketNodes)
            {
                if (!Elements.ContainsKey(socket.Key))
                {
                    _logger.LogError("Provided Animation doesn't contain motion for each socket. Missing {socketName}", socket.Key);
                    return;
                }

            }

            //Verify each socket has the same number of key frames.
            int? numKeyFrames = null;
            foreach(var motion in Elements)
            {
                if (numKeyFrames == null)
                    numKeyFrames = motion.Value.BoneKeyFrames.Count();
                else
                {
                    if(motion.Value.BoneKeyFrames.Count() != numKeyFrames)
                    {
                        _logger.LogError("The animation for socket {socket} has a different number of keyFrames", motion.Key); //Not sure if this is an error or just needs to be accommodated
                        return;
                    }
                }

            }

            if(numKeyFrames == null)
            {
                _logger.LogError("This animation {animationName} contains no key frames", name);
                return;
            }

            var skinnedJoints = socketNodes["v_body"].Skin?.Joints;

            if(skinnedJoints == null)
            {
                _logger.LogError("Failed to get joints for the v_body skin");
                return;
            }

            var numberBones = skinnedJoints.Count() + Elements.Count() - 1; //Subtract one to account for v_body.

            /* struct[40]
             * [0] Vector3 scale
             * [12] Vector3 translation
             * [24] Quaternion Rotation
             */

            var numberBytes = 40 * numberBones;

            //Account for time Accessor
            numberBytes += sizeof(float) * numKeyFrames.Value;

            DynamicByteBuffer animBuffer = new(numberBytes);

            Buffer buffer = new() { ByteLength = numberBytes };

            Animation animation = new()
            {
                Channels = new(),
                Samplers = new(),
                Name = name,
            };

            /* Assumptions - need to test before reducing footprint of bytebuffer.
             * KKEY can have scale, rotation, and translation
             * ZKEY can have rotation and translation.
             * XKEY can have scale and rotation
             */
            
            List<Accessor> pendingAccessors = new();
            List<BufferView> pendingBufferViews = new();

            #region Time Accessor
            //Time per frame in seconds
            float timePerFrame = 33f / 1000f;//Assumptions - 33ms per frame.

            //Build timeBufferView, time is same for all bones
            float minTime = float.MaxValue;
            float maxTime = float.MinValue;

            foreach (var keyFrame in Elements["v_body"].BoneKeyFrames)//using vbody for no specific reason.
            {
                //Write the Time.
                float time = keyFrame.FrameNumber * timePerFrame;

                if (time > maxTime) maxTime = time;
                if (time < minTime) minTime = time;

                animBuffer.Write(time);
            }

            BufferView timeBuffView = new()
            {
                Buffer = buffer,
                Name = $"frame time",
                ByteLength = sizeof(float) * numKeyFrames.Value
            };
            pendingBufferViews.Add(timeBuffView);

            Accessor timeAccessor = new()
            {
                BufferView = timeBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Type = Accessor.TypeEnum.SCALAR,
                Name = $"frame time",
                Count = numKeyFrames.Value,
                Min = new() { minTime },
                Max = new() { maxTime }
            };
            pendingAccessors.Add(timeAccessor);
            #endregion Time Accessor



            //Go through each bone and additional joint and add animation key frames.
            foreach(var element in Elements)
            {
                List<Node> boneJoints = new() { socketNodes[element.Key] };

                if (element.Key == "v_body") boneJoints = skinnedJoints; // If its v_body we need to use the skinned joints.

                if(boneJoints.Count != element.Value.BoneCount)
                {
                    _logger.LogError("The number of bones in the animation doesn't match the number expected from the skeleton.{animationName} Expecting {jointCount} have {bonecount}",name, boneJoints.Count, element.Value.BoneCount);
                    return;
                }

                string boneNameStart = element.Key;
                for (int i = 0; i < boneJoints.Count; i++)
                {
                    string BoneName = $"{boneNameStart} bone{i}";

                    Node boneNode = boneJoints[i];

                    BufferView scaleBuffView = new()
                    {
                        Buffer = buffer,
                        Name = $"{BoneName} scale",
                        ByteLength = 12 * numKeyFrames.Value,
                        ByteOffset = animBuffer.Count,
                    };
                    BufferView translationBuffView = new()
                    {
                        Buffer = buffer,
                        Name = $"{BoneName} trans",
                        ByteLength = 12 * numKeyFrames.Value,
                        ByteOffset = animBuffer.Count + 12 * numKeyFrames.Value,
                    };
                    BufferView rotationBuffView = new()
                    {
                        Buffer = buffer,
                        Name = $"{BoneName} rot",
                        ByteLength = 16 * numKeyFrames.Value,
                        ByteOffset = animBuffer.Count + 12 * numKeyFrames.Value + 12 * numKeyFrames.Value,
                    };
                    pendingBufferViews.AddRange(new List<BufferView> { scaleBuffView, translationBuffView, rotationBuffView });

                    Accessor scaleMatrixAccessor = new()
                    {
                        BufferView = scaleBuffView,
                        ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                        Type = Accessor.TypeEnum.VEC3,
                        Name = $"{BoneName} scale",
                        Count = numKeyFrames.Value
                    };
                    Accessor translationMatrixAccessor = new()
                    {
                        BufferView = translationBuffView,
                        ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                        Type = Accessor.TypeEnum.VEC3,
                        Name = $"{BoneName} trans",
                        Count = numKeyFrames.Value
                    };
                    Accessor rotationMatrixAccessor = new()
                    {
                        BufferView = rotationBuffView,
                        ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                        Type = Accessor.TypeEnum.VEC4,
                        Name = $"{BoneName} rot",
                        Count = numKeyFrames.Value
                    };
                    pendingAccessors.AddRange(new List<Accessor> { scaleMatrixAccessor, translationMatrixAccessor, rotationMatrixAccessor });

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

                    foreach (var keyFrame in element.Value.BoneKeyFrames)
                    {
                        Matrix m = keyFrame.Matricies[i];
                        //m.Transpose().DecomposeCM(out var translation, out var rotation, out var scale);
                        m.Decompose(out var scale, out var rotation, out var translation);

                        animBuffer.Write(scale);
                        transDynamicBuffer.Write(translation);
                        rotationDynamicBuffer.Write(rotation.Normalize());
                    }
                    animBuffer.Write(transDynamicBuffer.ToArray());
                    animBuffer.Write(rotationDynamicBuffer.ToArray());
                }
            }

            //Add components to gltf after successfully building arrays.

            gltf.Accessors.AddRange(pendingAccessors);
            gltf.BufferViews.AddRange(pendingBufferViews);

            gltf.Buffers.Add(buffer);
            gltf.Animations.Add(animation);

            _logger.LogDebug("Estimated Buffer Length: {totalBytes} - Actual buffer length: {animBufferCount}, Key frame Count: {frameCount}", numberBytes, animBuffer.Count, numKeyFrames.Value);
            buffer.ByteLength = animBuffer.Count;
            buffer.Uri = "data:application/gltf-buffer;base64," + animBuffer.ToBase64();
            buffer.Name = "Animation " + name;
        }

        public void Export(StreamWriter sw)
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            sw.Write(JsonSerializer.Serialize(gltf, jsonSerializerOptions));
        }

        private record AddGeometryResults(Accessor Vertices, Accessor Indices, Accessor UVs, Accessor? Joints, Accessor? Weights);
        private AddGeometryResults AddGeometry(C3Phy phy, bool skinned = false)
        {
            if (gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Accessors == null) gltf.Accessors = new();

            /* Vertex info struct - skinned.
             * struct[44]{
             *  [0] Vector3 Position
             *  [12] Vector2 UV
             *  [20] ushort[4] Joint
             *  [28]float[4] Weight
             *}
             */
            /* Vertex info struct - not-skinned.
             * struct[20]{
             *  [0] Vector3 Position
             *  [12] Vector2 UV
             *}
             */
            DynamicByteBuffer c3geometryBuffer;
            int vectorBufferSize = 0;
            if (skinned)
                vectorBufferSize = (phy.Vertices.Length * 44);
            else
                vectorBufferSize = (phy.Vertices.Length * 20);

            var geoBufferSize = vectorBufferSize + phy.Indices.Count() * sizeof(ushort);//Index array size
            c3geometryBuffer = new(geoBufferSize);

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

                if (skinned)
                {
                    c3geometryBuffer.Write(new ushort[] { (ushort)phyVertex.BoneWeights[0].Joint, (ushort)phyVertex.BoneWeights[1].Joint, 0, 0 });
                    c3geometryBuffer.Write(new float[] { phyVertex.BoneWeights[0].Weight, phyVertex.BoneWeights[1].Weight, 0, 0 });
                }
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
                ByteStride = 20
            };
            if (skinned)
                verticesBuffView.ByteStride = 44;

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

            Accessor? jointAccessor = null;
            Accessor? weightAccessor = null;
            if (skinned)
            {
                jointAccessor = new()
                {
                    BufferView = verticesBuffView,
                    ByteOffset = 20,//Offset for 1x Vector3 + 1x Vector2
                    ComponentType = Accessor.ComponentTypeEnum.UNSIGNED_SHORT,
                    Count = phy.Vertices.Length,
                    Type = Accessor.TypeEnum.VEC4,
                    Name = "joint accessor"
                };

                weightAccessor = new()
                {
                    BufferView = verticesBuffView,
                    ByteOffset = 28,//Offset for 1x Vector3 + 1x Vector2 + 1x Vector4
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Count = phy.Vertices.Length,
                    Type = Accessor.TypeEnum.VEC4,
                    Name = "weight accessor"
                };
            }

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
            gltf.Accessors.AddRange(new List<Accessor> { indicesAccessor, verticesAccessor, uvAccessor });
            if (jointAccessor != null && weightAccessor != null)
            {
                gltf.Accessors.AddRange(new List<Accessor> { jointAccessor, weightAccessor });
            }

            return new AddGeometryResults(verticesAccessor, indicesAccessor, uvAccessor, jointAccessor, weightAccessor);
        }
        private class BuildSkinResults
        {
            public required Skin Skin { get; init; }
            public required ReadOnlyDictionary<int, Node> JointNodeMap { get; init; }
        }
        private BuildSkinResults BuildSkeletonSkin(C3Phy mesh, C3Motion motion)
        {
            if (gltf.Nodes == null) gltf.Nodes = new();
            if (gltf.Skins == null) gltf.Skins = new();

            Node commonRoot = new()
            {
                Name = "skeleton",
                Children = new(),
            };
            gltf.Nodes.Add(commonRoot);


            Dictionary<int, Node> jointNodeMap = new();

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
                jointNodeMap.Add(i, jNode);

            }

            #endregion Skin

            return new BuildSkinResults() { Skin = skin, JointNodeMap = new(jointNodeMap) };
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
