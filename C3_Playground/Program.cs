using C3;
using C3.Core;
using C3.Exports;
using C3_Playground.CommandAttributes;
using Cocona;

namespace C3_Playground
{
    [HasSubCommands(typeof(Commands.Export))]
    [HasSubCommands(typeof(Commands.Test))]
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



        [Command("convert-dds")]
        public void ConvertToPng([Argument][FileExists] string inputTexture, [Argument] string outputPath)
        {
            PngExporter.Export(File.OpenRead(inputTexture), File.OpenWrite(outputPath));
        }
        
    }
}