using C3.Core;
using C3.Elements;
using C3.Exports.GLTF.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Xml.Linq;

namespace C3.Exports
{
    using Buffer = C3.Exports.GLTF.Schema.Buffer;

    public class GLTF2ExportOptions
    {
        public required string OutputPath { get; set; }
        public bool ExternalAnimationBuffer { get; set; } = false;
        public string? AnimationOutputRelativeDirectory { get; set; }


    }
    public class GLTF2Exporter
    {
        private readonly GLTF2ExportOptions _options;
        private readonly ILogger _logger;
        private readonly Scene _scene;

        private Dictionary<string, Material> usedMaterials = new();

        private Gltf gltf;

        public GLTF2Exporter(GLTF2ExportOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _scene = new Scene();
            _scene.Nodes = new();

            gltf = new()
            {
                Asset = new() { Version = "2.0", Generator = "Santa's C3 GLTF Exporter" }
            };
            gltf.Nodes = new();

            gltf.Scenes = new()
            {
                _scene
            };
            gltf.Scene = _scene;
        }
        /// <summary>
        /// Adds a skinned mesh to the scene
        /// </summary>
        /// <param name="c3Mesh">Mesh geometry</param>
        /// <param name="c3Moti">Mesh Skin</param>
        /// <param name="texturePath">Relative path to the model texture</param>
        public void WithSkinnedMesh(string name, C3Phy c3Mesh, C3Motion c3Moti, string texturePath)
        {
            //Skinned mesh will have a skeleton node that also needs to be in the scene
            Node node = new()
            {
                Name = name
            };

            gltf!.Nodes!.Add(node);
            gltf!.Scene!.Nodes!.Add(node);

            //Build the skin.
            var skinresults = BuildSkeletonSkin(c3Mesh, c3Moti);

            node.Skin = skinresults.Skin;


            AddSimple(node, c3Mesh, texturePath, true, true);
        }
        public int WithRigidMesh(string name, C3Phy c3Mesh, string texturePath)
        {
            //Skinned mesh will have a skeleton node that also needs to be in the scene
            Node node = new()
            {
                Name = name
            };
            gltf!.Nodes!.Add(node);
            gltf!.Scene!.Nodes!.Add(node);

            AddSimple(node, c3Mesh, texturePath, true, false);
            return node.Index;
        }
        public void WithAnimation(string name, AnimationTrack animTrack)
        {

            if (gltf.Buffers == null) gltf.Buffers = new();
            if (gltf.Animations == null) gltf.Animations = new();
            if (gltf.BufferViews == null) gltf.BufferViews = new();
            if (gltf.Accessors == null) gltf.Accessors = new();

            /* struct[40]
             * [0] Vector3 scale
             * [12] Vector3 translation
             * [24] Quaternion Rotation
             */

            var numberBytes = 40 * animTrack.NodeCount;

            int frameCount = animTrack.FrameCount();
            //Account for time Accessor
            numberBytes += sizeof(float) * frameCount;

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

            Dictionary<int, Accessor> timeAccessors = new();
            var commonTimeMap = animTrack.GetCommonTimeMap();

            //Each node has its own accessors
            foreach (var nodeIdx in animTrack.GetNodeIndices())
            {
                Node? targetNode = gltf?.Nodes?.Where(p=> p.Index == nodeIdx).FirstOrDefault();

                if(targetNode == null)
                {
                    _logger.LogError("Provided animation is targeting a node that doesn't exist.8 Node Index {0}", nodeIdx);
                    continue;
                }

                frameCount = animTrack.FrameCount(nodeIdx);


                var matchingTimeIdx = commonTimeMap[nodeIdx];

                //Check to see if there is already a suitable time track.
                Accessor? timeAccessor = timeAccessors.Where(p => p.Key == matchingTimeIdx).Select(s => s.Value).FirstOrDefault();
                
                if(timeAccessor == null)
                {
                    timeAccessor = BuildTimeAccessor(animTrack, animBuffer, buffer, nodeIdx, out BufferView timeBuffView);

                    timeAccessors.Add(nodeIdx, timeAccessor);
                    pendingAccessors.Add(timeAccessor);
                    pendingBufferViews.Add(timeBuffView);

                    _logger.LogInformation($"Build additional time accessor for node {0}", nodeIdx);
                }



                #region Node Sampler Setup
                BufferView scaleBuffView = new()
                {
                    Buffer = buffer,
                    ByteLength = 12 * frameCount,
                    ByteOffset = animBuffer.Count,
                };
                BufferView translationBuffView = new()
                {
                    Buffer = buffer,
                    ByteLength = 12 * frameCount,
                    ByteOffset = animBuffer.Count + 12 * frameCount,
                };
                BufferView rotationBuffView = new()
                {
                    Buffer = buffer,
                    ByteLength = 16 * frameCount,
                    ByteOffset = animBuffer.Count + 12 * frameCount + 12 * frameCount,
                };
                pendingBufferViews.AddRange(new List<BufferView> { scaleBuffView, translationBuffView, rotationBuffView });

                Accessor scaleMatrixAccessor = new()
                {
                    BufferView = scaleBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC3,
                    Count = frameCount
                };
                Accessor translationMatrixAccessor = new()
                {
                    BufferView = translationBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC3,
                    Count = frameCount
                };
                Accessor rotationMatrixAccessor = new()
                {
                    BufferView = rotationBuffView,
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Type = Accessor.TypeEnum.VEC4,
                    Count = frameCount
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
                        Node = targetNode
                    }
                };
                AnimationChannel translationChannel = new()
                {
                    Sampler = translationSampler,
                    Target = new()
                    {
                        Path = AnimationChannelTarget.PathEnum.translation,
                        Node = targetNode
                    }
                };
                AnimationChannel rotationChannel = new()
                {
                    Sampler = rotationSampler,
                    Target = new()
                    {
                        Path = AnimationChannelTarget.PathEnum.rotation,
                        Node = targetNode
                    }
                };

                animation.Channels.AddRange(new List<AnimationChannel> { scaleChannel, translationChannel, rotationChannel });

                #endregion Node Setup

                DynamicByteBuffer transDynamicBuffer = new(12 * frameCount);
                DynamicByteBuffer rotationDynamicBuffer = new(16 * frameCount);

                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    if(animTrack.DequeueNextFrame(targetNode.Index, out var m))
                    {
                        m.Decompose(out var scale, out var rotation, out var translation);

                        animBuffer.Write(scale);
                        transDynamicBuffer.Write(translation);
                        rotationDynamicBuffer.Write(rotation.Normalize());
                    }
                }
                animBuffer.Write(transDynamicBuffer.ToArray());
                animBuffer.Write(rotationDynamicBuffer.ToArray());
            }

