namespace C3.Exports.GLTF.Schema
{
    internal class Sampler : IndexedItem
    {
        public MagFilterEnum? MagFilter { get; set; }
        public MinFilterEnum? MinFilter { get; set; }
        public WrapSEnum? WrapS { get; set; }
        public WrapTEnum? WrapT { get; set; }
        public string? Name { get; set; }
        public Extension? Extensions { get; set; }
        public Extra? Extras { get; set; }


        public enum MagFilterEnum
        {
            NEAREST = 9728,
            LINEAR = 9729,
        }

        public enum MinFilterEnum
        {
            NEAREST = 9728,
            LINEAR = 9729,
            NEAREST_MIPMAP_NEAREST = 9984,
            LINEAR_MIPMAP_NEAREST = 9985,
            NEAREST_MIPMAP_LINEAR = 9986,
            LINEAR_MIPMAP_LINEAR = 9987,
        }

        public enum WrapSEnum
        {
            CLAMP_TO_EDGE = 33071,
            MIRRORED_REPEAT = 33648,
            REPEAT = 10497,
        }

        public enum WrapTEnum
        {
            CLAMP_TO_EDGE = 33071,
            MIRRORED_REPEAT = 33648,
            REPEAT = 10497,
        }
    }
}
