using C3.Elements;

namespace C3.Loaders
{
    public static class C3CameraLoader
    {
        public static C3Camera Load(BinaryReader br)
        {
            C3Camera camera = new C3Camera();

            camera.Name = br.ReadASCIIString(br.ReadUInt32());
            camera.Fov = br.ReadSingle();
            camera.FrameCount = br.ReadUInt32();
            camera.From = new Core.Vector3[camera.FrameCount];
            camera.To = new Core.Vector3[camera.FrameCount];

            for (int i = 0; i < camera.FrameCount; i++)
                camera.From[i] = br.ReadVector3();
            for (int i = 0; i < camera.FrameCount; i++)
                camera.To[i] = br.ReadVector3();

            return camera;
        }
    }
}