            //Add components to gltf after successfully building arrays.

            gltf.Accessors.AddRange(pendingAccessors);
            gltf.BufferViews.AddRange(pendingBufferViews);

            gltf.Buffers.Add(buffer);
            gltf.Animations.Add(animation);

            buffer.ByteLength = animBuffer.Count;
            buffer.Name = "Animation " + name;
            if (!_options.ExternalAnimationBuffer)
            {
                buffer.Uri = "data:application/gltf-buffer;base64," + animBuffer.ToBase64();
            }
            else
            {
                if (!Directory.Exists(_options.OutputPath))
                {
                    _logger.LogError("OutputPath is required to be a directory that already exists. {0} does not exist.", _options.OutputPath);
                    return;
                }
                string outDir = _options.OutputPath;

                string childPath = $"{name}.bin";
                if(_options.AnimationOutputRelativeDirectory != null)
                    childPath = Path.Combine(_options.AnimationOutputRelativeDirectory, childPath);

                buffer.Uri = childPath;

                outDir = Path.Combine(outDir, childPath);

                if (!Directory.Exists(outDir)) Directory.CreateDirectory(new FileInfo(outDir).Directory.ToString());

                using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(outDir)))
                {
                    bw.Write(animBuffer.ToArray());
                }
            }

        }
        private Accessor BuildTimeAccessor(AnimationTrack track, DynamicByteBuffer animBuffer, Buffer buffer, int nodeIdx, out BufferView timeBuffView)
        {
            int frameCount = track.FrameCount(nodeIdx);
            //Build timeBufferView, time is same for all bones
            float minTime = float.MaxValue;
            float maxTime = float.MinValue;


            foreach (var keyFrame in track.GetFrameIndices(nodeIdx))//using vbody for no specific reason.
            {
                //Write the Time.
                float time = keyFrame * track.FrameTime;

                if (time > maxTime) maxTime = time;
                if (time < minTime) minTime = time;

                animBuffer.Write(time);
            }

            timeBuffView = new()
            {
                Buffer = buffer,
                ByteLength = sizeof(float) * frameCount
            };


            Accessor timeAccessor = new()
            {
                BufferView = timeBuffView,
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Type = Accessor.TypeEnum.SCALAR,
                Count = frameCount,
                Min = new() { minTime },
                Max = new() { maxTime }
            };
            return timeAccessor;

        }
        public void WithPose(AnimationTrack animation)
        {

        }

        public void WithSocket(string name)
        {
            Node node = new()
            {
                Name = name
            };
            gltf!.Nodes!.Add(node);
            gltf!.Scene!.Nodes!.Add(node);
        }
        
        //public void AddAnimation(string name, C3Model model)
        //{
        //    if (gltf.Buffers == null) gltf.Buffers = new();
        //    if (gltf.Animations == null) gltf.Animations = new();
        //    if (gltf.BufferViews == null) gltf.BufferViews = new();
        //    if (gltf.Accessors == null) gltf.Accessors = new();

        //    Dictionary<string, C3Motion> Elements = new();

        //    for (int i = 0; i < model.Animations.Count; i++)
        //    {
        //        if (!elementIndices.TryGetValue(i, out var elementName))
        //        {
        //            _logger.LogWarning("Body doesn't contain an element at index {index}", i);
        //        }
        //        else
        //            Elements.Add(elementName, model.Animations[i]);
        //    }

        //    //Verify there is a motion for each socket.
        //    foreach (var socket in socketNodes)
        //    {
        //        if (!Elements.ContainsKey(socket.Key))
        //        {
        //            _logger.LogError("Provided Animation doesn't contain motion for each socket. Missing {socketName}", socket.Key);
        //            return;
        //        }

        //    }

        //    //Verify each socket has the same number of key frames.
        //    int? numKeyFrames = null;
        //    foreach (var motion in Elements)
        //    {
        //        if (numKeyFrames == null)
        //            numKeyFrames = motion.Value.BoneKeyFrames.Count();
        //        else
        //        {
        //            if (motion.Value.BoneKeyFrames.Count() != numKeyFrames)
        //            {
        //                _logger.LogError("The animation for socket {socket} has a different number of keyFrames", motion.Key); //Not sure if this is an error or just needs to be accommodated
        //                return;
        //            }
        //        }

        //    }

        //    if (numKeyFrames == null)
        //    {
        //        _logger.LogError("This animation {animationName} contains no key frames", name);
        //        return;
        //    }

        //    var skinnedJoints = socketNodes["v_body"].Skin?.Joints;

        //    if (skinnedJoints == null)
        //    {
        //        _logger.LogError("Failed to get joints for the v_body skin");
        //        return;
        //    }

        //    var numberBones = skinnedJoints.Count() + Elements.Count() - 1; //Subtract one to account for v_body.

        //    /* struct[40]
        //     * [0] Vector3 scale
        //     * [12] Vector3 translation
        //     * [24] Quaternion Rotation
        //     */

        //    var numberBytes = 40 * numberBones;

        //    //Account for time Accessor
        //    numberBytes += sizeof(float) * numKeyFrames.Value;

        //    DynamicByteBuffer animBuffer = new(numberBytes);

        //    Buffer buffer = new() { ByteLength = numberBytes };

        //    Animation animation = new()
        //    {
        //        Channels = new(),
        //        Samplers = new(),
        //        Name = name,
        //    };

        //    /* Assumptions - need to test before reducing footprint of bytebuffer.
        //     * KKEY can have scale, rotation, and translation
        //     * ZKEY can have rotation and translation.
        //     * XKEY can have scale and rotation
        //     */

        //    List<Accessor> pendingAccessors = new();
        //    List<BufferView> pendingBufferViews = new();

        //    #region Time Accessor
        //    //Time per frame in seconds
        //    float timePerFrame = 33f / 1000f;//Assumptions - 33ms per frame.

        //    //Build timeBufferView, time is same for all bones
        //    float minTime = float.MaxValue;
        //    float maxTime = float.MinValue;

        //    foreach (var keyFrame in Elements["v_body"].BoneKeyFrames)//using vbody for no specific reason.
        //    {
        //        //Write the Time.
        //        float time = keyFrame.FrameNumber * timePerFrame;

        //        if (time > maxTime) maxTime = time;
        //        if (time < minTime) minTime = time;

        //        animBuffer.Write(time);
        //    }

        //    BufferView timeBuffView = new()
        //    {
        //        Buffer = buffer,
        //        Name = $"frame time",
        //        ByteLength = sizeof(float) * numKeyFrames.Value
        //    };
        //    pendingBufferViews.Add(timeBuffView);

        //    Accessor timeAccessor = new()
        //    {
        //        BufferView = timeBuffView,
        //        ComponentType = Accessor.ComponentTypeEnum.FLOAT,
        //        Type = Accessor.TypeEnum.SCALAR,
        //        Name = $"frame time",
        //        Count = numKeyFrames.Value,
        //        Min = new() { minTime },
        //        Max = new() { maxTime }
        //    };
        //    pendingAccessors.Add(timeAccessor);
        //    #endregion Time Accessor



        //    //Go through each bone and additional joint and add animation key frames.
        //    foreach (var element in Elements)
        //    {
        //        List<Node> boneJoints = new() { socketNodes[element.Key] };

        //        if (element.Key == "v_body") boneJoints = skinnedJoints; // If its v_body we need to use the skinned joints.

        //        if (boneJoints.Count != element.Value.BoneCount)
        //        {
        //            _logger.LogError("The number of bones in the animation doesn't match the number expected from the skeleton.{animationName} Expecting {jointCount} have {bonecount}", name, boneJoints.Count, element.Value.BoneCount);
        //            return;
        //        }

        //        string boneNameStart = element.Key;
        //        for (int i = 0; i < boneJoints.Count; i++)
        //        {
        //            string BoneName = $"{boneNameStart} bone{i}";

        //            Node boneNode = boneJoints[i];

        //            BufferView scaleBuffView = new()
        //            {
        //                Buffer = buffer,
        //                Name = $"{BoneName} scale",
        //                ByteLength = 12 * numKeyFrames.Value,
        //                ByteOffset = animBuffer.Count,
        //            };
        //            BufferView translationBuffView = new()
        //            {
        //                Buffer = buffer,
        //                Name = $"{BoneName} trans",
        //                ByteLength = 12 * numKeyFrames.Value,
        //                ByteOffset = animBuffer.Count + 12 * numKeyFrames.Value,
        //            };
        //            BufferView rotationBuffView = new()
        //            {
        //                Buffer = buffer,
        //                Name = $"{BoneName} rot",
        //                ByteLength = 16 * numKeyFrames.Value,
        //                ByteOffset = animBuffer.Count + 12 * numKeyFrames.Value + 12 * numKeyFrames.Value,
        //            };
        //            pendingBufferViews.AddRange(new List<BufferView> { scaleBuffView, translationBuffView, rotationBuffView });

        //            Accessor scaleMatrixAccessor = new()
        //            {
        //                BufferView = scaleBuffView,
        //                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
        //                Type = Accessor.TypeEnum.VEC3,
        //                Name = $"{BoneName} scale",
        //                Count = numKeyFrames.Value
        //            };
        //            Accessor translationMatrixAccessor = new()
        //            {
        //                BufferView = translationBuffView,
        //                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
        //                Type = Accessor.TypeEnum.VEC3,
        //                Name = $"{BoneName} trans",
        //                Count = numKeyFrames.Value
        //            };
        //            Accessor rotationMatrixAccessor = new()
        //            {
        //                BufferView = rotationBuffView,
        //                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
        //                Type = Accessor.TypeEnum.VEC4,
        //                Name = $"{BoneName} rot",
        //                Count = numKeyFrames.Value
        //            };
        //            pendingAccessors.AddRange(new List<Accessor> { scaleMatrixAccessor, translationMatrixAccessor, rotationMatrixAccessor });

        //            //Three samplers per bone.
        //            AnimationSampler scaleSampler = new()
        //            {
        //                Input = timeAccessor,
        //                Output = scaleMatrixAccessor
        //            };
        //            AnimationSampler translationSampler = new()
        //            {
        //                Input = timeAccessor,
        //                Output = translationMatrixAccessor
        //            };
        //            AnimationSampler rotationSampler = new()
        //            {
        //                Input = timeAccessor,
        //                Output = rotationMatrixAccessor
        //            };
        //            animation.Samplers.AddRange(new List<AnimationSampler> { scaleSampler, translationSampler, rotationSampler });

        //            //Three channels per bone.
        //            AnimationChannel scaleChannel = new()
        //            {
        //                Sampler = scaleSampler,
        //                Target = new()
        //                {
        //                    Path = AnimationChannelTarget.PathEnum.scale,
        //                    Node = boneNode
        //                }
        //            };
        //            AnimationChannel translationChannel = new()
        //            {
        //                Sampler = translationSampler,
        //                Target = new()
        //                {
        //                    Path = AnimationChannelTarget.PathEnum.translation,
        //                    Node = boneNode
        //                }
        //            };
        //            AnimationChannel rotationChannel = new()
        //            {
        //                Sampler = rotationSampler,
        //                Target = new()
        //                {
        //                    Path = AnimationChannelTarget.PathEnum.rotation,
        //                    Node = boneNode
        //                }
        //            };

        //            animation.Channels.AddRange(new List<AnimationChannel> { scaleChannel, translationChannel, rotationChannel });

        //            DynamicByteBuffer transDynamicBuffer = new(12 * numKeyFrames);
        //            DynamicByteBuffer rotationDynamicBuffer = new(16 * numKeyFrames);

        //            foreach (var keyFrame in element.Value.BoneKeyFrames)
        //            {
        //                Matrix m = keyFrame.Matricies[i];
        //                //m.Transpose().DecomposeCM(out var translation, out var rotation, out var scale);
        //                m.Decompose(out var scale, out var rotation, out var translation);

        //                animBuffer.Write(scale);
        //                transDynamicBuffer.Write(translation);
        //                rotationDynamicBuffer.Write(rotation.Normalize());
        //            }
        //            animBuffer.Write(transDynamicBuffer.ToArray());
        //            animBuffer.Write(rotationDynamicBuffer.ToArray());
        //        }
        //    }

        //    //Add components to gltf after successfully building arrays.

        //    gltf.Accessors.AddRange(pendingAccessors);
        //    gltf.BufferViews.AddRange(pendingBufferViews);

        //    gltf.Buffers.Add(buffer);
        //    gltf.Animations.Add(animation);

        //    _logger.LogDebug("Estimated Buffer Length: {totalBytes} - Actual buffer length: {animBufferCount}, Key frame Count: {frameCount}", numberBytes, animBuffer.Count, numKeyFrames.Value);
        //    buffer.ByteLength = animBuffer.Count;
        //    buffer.Uri = "data:application/gltf-buffer;base64," + animBuffer.ToBase64();
        //    buffer.Name = "Animation " + name;
        //}

        private void AddSimple(Node socketNode, C3Phy mesh, string? texturePath = null, bool externalTexture = false, bool skinned = false)
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
                Material? material = null;
                if (!usedMaterials.TryGetValue(texturePath, out material))
                {
                    if (gltf.Images == null) gltf.Images = new();
                    if (gltf.Textures == null) gltf.Textures = new();
                    if (gltf.Materials == null) gltf.Materials = new();
                    Image image = new();
                    if (!externalTexture)
                    {
                        byte[] imBytes = File.ReadAllBytes(texturePath);
                        image.Uri = "data:image/png;base64," + Convert.ToBase64String(imBytes);
                        image.Name = "Texture Image";
                    }
                    else
                    {
                        image.Uri = texturePath;
                        image.Name = "External texture";
                    }

                    Texture texture = new()
                    {
                        Name = "Texture",
                        Source = image
                    };

                    material = new()
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
    
                    usedMaterials.Add(texturePath, material);
                }


                meshPrimitive.Material = material;
            }
            #endregion Texture
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
                Max = new() { max.X, max.Y, max.Z }
            };

            Accessor uvAccessor = new()
            {
                BufferView = verticesBuffView,
                ByteOffset = 12,//Offset for 1x Vector3
                ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                Count = phy.Vertices.Length,
                Type = Accessor.TypeEnum.VEC2,
                Min = new() { minUV.X, minUV.Y },
                Max = new() { maxUV.X, maxUV.Y }
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
                    Type = Accessor.TypeEnum.VEC4
                };

                weightAccessor = new()
                {
                    BufferView = verticesBuffView,
                    ByteOffset = 28,//Offset for 1x Vector3 + 1x Vector2 + 1x Vector4
                    ComponentType = Accessor.ComponentTypeEnum.FLOAT,
                    Count = phy.Vertices.Length,
                    Type = Accessor.TypeEnum.VEC4
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
                Max = new() { (float)phy.Indices.Max() }
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
            gltf.Scene!.Nodes!.Add(commonRoot);

            Dictionary<int, Node> jointNodeMap = new();

            #region Skin
            Skin skin = new() { Joints = new(), Skeleton = commonRoot };
            gltf.Skins.Add(skin);

            for (int i = 0; i < motion.BoneCount; i++)
            {
                //Create a joint nodey
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
