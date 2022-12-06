using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace C3.Exports
{
    public static class PngExporter
    {
        private static BcDecoder _decoder = new();
        public static void Export(Stream input, Stream output)
        {
            using (Image<Rgba32> Image = _decoder.DecodeToImageRgba32(input))
            {
                var encoder = Image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);

                Image.Save(output, encoder);
            }
        }
    }
}
