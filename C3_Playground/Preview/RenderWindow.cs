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

        private BasicEffect basicEffect;

        private Basic3dExampleCamera camera3D;

        private ModelRenderer myModels;

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

            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = Matrix.Identity;
            //basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = true;


            //Camera Setup
            Vector3 cameraPosition = new Vector3(200f, 200f, 0f);
            Vector3 cameraTarget = new Vector3(0, 0, 0);
            //viewMatrix = Matrix.CreateLookAt(new Vector3(200, 0, 0), new Vector3(0, 200, 0), new Vector3(0, 1, 0));
            viewMatrix = Matrix.CreateLookAt(cameraTarget + new Vector3(-1, -1, -1) * cameraPosition, cameraTarget, Vector3.Forward);
            projectionMatrix = Matrix.CreateOrthographic(500, 500, 0.001f, 20000f);


            
            //Load a Mesh.
            C3Model? model = null;
            using (BinaryReader br = new BinaryReader(File.OpenRead(_modelFile == "" ? @"D:\Programming\Conquer\Clients\5165\c3\mesh\002000000.c3" : _modelFile)))
                model = C3ModelLoader.Load(br);

            //C3Model? danceAnimation = null;
            //string animation = @"C:\Program Files (x86)\Conquer Online\Conquer Online 3.0\c3\monster\314\100.C3";
            //using (BinaryReader br = new BinaryReader(File.OpenRead(animation)))
            //    danceAnimation = C3ModelLoader.Load(br);
            //if (model != null && danceAnimation != null)
            //{
            //    Console.WriteLine("Replaced motion with motion file");
            //    model.Animations = danceAnimation.Animations;
            //}

            Texture2D myTexture;
            DDSLib.DDSFromFile(_textureFile == "" ? @"D:\Programming\Conquer\Clients\5165\c3\texture\002000000.dds" : _textureFile, GraphicsDevice, false, out myTexture);



            if(model != null)
            {
                myModels = new(model, GraphicsDevice, myTexture);

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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            basicEffect.World = Matrix.Identity;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;

            //GraphicsDevice.SetVertexBuffer(vertexBuffer);
            //GraphicsDevice.Indices = indexBuffer;

            myModels.Draw(gameTime, basicEffect);

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
