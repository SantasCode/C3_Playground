using C3;
using C3_Playground.Preview.Model;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography.X509Certificates;

namespace C3_Playground.Preview
{
    internal class RenderWindow : Game
    {
        private GraphicsDeviceManager graphicsDeviceManager;

        private VertexPositionColor[] axisLines;


        private Basic3dExampleCamera camera3D;

        private BasicEffect bodyEffect;
        private ModelRenderer myModels;
        private BasicEffect weaponLEffect;
        private ModelRenderer weaponLModels;
        private BasicEffect weaponREffect;
        private ModelRenderer weaponRModels;
        private BasicEffect mountEffect;
        private ModelRenderer mountModels;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        private IndexBuffer indexBuffer;
        private VertexBuffer vertexBuffer;

        private readonly string _modelFile;
        private readonly string _textureFile;

        internal RenderWindow(string modelFile = "", string textureFile = "", int windowWidth = 1024, int windowHeight = 768)
        {
            _modelFile = modelFile;
            _textureFile = textureFile;

            graphicsDeviceManager = new(this)
            {
                PreferredBackBufferWidth = windowWidth,
                PreferredBackBufferHeight = windowHeight
            };
        }

        protected override void LoadContent()
        {
            axisLines = new VertexPositionColor[6];
            axisLines[0] = new VertexPositionColor(new Vector3(-1000, 0, 0), Color.Red);
            axisLines[1] = new VertexPositionColor(new Vector3(1000, 0, 0), Color.Red);
            axisLines[2] = new VertexPositionColor(new Vector3(0, -1000, 0), Color.Green);
            axisLines[3] = new VertexPositionColor(new Vector3(0, 1000, 0), Color.Green);
            axisLines[4] = new VertexPositionColor(new Vector3(0, 0, -1000), Color.Blue);
            axisLines[5] = new VertexPositionColor(new Vector3(0, 0, 1000), Color.Blue);

            camera3D = new(GraphicsDevice, this.Window);
            camera3D.TargetPositionToLookAt = new Vector3(0, 0, 0);
            camera3D.Position = new Vector3(100, 0, 0);

            bodyEffect = new BasicEffect(GraphicsDevice);
            bodyEffect.World = Matrix.Identity;
            bodyEffect.TextureEnabled = true;
            
            weaponLEffect = new BasicEffect(GraphicsDevice);
            weaponLEffect.World = Matrix.Identity;
            weaponLEffect.TextureEnabled = true;
            
            weaponREffect = new BasicEffect(GraphicsDevice);
            weaponREffect.World = Matrix.Identity;
            weaponREffect.TextureEnabled = true;

            mountEffect = new BasicEffect(GraphicsDevice);
            mountEffect.World = Matrix.Identity;
            mountEffect.TextureEnabled = true;


            //Camera Setup
            Vector3 cameraPosition = new Vector3(200f, 200f, 0f);
            Vector3 cameraTarget = new Vector3(0, 0, -150);
            //viewMatrix = Matrix.CreateLookAt(new Vector3(200, 0, 0), new Vector3(0, 200, 0), new Vector3(0, 1, 0));
            viewMatrix = Matrix.CreateLookAt(cameraTarget + new Vector3(-1, -1, -1) * cameraPosition, cameraTarget, Vector3.Forward);
            projectionMatrix = Matrix.CreateOrthographic(500, 500, 0.001f, 20000f);


            
            //Load a Mesh.
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(_modelFile == "" ? @"D:\Programming\Conquer\Clients\5165\c3\mesh\002000000.c3" : _modelFile)))
                model = C3ModelLoader.Load(br);

            C3Model? modelAnimation = null;
            string animation = @"D:\Programming\Conquer\Clients\5579\c3\1002\000\100.C3";
            using (BinaryReader br = new BinaryReader(File.OpenRead(animation)))
                modelAnimation = C3ModelLoader.Load(br);
            if (model != null && modelAnimation != null)
            {
                Console.WriteLine("Replaced motion with motion file");
                model.Animations = modelAnimation.Animations;
            }

            Texture2D myTexture;
            DDSLib.DDSFromFile(_textureFile == "" ? @"D:\Programming\Conquer\Clients\5165\c3\texture\002000000.dds" : _textureFile, GraphicsDevice, false, out myTexture);
            bodyEffect.Texture = myTexture;

