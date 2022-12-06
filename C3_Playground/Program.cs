using C3;
using C3.Core;
using C3.Exports;
using C3.IniFiles.FileSet;
using C3_Playground.CommandAttributes;
using C3_Playground.Preview.Model;
using Cocona;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace C3_Playground
{
    class Program
    {
        static void Log(string message) => Console.WriteLine(message);
        static void Main(string[] args)
        {
                CoconaApp.Run<Program>(args, options =>
                {
                    options.TreatPublicMethodsAsCommands = false;
                });
        }
        [Command("preview")]
        public void Preview([Argument][FileExists] string file = "", [Argument][FileExists] string textureFile = "", [Option('w')] int Width = 1024, [Option('h')] int Height = 768)
        {
            using (var game = new Preview.RenderWindow(file, textureFile, Width, Height))
                game.Run();
        }

        [Command("test-mesh")]
        public void C3_TestLoadMesh([Argument][FileExists] string file, [Option('v')]bool verbose = false)
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
            {

                C3Model? model = C3ModelLoader.Load(br, verbose);
                if (model == null)
                {
                    Console.WriteLine($"null model file {file}");
                    return;
                }
                List<uint> BoneIndexes = new();
                foreach (var phy in model.Meshs)
                {
                    if (phy.Vertices != null)
                    {
                        foreach (var vert in phy.Vertices)
                        {
                        }
                    }
                    Console.WriteLine($"Phy - {phy.Name}");
                    BoneIndexes.Sort();
                    foreach(var index in BoneIndexes)
                    {
                        Console.WriteLine(index);
                    }
                    Console.WriteLine("Enter to do next");
                    Console.Read();
                }

            }
        }

        [Command("print-info")]
        public void C3_PrintMesh([Argument][FileExists] string file)
        {
            C3Model? myModel;
            using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                myModel = C3ModelLoader.Load(br);

            if (myModel == null) return;

            Console.WriteLine($"{file}");
            Console.WriteLine($"Mesh: {myModel.Meshs.Count} Anim: {myModel.Animations.Count}");
            Console.WriteLine($"Cameras: {myModel.Cameras.Count} Effects: {myModel.Effects.Count}");
            Console.WriteLine($"Shape: {myModel.Shapes.Count} Smotions: {myModel.ShapeMotions.Count}");

            if (myModel.Meshs.Count == myModel.Animations.Count)
                Console.WriteLine($"Equal parts of each.");

            for(int i = 0; i < myModel.Meshs.Count; i++)
            {
                Console.WriteLine($"PHY-->{myModel.Meshs[i].Name}, Bone Count: {myModel.Animations[i].BoneCount}");
            }
            for (int i = 0; i < myModel.Animations.Count; i++)
            {
                Console.WriteLine($"MOTI-->Index: {i} Bone Count: {myModel.Animations[i].BoneCount}");
            }
        }
        [Command("test-meshes")]
        public void C3_TestLoadMeshes([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(fileDir, "*.c3", SearchOption.AllDirectories))
            {
                count++;
                if (verbose) Console.WriteLine($"{file}");
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {file}");
                        continue;
                    }

                    if (model.Meshs.Count != model.Animations.Count)
                        Console.WriteLine($"PHY MOTI Mismatch - {file}");
                    if (model.Effects.Count != model.Cameras.Count)
                        Console.WriteLine($"PTCL CAME Mismatch - {file}");
                    if (model.Effects.Count > 0)
                        Console.WriteLine($"Has Effects - {file}");

                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }
        [Command("test-vbody")]
        public void C3_Testvbody([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            GameData game = new GameData(fileDir);
            var armors = game.GetArmorC3();
            foreach (var armor in armors)
            {
                count++;
                if (verbose) Console.WriteLine($"{armor}");
                using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(fileDir, armor))))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {armor}");
                        continue;
                    }

                    var body = model.Meshs.Where(p => p.Name.ToLower() == "v_body").FirstOrDefault();
                    if (body == null)
                        continue;

                    //Has vbody.
                    if (model.Meshs.Count < 4)
                        Console.WriteLine($"Model has vbody with insuffecient parts. - {new FileInfo(armor).Name}");
                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }
        [Command("test-matrices")]
        public void C3_TestLoadMatrices([Argument][DirectoryExists] string fileDir, [Option('v')] bool verbose = false)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(fileDir, "*.c3", SearchOption.AllDirectories))
            {
                count++;
                using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                {

                    C3Model? model = C3ModelLoader.Load(br, verbose);
                    if (model == null)
                    {
                        Console.WriteLine($"null model file - {file}");
                        continue;
                    }

                    if (model.Meshs.Count > 0)
                    {
                        foreach(var mesh in model.Meshs)
                        {
                            if (!Matrix.Identity.Equals(mesh.InitMatrix))
                                Console.WriteLine($"InitMatrix is not identity: {mesh.Name} - {file}");

                        }
                    }

                }
            }
            Console.WriteLine($"Finished reading {count} c3 files");
        }

        [Command("test-ini")]
        public void Ini_LoadTest([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type == C3.IniFiles.Entities.ItemType.Armor) continue;
                if (item.Type == C3.IniFiles.Entities.ItemType.Helmet) continue;

                foreach (var model in item.BaseModel)
                {
                    C3Model? mesh = null;
                    using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, model.Value.Item1))))
                        mesh = C3ModelLoader.Load(br, false);

                    if (mesh == null) continue;

                    if (mesh.Meshs.Count > 1) 
                        Console.WriteLine($"Item ({item.Name}/{model.Value.Item1}) has more than 1 mesh");
                    


                }
            }
        }
        [Command("convert-dds")]
        public void ConvertToPng([Argument][FileExists] string inputTexture, [Argument] string outputPath)
        {
            PngExporter.Export(File.OpenRead(inputTexture), File.OpenWrite(outputPath));
        }
        [Command("test-1h")]
        public void Ini_Test1h([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.OneHander) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("Multiple models");

                var modelTexturePair = item.BaseModel[0];
                
                C3Model? model = null;
                using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                    model = C3ModelLoader.Load(br, false);

                if (model == null) continue;


                //We only care about 1 of the animation/mesh pairs.
                var animation = model.Animations[0];

                //Need to determine if the key frames are identical.
                Matrix? m = null;
                foreach (var frame in animation.BoneKeyFrames)
                {
                    if (m == null) m = frame.Matricies[0];
                    else
                    {
                        if (!frame.Matricies[0].Equals(m))
                            Console.WriteLine("Frames have different matricies - has some sort of animation");
                    }
                }

                if (m == null)
                {
                    Console.WriteLine("Animation matrix is null");
                }
                else
                {
                    //Multiple the mesh initial matrix by the "animation initial matrix"
                    model.Meshs[0].InitMatrix = Matrix.Multiply(model.Meshs[0].InitMatrix, m);
                }

                //Export the model, if it doesn't exist.
                var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());
                string texturePath = @"C:\Temp\Conquer\Export\items\onehand\texture";
                string modelPath = @"C:\Temp\Conquer\Export\items\onehand";

                string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                texturePath = Path.Combine(texturePath, $"{textureId}.png");

                if (!File.Exists(modelPath))
                {
                    var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                    exporter.AddSimple("1h_weapon", model.Meshs[0], relativePAth, true, false);
                    using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                        exporter.Export(tw);
                }

                if (!File.Exists(texturePath))
                    PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                //Export the texture, if it doesn't exist.
            }
        }
        [Command("test-2h")]
        public void Ini_Test2h([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.TwoHander) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("Multiple models");

                var modelTexturePair = item.BaseModel[0];

                C3Model? model = null;
                using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                    model = C3ModelLoader.Load(br, false);

                if (model == null) continue;


                if (model.Animations.Count() > 1)
                    Console.WriteLine($"2Hand model has more than 1 animation ({model.Animations.Count()}) - {item.Name}");

                if (model.Animations[0].BoneCount > 1)
                    Console.WriteLine($"2Hand model has more than 1 bone ({model.Animations[0].BoneCount}) - {item.Name}");


                //We only care about 1 of the animation/mesh pairs.
                var animation = model.Animations[0];


                //Need to determine if the key frames are identical.
                Matrix? m = null;
                foreach (var frame in animation.BoneKeyFrames)
                {
                    if (m == null) m = frame.Matricies[0];
                    else
                    {
                        if (!frame.Matricies[0].Equals(m))
                            Console.WriteLine("Frames have different matricies - has some sort of animation");
                    }
                }

                if (m == null)
                {
                    Console.WriteLine("Animation matrix is null");
                }
                else
                {
                    //Multiple the mesh initial matrix by the "animation initial matrix"
                    model.Meshs[0].InitMatrix = Matrix.Multiply(model.Meshs[0].InitMatrix, m);
                }

                //Export the model, if it doesn't exist.
                var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());
                string texturePath = @"C:\Temp\Conquer\Export\items\twohand\texture";
                string modelPath = @"C:\Temp\Conquer\Export\items\twohand";

                string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                texturePath = Path.Combine(texturePath, $"{textureId}.png");

                if (!File.Exists(modelPath))
                {
                    var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                    exporter.AddSimple("2h_weapon", model.Meshs[0], relativePAth, true, false);
                    using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                        exporter.Export(tw);
                }

                if (!File.Exists(texturePath))
                    PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                //Export the texture, if it doesn't exist.
            }
        }
        [Command("test-shield")]
        public void Ini_TestShield([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.Shield) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("Multiple models");

                var modelTexturePair = item.BaseModel[0];

                C3Model? model = null;
                using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                    model = C3ModelLoader.Load(br, false);

                if (model == null) continue;


                if (model.Animations.Count() > 1)
                    Console.WriteLine($"Shield model has more than 1 animation ({model.Animations.Count()}) - {item.Name}");

                if (model.Animations[0].BoneCount > 1)
                    Console.WriteLine($"shield model has more than 1 bone ({model.Animations[0].BoneCount}) - {item.Name}");


                //We only care about 1 of the animation/mesh pairs.
                var animation = model.Animations[0];


                //Need to determine if the key frames are identical.
                Matrix? m = null;
                foreach (var frame in animation.BoneKeyFrames)
                {
                    if (m == null) m = frame.Matricies[0];
                    else
                    {
                        if (!frame.Matricies[0].Equals(m))
                            Console.WriteLine("Frames have different matricies - has some sort of animation");
                    }
                }

                if (m == null)
                {
                    Console.WriteLine("Animation matrix is null");
                }
                else
                {
                    //Multiple the mesh initial matrix by the "animation initial matrix"
                    model.Meshs[0].InitMatrix = Matrix.Multiply(model.Meshs[0].InitMatrix, m);
                }

                //Export the model, if it doesn't exist.
                var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());
                string texturePath = @"C:\Temp\Conquer\Export\items\shield\texture";
                string modelPath = @"C:\Temp\Conquer\Export\items\shield";

                string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                texturePath = Path.Combine(texturePath, $"{textureId}.png");

                if (!File.Exists(modelPath))
                {
                    var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                    exporter.AddSimple("shield", model.Meshs[0], relativePAth, true, false);
                    using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                        exporter.Export(tw);
                }

                if (!File.Exists(texturePath))
                    PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                //Export the texture, if it doesn't exist.
            }
        }
        [Command("test-armet")]
        public void Ini_TestArmet([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.Helmet) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("Multiple models");

                foreach (var baseModel in item.BaseModel)
                {
                    var modelTexturePair = baseModel.Value;

                    C3Model? model = null;
                    using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                        model = C3ModelLoader.Load(br, false);

                    if (model == null) continue;


                    if (model.Animations.Count() > 1)
                        Console.WriteLine($"armet model has more than 1 animation ({model.Animations.Count()}) - {item.Name}");

                    if (model.Animations[0].BoneCount > 1)
                        Console.WriteLine($"armet model has more than 1 bone ({model.Animations[0].BoneCount}) - {item.Name}");


                    //We only care about 1 of the animation/mesh pairs.
                    var animation = model.Animations[0];


                    //Need to determine if the key frames are identical.
                    Matrix? m = null;
                    foreach (var frame in animation.BoneKeyFrames)
                    {
                        if (m == null) m = frame.Matricies[0];
                        else
                        {
                            if (!frame.Matricies[0].Equals(m))
                                Console.WriteLine("Frames have different matricies - has some sort of animation");
                        }
                    }

                    if (m == null)
                    {
                        Console.WriteLine("Animation matrix is null");
                    }
                    else
                    {
                        //Multiple the mesh initial matrix by the "animation initial matrix"
                        model.Meshs[0].InitMatrix = Matrix.Multiply(model.Meshs[0].InitMatrix, m);
                    }

                    //Export the model, if it doesn't exist.
                    var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());
                    string texturePath = @"C:\Temp\Conquer\Export\items\armet\texture";
                    string modelPath = @"C:\Temp\Conquer\Export\items\armet";

                    string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                    string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                    modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                    texturePath = Path.Combine(texturePath, $"{textureId}.png");

                    if (!File.Exists(modelPath))
                    {
                        var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                        exporter.AddSimple("armet", model.Meshs[0], relativePAth, true, false);
                        using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                            exporter.Export(tw);
                    }

                    if (!File.Exists(texturePath))
                        PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                    //Export the texture, if it doesn't exist.
                }
            }
        }
        [Command("test-armor")]
        public void Ini_TestArmor([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetItems();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.Armor) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("Multiple models");

                foreach (var baseModel in item.BaseModel)
                {
                    var modelTexturePair = baseModel.Value;

                    C3Model? model = null;
                    using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                        model = C3ModelLoader.Load(br, false);

                    if (model == null) continue;


                    if (model.Animations.Count() > 1)
                        Console.WriteLine($"armor model has more than 1 animation ({model.Animations.Count()}) - {item.Name}");

                    if (model.Animations[0].BoneCount > 1)
                        Console.WriteLine($"armor model has more than 1 bone ({model.Animations[0].BoneCount}) - {item.Name}");

                    var bodyMesh = model.Meshs.Where(p => p.Name == "v_body").FirstOrDefault();

                    if (bodyMesh == null)
                    {
                        Console.WriteLine("Failed to find v_body");
                        continue;
                    }

                    int bodyIdx = model.Meshs.IndexOf(bodyMesh);

                    //We only care about 1 of the animation/mesh pairs.
                    var animation = model.Animations[bodyIdx];


                    //Need to determine if the key frames are identical.
                    Matrix? m = null;
                    foreach (var frame in animation.BoneKeyFrames)
                    {
                        if (m == null) m = frame.Matricies[bodyIdx];
                        else
                        {
                            if (!frame.Matricies[0].Equals(m))
                                Console.WriteLine("Frames have different matricies - has some sort of animation");
                        }
                    }

                    if (m == null)
                    {
                        Console.WriteLine("Animation matrix is null");
                    }
                    else
                    {
                        //Multiple the mesh initial matrix by the "animation initial matrix"
                        model.Meshs[bodyIdx].InitMatrix = Matrix.Multiply(model.Meshs[bodyIdx].InitMatrix, m);
                    }

                    //Export the model, if it doesn't exist.
                    var exporter = new GLTF2Export(ConsoleAppLogger.CreateLogger<Program>());
                    string texturePath = @"C:\Temp\Conquer\Export\items\armor\texture";
                    string modelPath = @"C:\Temp\Conquer\Export\items\armor";

                    string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                    string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                    modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                    texturePath = Path.Combine(texturePath, $"{textureId}.png");

                    if (!File.Exists(modelPath))
                    {
                        var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                        exporter.AddSimple("armor", model.Meshs[bodyIdx], relativePAth, true, false);
                        using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                            exporter.Export(tw);
                    }

                    if (!File.Exists(texturePath))
                        PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                    //Export the texture, if it doesn't exist.
                }
            }
        }
        [Command("export-obj")]
        public void Export_obj([Argument][FileExists] string filePath, [Argument] string outputPath)
        {
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath)))
                model = C3ModelLoader.Load(br);

            if(model != null)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
                using (TextWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    ObjExporter.Export(model, tw);
            }
        }

        [Command("export-gltf2")]
        public void Export_gltf2([Argument][FileExists] string filePath, [Argument][FileExists] string texturePath, [Argument] string outputPath)
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

                //C3Model? armorModel = null;
                //using (BinaryReader br = new BinaryReader(File.OpenRead(@"D:\Programming\Conquer\Clients\5165\c3\mesh\002131000.c3")))
                //    armorModel = C3ModelLoader.Load(br);
                //if (armorModel != null) 
                //{
                //    var armorPhy = armorModel.Meshs.Where(p => p.Name.ToLower() == "v_body").FirstOrDefault();
                //    if (armorPhy != null)
                //        exporter.AddToSocket("v_body", armorPhy, @"C:\Temp\Conquer\002131300.png");
                //}

                C3Model? weaponModel = null;
                using (BinaryReader br = new BinaryReader(File.OpenRead(@"D:\Programming\Conquer\Clients\5165\c3\mesh\410280.C3")))
                    weaponModel = C3ModelLoader.Load(br);
                if (weaponModel != null)
                {
                    //Figure out the initial matrix
                    //Need to determine if the key frames are identical.
                    Matrix? m = null;
                    foreach (var frame in weaponModel.Animations[0].BoneKeyFrames)
                    {
                        if (m == null) m = frame.Matricies[0];
                        else
                        {
                            if (!frame.Matricies[0].Equals(m))
                                Console.WriteLine("Frames have different matricies - has some sort of animation");
                        }
                    }
                    weaponModel.Meshs[0].InitMatrix = Matrix.Multiply(model.Meshs[0].InitMatrix, m);
                    var weaponPhy = weaponModel.Meshs[0];
                    if (weaponPhy != null)
                    {
                        exporter.AddToSocket("v_r_weapon", weaponPhy, @"C:\Temp\Conquer\410285.png");
                        exporter.AddToSocket("v_l_weapon", weaponPhy, @"C:\Temp\Conquer\410285.png");
                    }
                }

                //foreach (var file in Directory.GetFiles(@"D:\Programming\Conquer\Clients\5165\c3\0004\410"))
                //{
                //    C3Model? newModel = new();
                //    using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
                //        newModel = C3ModelLoader.Load(br);
                //    string fileName = new FileInfo(file).Name;
                //    if (newModel != null)
                //        exporter.AddAnimation(fileName, newModel);
                //}

                using (StreamWriter tw = new StreamWriter(File.OpenWrite(outputPath)))
                    exporter.Export(tw);
            }
        }

        [Command("export-json")]
        public void Export_json([Argument][FileExists] string filePath, [Argument] string outputPath)
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