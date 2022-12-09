using C3.Exports;
using C3;
using C3_Playground.CommandAttributes;
using Cocona;
using System.Text.Json;

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
