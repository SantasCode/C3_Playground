using C3;
using C3_Playground.Preview.Model;
using Cocona.Filters;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography.X509Certificates;

namespace C3_Playground.Preview
{
    internal class RenderWindow : Game
    {
        private readonly PreviewArgs _previewArgs;
        
        private readonly GraphicsDeviceManager graphicsDeviceManager;

        private VertexPositionColor[] axisLines;

        private Basic3dExampleCamera camera3D;

        private BasicEffect bodyEffect;
        private ModelRenderer armorModel;
        private BasicEffect weaponLEffect;
        private ModelRenderer weaponLModels;
        private BasicEffect weaponREffect;
        private ModelRenderer weaponRModels;
        private BasicEffect mountEffect;
        private ModelRenderer mountModels;
        private BasicEffect armetEffect;
        private ModelRenderer armetModels;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        private readonly string _modelFile;
        private readonly string _textureFile;

        internal RenderWindow(PreviewArgs previewArgs)
        {
            _previewArgs = previewArgs;

            graphicsDeviceManager = new(this)
            {
                PreferredBackBufferWidth = _previewArgs.Width,
                PreferredBackBufferHeight = _previewArgs.Height
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

            armetEffect = new BasicEffect(GraphicsDevice);
            armetEffect.World = Matrix.Identity;
            armetEffect.TextureEnabled = true;


            //Camera Setup
            Vector3 cameraPosition = new Vector3(200f, 200f, 0f);
            Vector3 cameraTarget = new Vector3(0, 0, -150);
            //viewMatrix = Matrix.CreateLookAt(new Vector3(200, 0, 0), new Vector3(0, 200, 0), new Vector3(0, 1, 0));
            viewMatrix = Matrix.CreateLookAt(cameraTarget + new Vector3(-1, -1, -1) * cameraPosition, cameraTarget, Vector3.Forward);
            projectionMatrix = Matrix.CreateOrthographic(500, 500, 0.001f, 20000f);

            //CullClockwise draws the armorModel "mirrored" correctly but looks screwed up.
            //GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;


            C3Model? armorModel = null;
            C3Model? armorAnimation = null;
            Texture2D? armorTexture = null;

            C3Model? armetModel = null;
            Texture2D? armetTexture = null;

            C3Model? weaponLModel = null;
            Texture2D? weaponLTexture = null;

            C3Model? weaponRModel = null;
            Texture2D? weaponRTexture = null;

            C3Model? mountModel = null;
            Texture2D? mountTexture = null;
            C3Model? mountAnimation = null;

            if (_previewArgs.Armor != null && _previewArgs.ArmorTexture != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.Armor)))
                    armorModel = C3ModelLoader.Load(br);

                DDSLib.DDSFromFile(_previewArgs.ArmorTexture, GraphicsDevice, false, out armorTexture);
                bodyEffect.Texture = armorTexture;
            }

            if (_previewArgs.Armet != null && _previewArgs.ArmetTexture != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.Armet)))
                    armetModel = C3ModelLoader.Load(br);

                DDSLib.DDSFromFile(_previewArgs.ArmetTexture, GraphicsDevice, false, out armetTexture);
                armetEffect.Texture = armetTexture;
            }

            if (_previewArgs.LeftWeapon != null && _previewArgs.LeftWeaponTexture!= null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.LeftWeapon)))
                    weaponLModel= C3ModelLoader.Load(br);

                DDSLib.DDSFromFile(_previewArgs.LeftWeaponTexture, GraphicsDevice, false, out weaponLTexture);
                weaponLEffect.Texture = weaponLTexture;
            }

            if (_previewArgs.RightWeapon != null && _previewArgs.RightWeaponTexture != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.RightWeapon)))
                    weaponRModel = C3ModelLoader.Load(br);

                DDSLib.DDSFromFile(_previewArgs.RightWeaponTexture, GraphicsDevice, false, out weaponRTexture);
                weaponREffect.Texture = weaponRTexture;
            }

            if (_previewArgs.Mount != null && _previewArgs.MountTexture != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.Mount)))
                    mountModel = C3ModelLoader.Load(br);

                DDSLib.DDSFromFile(_previewArgs.MountTexture, GraphicsDevice, false, out mountTexture);
                mountEffect.Texture = mountTexture;
            }

            if (_previewArgs.Motion != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.Motion)))
                    armorAnimation = C3ModelLoader.Load(br);
            }

            if (_previewArgs.MountMotion != null)
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(_previewArgs.MountMotion)))
                    mountAnimation = C3ModelLoader.Load(br);
            }

            //Add model associations.
            if (armorModel != null && armorTexture != null)
            {
                if (armorAnimation != null)
                    armorModel.Animations = armorAnimation.Animations;

                this.armorModel = new(armorModel, GraphicsDevice, armorTexture);
            }
            if(armetModel != null && armetTexture != null)
            {
                armetModels = new ModelRenderer(armetModel, GraphicsDevice, armetTexture);
                armetModels.SetParent(0, this.armorModel.NamedParts["v_armet"]);
            }
            if (weaponLModel != null && weaponLTexture != null)
            {
                weaponLModels = new(weaponLModel, GraphicsDevice, weaponLTexture);
                weaponLModels.SetParent(0, this.armorModel.NamedParts["v_l_weapon"]);
            }
            if (weaponRModel != null && weaponRTexture != null)
            {
                weaponRModels = new(weaponRModel, GraphicsDevice, weaponRTexture);
                weaponRModels.SetParent(0, this.armorModel.NamedParts["v_r_weapon"]);
            }
            if (mountModel != null && mountTexture != null)
            {
                if (mountAnimation != null)
                    mountModel.Animations = mountAnimation.Animations;

                mountModels = new(mountModel, GraphicsDevice, mountTexture);
                this.armorModel.SetParentAll(mountModels.NamedParts["v_mount"]);

            }

            TargetElapsedTime = TimeSpan.FromSeconds(1f/24f);
            BlendState blend = new BlendState()
            {
                ColorSourceBlend = Blend.SourceColor,
                //AlphaSourceBlend = Blend.SourceAlpha,
                ColorDestinationBlend = Blend.One,
                //AlphaDestinationBlend = Blend.InverseSourceAlpha,
                ColorBlendFunction = BlendFunction.Add,
            };
            GraphicsDevice.BlendState = blend;
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

            armorModel?.Update(gameTime);
            armetModels?.Update(gameTime);
            weaponLModels?.Update(gameTime);
            weaponRModels?.Update(gameTime);
            mountModels?.Update(gameTime);

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

            armetEffect.World = Matrix.Identity;
            armetEffect.View = viewMatrix;
            armetEffect.Projection = projectionMatrix;


            mountModels?.Draw(gameTime, mountEffect);
            armorModel?.Draw(gameTime, bodyEffect);
            armetModels?.Draw(gameTime, armetEffect);
            weaponLModels?.Draw(gameTime, weaponLEffect);
            weaponRModels?.Draw(gameTime, weaponREffect);

            base.Draw(gameTime);
        }
    }
}
