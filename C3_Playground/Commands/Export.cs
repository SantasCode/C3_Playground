using C3.Exports;
using C3;
using C3_Playground.CommandAttributes;
using Cocona;
using System.Text.Json;
using C3.Elements;
using Microsoft.Extensions.Logging;
using C3.Core;
using System.Net;

namespace C3_Playground.Commands
{
    [HasSubCommands(typeof(BatchExport), commandName: "batch", Description = "Batch converts a given item type to gltf2 file format")]
    internal class Export
    {
        [Command(Description = "Exports the object mesh to an obj file format.")]
        public void Obj([Argument][FileExists] string filePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                using (TextWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    ObjExporter.Export(model, tw);
            }
        }

        [Command(Description = "Exports the object mesh, texture, and animations to a gltf2 file format.")]
        public void Gltf2([Argument][FileExists] string filePath, [Argument][FileExists] string texturePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());

                exporter.AddBody(model, texturePath);
                exporter.AddAnimation("pose 1", model);

                string rootDir = @"D:\Programming\Conquer\Clients\5165\c3\0001\";
                foreach (var file in Directory.GetFiles(rootDir, "*.c3", SearchOption.AllDirectories))
                {
                    C3Model? newModel = new();
                    using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                        newModel = C3ModelLoader.Load(br);
                    string fileName = Path.GetRelativePath(rootDir, file).Replace(".c3", "").Replace(".C3", "");
                    if (newModel != null)
                        exporter.AddAnimation(fileName, newModel);
                }

                using (StreamWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    exporter.Export(tw);
            }
        }

        [Command(Description = "Exports armor texture to a png and armor mesh to a gltf2 file format.")]
        public void Body([Argument][FileExists] string filePath, [Argument][FileExists] string texturePath, [Argument] string outputPath)
        {
            var logger = ConsoleAppLogger.CreateLogger<Program>();

            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                GLTF2ExportOptions options = new()
                {
                    OutputPath = "",
                    AnimationOutputRelativeDirectory = "animation",
                    ExternalAnimationBuffer = true
                };
                var exporter = new GLTF2Exporter(options, ConsoleAppLogger.CreateLogger<GLTF2Exporter>());

                //A body has a skinned mesh and several named sockets.
                //We want to add the skinned mesh first.

                List<(C3Phy mesh, C3Motion moti)> Elements = new();
                for(int i = 0; i < model.Meshs.Count; i ++)
                {
                    Elements.Add((model.Meshs[i], model.Animations[i]));
                }
                var bodyPair = Elements.Where(p => p.mesh.Name == "v_body").FirstOrDefault();

                if(bodyPair.moti == null)
                {
                    logger.LogError("Expect a body model to contain a 'v_body' element");
                    return;
                }

                exporter.WithSkinnedMesh(bodyPair.mesh.Name, bodyPair.mesh, bodyPair.moti, texturePath);

                foreach(var pair in Elements)
                {
                    if (pair.mesh.Name == "v_body") continue;

                    exporter.WithSocket(pair.mesh.Name);
                }

                using (StreamWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    exporter.Export(tw);
            }
        }

        [Command(Description = "Exports effect texture to a png and efffect mesh to a gltf2 file format.")]
        public void Effect([Argument][FileExists] string filePath, [Argument][FileExists] string texturePath)
        {
            var logger = ConsoleAppLogger.CreateLogger<Program>();

            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                string textureDir = @"C:\Temp\Conquer\Export\effects\texture";
                string modelPath = @"C:\Temp\Conquer\Export\effects";

                string meshId = new FileInfo(filePath).Name.Replace(new FileInfo(filePath).Extension, "");
                string textureId = new FileInfo(texturePath).Name.Replace(new FileInfo(texturePath).Extension, "");

                modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                var newTexturePath = Path.Combine(textureDir, $"{textureId}.png");

                string relativeTexturePath = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), newTexturePath);

                if (File.Exists(modelPath))
                    File.Delete(modelPath);

                GLTF2ExportOptions options = new()
                {
                    OutputPath = @"C:\Temp\Conquer\Export\effects",
                    AnimationOutputRelativeDirectory = "anim",
                    ExternalAnimationBuffer = true
                };
                var exporter = new GLTF2Exporter(options, ConsoleAppLogger.CreateLogger<GLTF2Exporter>());

                //A body has a skinned mesh and several named sockets.
                //We want to add the skinned mesh first.

                List<(C3Phy mesh, C3Motion moti)> Elements = new();
                
                AnimationTrack animationTrack = new(ConsoleAppLogger.CreateLogger<AnimationTrack>());

                
                for (int i = 0; i < model.Meshs.Count; i++)
                    Elements.Add((model.Meshs[i], model.Animations[i]));

                foreach (var pair in Elements)
                {
                    var nodeIdx = exporter.WithRigidMesh(pair.mesh.Name, pair.mesh, relativeTexturePath);
                    animationTrack.PopulateFrames(nodeIdx, pair.moti);
                }
                exporter.WithAnimation("default", animationTrack);

                using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                    exporter.Export(tw);

                if (!File.Exists(newTexturePath))
                    PngExporter.Export(File.OpenRead(texturePath), File.OpenWrite(newTexturePath));
            }
        }

        [Command(Description = "Exports the original c3 schema to a json file.")]
        public void Json([Argument][FileExists] string filePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if (model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                string jsonText = JsonSerializer.Serialize(model, new JsonSerializerOptions() { WriteIndented = true });
                using (TextWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    tw.Write(jsonText);
            }
        }
    }
}
