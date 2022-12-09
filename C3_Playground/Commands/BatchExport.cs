using C3.Exports;
using C3;
using C3.Core;
using C3_Playground.CommandAttributes;
using Cocona;

namespace C3_Playground.Commands
{
    internal class BatchExport
    {
        [Command(Description = "Exports all one handed items defined in itemtype.dat that have valid models/textures")]
        public void Onehand([Argument][DirectoryExists] string clientDirectory)
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

        [Command(Description = "Exports all two handed items defined in itemtype.dat that have valid models/textures. Excludes bows")]
        public void Twohand([Argument][DirectoryExists] string clientDirectory)
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

        [Command(Description = "Exports all shield items defined in itemtype.dat that have valid models/textures")]
        public void Shield([Argument][DirectoryExists] string clientDirectory)
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

        [Command(Description = "Exports all helmet items defined in itemtype.dat that have valid models/textures")]
        public void Armet([Argument][DirectoryExists] string clientDirectory)
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

        [Command(Description = "Exports all hair defined in armet.ini that have valid models/textures")]
        public void Hair([Argument][DirectoryExists] string clientDirectory)
        {
            GameData game = new GameData(clientDirectory);
            var items = game.GetHair();

            foreach (var item in items)
            {
                if (item.Type != C3.IniFiles.Entities.ItemType.Hair) continue;

                if (item.BaseModel.Count > 1)
                    Console.WriteLine("[Hair] Multiple models");

                foreach (var baseModel in item.BaseModel)
                {
                    var modelTexturePair = baseModel.Value;

                    C3Model? model = null;
                    using (BinaryReader br = new BinaryReader(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item1))))
                        model = C3ModelLoader.Load(br, false);

                    if (model == null) continue;


                    if (model.Animations.Count() > 1)
                        Console.WriteLine($"hair model has more than 1 animation ({model.Animations.Count()}) - {item.Name}");

                    if (model.Animations[0].BoneCount > 1)
                        Console.WriteLine($"hair model has more than 1 bone ({model.Animations[0].BoneCount}) - {item.Name}");


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
                    string texturePath = @"C:\Temp\Conquer\Export\items\hair\texture";
                    string modelPath = @"C:\Temp\Conquer\Export\items\hair";

                    string meshId = new FileInfo(modelTexturePair.Item1).Name.Replace(new FileInfo(modelTexturePair.Item1).Extension, "");
                    string textureId = new FileInfo(modelTexturePair.Item2).Name.Replace(new FileInfo(modelTexturePair.Item2).Extension, "");

                    modelPath = Path.Combine(modelPath, $"{meshId}.gltf");
                    texturePath = Path.Combine(texturePath, $"{textureId}.png");

                    if (!File.Exists(modelPath))
                    {
                        var relativePAth = Path.GetRelativePath(new FileInfo(modelPath).Directory.ToString(), texturePath);
                        exporter.AddSimple("hair", model.Meshs[0], relativePAth, true, false);
                        using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                            exporter.Export(tw);
                    }

                    if (!File.Exists(texturePath))
                        PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                    //Export the texture, if it doesn't exist.
                }
            }
        }

        [Command(Description = "Exports all armor items defined in itemtype.dat that have valid models/textures")]
        public void Armor([Argument][DirectoryExists] string clientDirectory)
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

                    var bodyMesh = model.Meshs.Where(p => p.Name == "v_body").FirstOrDefault();

                    if (bodyMesh == null)
                    {
                        Console.WriteLine("Failed to find v_body");
                        continue;
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
                        //exporter.AddSimple("armor", model.Meshs[bodyIdx], relativePAth, true, false);
                        exporter.AddBody(model, relativePAth, true, true);
                        //exporter.AddBody(model, texturePath, false, true);
                        using (StreamWriter tw = new StreamWriter(File.OpenWrite(modelPath)))
                            exporter.Export(tw);
                    }

                    if (!File.Exists(texturePath))
                        PngExporter.Export(File.OpenRead(Path.Combine(clientDirectory, modelTexturePair.Item2)), File.OpenWrite(texturePath));
                    //Export the texture, if it doesn't exist.
                }
            }
        }
    }
}
