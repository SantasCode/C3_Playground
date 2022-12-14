using C3;
using C3.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3_Playground.Preview.Model
{
    internal class ModelRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;

        public Dictionary<string, Model> NamedParts { get; init; }

        private readonly bool isBody;
        private readonly bool isMount;
        public bool IsBody => isBody;
        public bool IsMount => isMount;

        public ModelRenderer(C3Model c3Model, GraphicsDevice graphicsDevice, Texture2D texture)
        {
            if (c3Model.Meshs.Count != c3Model.Animations.Count) throw new Exception("Number of meshes does not match the number of motions. Need to analyze");
            
            _graphicsDevice = graphicsDevice;
            
            NamedParts = new();
            for (int i = 0; i < c3Model.Meshs.Count; i++)
            {
                NamedParts.Add(c3Model.Meshs[i].Name, new Model(c3Model.Meshs[i], c3Model.Animations[i], _graphicsDevice, texture));
            }

            isBody = NamedParts.ContainsKey("v_body");
            isMount = NamedParts.ContainsKey("v_mount");
        }
        public void Update(GameTime gameTime)
        {
            foreach (var model in NamedParts.Values)
                model.Update(gameTime);
        }
        public void Draw(GameTime gameTime, Effect basicEffect)
        {
            if (IsBody)
                NamedParts["v_body"].Draw(gameTime, basicEffect);
            else if (IsMount)
                NamedParts["v_mount"].Draw(gameTime, basicEffect);
            else
                foreach (var model in NamedParts.Values)
                    model.Draw(gameTime, basicEffect);
        }

        public void SetParent(int partIndex, Model model) => NamedParts.ElementAt(partIndex).Value.Parent = model;
        public void SetParent(string partName, Model model) => NamedParts[partName].Parent = model;
        public void SetParentAll(Model model)
        {
            foreach(var part in NamedParts.Values)
                part.Parent = model;
        }
    }

    internal class Model
    { 
        private readonly GraphicsDevice _graphicsDevice;
        private readonly C3Phy _c3Phy;
        public IndexBuffer IndexBuffer { get; init; }
        

        private VertexPositionTexture[] vertices;
        private VertexBuffer vertexBuffer;
        private bool vertexBufferDirty = true;
        public VertexBuffer VertexBuffer
        {
            get
            {
                if (vertexBufferDirty)
                {
                    vertexBuffer.SetData<VertexPositionTexture>(vertices);
                    vertexBufferDirty = false;
                    return vertexBuffer;
                }
                return vertexBuffer;
            }
        }
       
        public Skeleton Skeleton { get; init; }//TODO: Get rid of skeleton, might be overkill.

        public Motion BaseMotion { get; set; }

        public Motion? ActiveMotion { get; set; }

        public Texture2D Texture { get; set; }

        public Model? Parent { get; set; }


        public Model(C3Phy c3Phy, C3Motion c3Motion, GraphicsDevice graphicsDevice, Texture2D texture)
        {
            _graphicsDevice = graphicsDevice;
            _c3Phy = c3Phy;
            Texture = texture;

            #region Build Geometry
            IndexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, c3Phy.Indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<ushort>(c3Phy.Indices);

            vertices = new VertexPositionTexture[c3Phy.Vertices.Length];
            for (int i = 0; i < c3Phy.Vertices.Length; i++)
            {
                vertices[i] = new()
                {
                    TextureCoordinate = new Vector2(c3Phy.Vertices[i].U, c3Phy.Vertices[i].V),
                    Position = new Vector3(c3Phy.Vertices[i].Position.X, c3Phy.Vertices[i].Position.Y, c3Phy.Vertices[i].Position.Z)
                };
            }
            vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), vertices.Length, BufferUsage.WriteOnly);
            #endregion

            //Build bone relation. 
            Skeleton = new(c3Phy);

            //Set Base Motion
            BaseMotion = new(c3Motion, c3Phy.InitMatrix);
        }

        public void Update(GameTime gameTime)
        {
            //Perform calcs on vertices. Are transforms applied to the previously calculated or base?

            //Going to update Frame each call, going to be way to fast.
            bool changed = BaseMotion.NextFrame();


            if (changed)
            {
                vertexBufferDirty = true;
                foreach (var bone in Skeleton.BoneStore)
                {
                    if (Skeleton.TryGetBoneVertices(bone.Key, out var boneVertices))
                    {
                        foreach (var vertexIdx in boneVertices)
                        {
                            if (vertexIdx.Item2 == 0) 
                                continue;
                            vertices[vertexIdx.Item1] = new VertexPositionTexture()
                            {
                                TextureCoordinate = new Vector2(_c3Phy.Vertices[vertexIdx.Item1].U, _c3Phy.Vertices[vertexIdx.Item1].V),
                                Position = CalculateVertex(new Vector3(_c3Phy.Vertices[vertexIdx.Item1].Position.X, _c3Phy.Vertices[vertexIdx.Item1].Position.Y, _c3Phy.Vertices[vertexIdx.Item1].Position.Z),
                                    GetTransform(bone.Key),
                                    vertexIdx.Item2)
                            };
                        }
                    }
                }
            }
        }

        public void Draw( GameTime gameTime, Effect basicEffect)
        {

            _graphicsDevice.SetVertexBuffer(VertexBuffer);
            _graphicsDevice.Indices = IndexBuffer;

            foreach (var pass in ((AlphaTestEffect)basicEffect).CurrentTechnique.Passes)
            {
                pass.Apply();
            }
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount);
        }

        public Vector3 CalculateVertex(Vector3 vertex, Matrix transform, float weight)
        {
            //It appears that the weight does not have an effect...not used in the eu client. Renders incorrectly when using weight.

            //Need to get the parents parent transform, if it exists. Etc.
            var result = Vector3.Transform(vertex, transform); //Matrix.Multiply(transform, weight));
            return result;
        }

        public Matrix GetTransform(uint BoneIndex)
        {
            Matrix parentTransform = Parent?.GetTransform(0) ?? Matrix.Identity;
            return Matrix.Multiply(BaseMotion.GetMatrix(BoneIndex), parentTransform);
        }
    }
}