            ///Left Weapon
            C3Model? weaponLModel = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(@"D:\Programming\Conquer\Clients\5165\c3\mesh\410210.c3")))
                weaponLModel = C3ModelLoader.Load(br);

            Texture2D weaponLTexture;
            DDSLib.DDSFromFile(@"D:\Programming\Conquer\Clients\5165\c3\texture\410216.dds", GraphicsDevice, false, out weaponLTexture);
            weaponLEffect.Texture = weaponLTexture;


            ///Right Weapon
            C3Model? weaponRModel = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(@"D:\Programming\Conquer\Clients\5165\c3\mesh\480280.c3")))
                weaponRModel = C3ModelLoader.Load(br);

            Texture2D weaponRTexture;
            DDSLib.DDSFromFile(@"D:\Programming\Conquer\Clients\5165\c3\texture\480285.dds", GraphicsDevice, false, out weaponRTexture);
            weaponREffect.Texture = weaponRTexture;



            ///Mount
            C3Model? mountModel = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(@"D:\Programming\Conquer\Clients\5579\c3\Mount\819\8190000.C3")))
                mountModel = C3ModelLoader.Load(br);

            Texture2D mountTexture;
            DDSLib.DDSFromFile(@"D:\Programming\Conquer\Clients\5579\c3\Mount\819\8190000.dds", GraphicsDevice, false, out mountTexture);
            mountEffect.Texture = mountTexture;
            
            C3Model? mountAnimation = null;
            string mountAnim = @"D:\Programming\Conquer\Clients\5579\c3\Mount\819\100.C3";
            using (BinaryReader br = new BinaryReader(File.OpenRead(mountAnim)))
                mountAnimation = C3ModelLoader.Load(br);
            if (mountModel != null && mountAnimation != null)
            {
                Console.WriteLine("Replaced motion with motion file");
                mountModel.Animations = mountAnimation.Animations;
            }


            if (model != null)
            {
                myModels = new(model, GraphicsDevice, myTexture);

            }
            if (weaponLModel != null)
            {
                weaponLModels = new(weaponLModel, GraphicsDevice, weaponLTexture);
                weaponLModels.SetParent(0, myModels.NamedParts["v_l_weapon"]);
            }
            if (weaponRModel != null)
            {
                weaponRModels = new(weaponRModel, GraphicsDevice, weaponRTexture);
                weaponRModels.SetParent(0, myModels.NamedParts["v_r_weapon"]);
            }
            if (mountModel != null)
            {
                mountModels = new(mountModel, GraphicsDevice, mountTexture);
                myModels.SetParentAll(mountModels.NamedParts["v_mount"]);
            }
            TargetElapsedTime = TimeSpan.FromSeconds(1f/30f);
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();


            #region 3d Model Rotation
            float dx = 0, dy = 0, dz = 0;
            if (state.IsKeyDown(Keys.A))
                dx -= 0.02f;
            if (state.IsKeyDown(Keys.D))
                dx += 0.02f;
            if (state.IsKeyDown(Keys.W))
                dy -= 0.02f;
            if (state.IsKeyDown(Keys.S))
                dy += 0.02f;
            if (state.IsKeyDown(Keys.Q))
                dz -= 0.02f;
            if (state.IsKeyDown(Keys.E))
                dz += 0.02f;
            viewMatrix = Matrix.CreateRotationX(dx * MathHelper.PiOver4) * Matrix.CreateRotationY(dy * MathHelper.PiOver4) * Matrix.CreateRotationZ(dz * MathHelper.PiOver4) * viewMatrix;
            #endregion 3d Model Rotation

            myModels.Update(gameTime);
            weaponLModels.Update(gameTime);
            weaponRModels.Update(gameTime);
            mountModels.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            bodyEffect.World = Matrix.Identity;
            bodyEffect.View = viewMatrix;
            bodyEffect.Projection = projectionMatrix;

            weaponLEffect.World = Matrix.Identity;
            weaponLEffect.View = viewMatrix;
            weaponLEffect.Projection = projectionMatrix;

            weaponREffect.World = Matrix.Identity;
            weaponREffect.View = viewMatrix;
            weaponREffect.Projection = projectionMatrix;

            mountEffect.World = Matrix.Identity;
            mountEffect.View = viewMatrix;
            mountEffect.Projection = projectionMatrix;

            //GraphicsDevice.SetVertexBuffer(vertexBuffer);
            //GraphicsDevice.Indices = indexBuffer;

            myModels.Draw(gameTime, bodyEffect);
            weaponLModels.Draw(gameTime, weaponLEffect);
            weaponRModels.Draw(gameTime, weaponREffect);
            mountModels.Draw(gameTime, mountEffect);

            //basicEffect.VertexColorEnabled = true;

            //foreach (var pass in basicEffect.CurrentTechnique.Passes)
            //{
            //    pass.Apply();
            //    //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount);
            //    GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, axisLines, 0, 3);
            //}
            base.Draw(gameTime);
        }
    }
}
