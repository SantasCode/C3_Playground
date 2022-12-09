using C3;
using C3.Core;
using C3.Exports;
using C3_Playground.CommandAttributes;
using Cocona;

namespace C3_Playground
{
    [HasSubCommands(typeof(Commands.Export))]
    [HasSubCommands(typeof(Commands.Test))]
    [HasSubCommands(typeof(Commands.PreviewCommands), commandName:"preview")]
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



        [Command("convert-dds")]
        public void ConvertToPng([Argument][FileExists] string inputTexture, [Argument] string outputPath)
        {
            PngExporter.Export(File.OpenRead(inputTexture), File.OpenWrite(outputPath));
        }
        
    }
}