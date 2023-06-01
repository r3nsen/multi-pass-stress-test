using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace multi_pass_stress_test
{
    partial class GraphicsManager
    {
        GraphicsDevice graphicsD;
        public Effect effect;

        Matrix view;
        Matrix projection;

        VertexPositionColorTexture[] vertex = new VertexPositionColorTexture[6];
        short[] index = new short[4];
        int vertexCount = 0, indexCount = 0;

        RasterizerState rast = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid };

        VertexBuffer vb;
        IndexBuffer ib;

        public RenderTarget2D[] swap_tex = new RenderTarget2D[2];
        public int swap_index = 0;

        public GraphicsManager(GraphicsDevice graphicsDevice)
        {
            graphicsD = graphicsDevice;
            graphicsD.SamplerStates[0] = SamplerState.PointClamp;
            vertex = new VertexPositionColorTexture[4];
            index = new short[6];
            //vb = new VertexBuffer(graphicsD, typeof(VertexPositionColorTexture),4, BufferUsage.WriteOnly);
            //ib = new IndexBuffer(graphicsD, typeof(short), 6, BufferUsage.WriteOnly);
            //graphicsD.SetVertexBuffer(vb);
            //graphicsD.Indices = ib;

        }

        public void begin(Vector2 pos, Vector2 size)
        {
            Vector3 orig = new Vector3(0, 0, 3);
            Vector3 target = Vector3.Zero;
            Vector3 up = Vector3.Up;
            Matrix.CreateLookAt(ref orig, ref target, ref up, out view);
            Matrix.CreateOrthographicOffCenter(pos.X, pos.X + size.X, pos.Y + size.Y, pos.Y, -100, 100, out projection);
            effect.Parameters["WorldViewProjection"].SetValue(view * projection);
            effect.Parameters["tex"].SetValue(FontManager.tex);
            text_effect.Parameters["WorldViewProjection"].SetValue(view * projection);

            if (swap_tex[0] == null)
            {
                swap_tex[0] = new RenderTarget2D(graphicsD, (int)size.X, (int)size.Y);
                swap_tex[1] = new RenderTarget2D(graphicsD, (int)size.X, (int)size.Y);
            }

        }
        public void draw(Vector2 pos, Vector2 size, Color cor)
        {
            vertexCount = indexCount = 0;
            EnsureSpace(6, 4);

            index[indexCount++] = (short)(vertexCount + 0);
            index[indexCount++] = (short)(vertexCount + 1);
            index[indexCount++] = (short)(vertexCount + 2);
            index[indexCount++] = (short)(vertexCount + 1);
            index[indexCount++] = (short)(vertexCount + 3);
            index[indexCount++] = (short)(vertexCount + 2);

            vertex[vertexCount++] = new VertexPositionColorTexture(new Vector3(0, 0, 0), cor, new Vector2(0, 0));
            vertex[vertexCount++] = new VertexPositionColorTexture(new Vector3(1, 0, 0), cor, new Vector2(1, 0));
            vertex[vertexCount++] = new VertexPositionColorTexture(new Vector3(0, 1, 0), cor, new Vector2(0, 1));
            vertex[vertexCount++] = new VertexPositionColorTexture(new Vector3(1, 1, 0), cor, new Vector2(1, 1));

            Matrix world = //Matrix.CreateTranslation(new Vector3(-.5f, -.5f, 0))
                           //* Matrix.CreateRotationZ(MathHelper.PiOver2 * flipX)
                Matrix.CreateScale(new Vector3(size, 1))
                * Matrix.CreateTranslation(new Vector3(pos, 0));

            for (int i = vertexCount - 4; i < vertexCount; i++)
                Vector3.Transform(ref vertex[i].Position, ref world, out vertex[i].Position);

            vb = new VertexBuffer(graphicsD, typeof(VertexPositionColorTexture), 4, BufferUsage.WriteOnly);
            ib = new IndexBuffer(graphicsD, typeof(short), 6, BufferUsage.WriteOnly);

            vb.SetData(vertex);
            ib.SetData(index);

            graphicsD.SetVertexBuffer(vb);
            graphicsD.Indices = ib;

        }
        public void pre_flush(RenderTarget2D srt = null, RenderTarget2D drt = null)
        {
            graphicsD.RasterizerState = rast;
            graphicsD.BlendState = BlendState.NonPremultiplied;
            if (srt != null)
                effect.Parameters["tex"].SetValue(srt);
            graphicsD.SetRenderTarget(drt);

            //if (vertexCount == 0) return;

            effect.CurrentTechnique.Passes[0].Apply();

            //graphicsD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertex, 0, vertexCount, index, 0, indexCount / 3);
            graphicsD.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            //vertexCount = indexCount = 0;
            if (drt != null)
                graphicsD.SetRenderTarget(null);
            print("img.png", drt);
            //flush_text();

        }
        void print(string name, RenderTarget2D tex)
        {
            Stream s = File.Create(name);
            tex.SaveAsPng(s, tex.Width, tex.Height);
            s.Close();
        }
        public void flush()
        {
            graphicsD.RasterizerState = rast;
            graphicsD.BlendState = BlendState.NonPremultiplied;
            effect.Parameters["tex"].SetValue(swap_tex[swap_index++]);
            graphicsD.SetRenderTarget(swap_tex[swap_index %= 2]);

            //if (vertexCount == 0) return;

            effect.CurrentTechnique.Passes[0].Apply();

            //graphicsD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertex, 0, vertexCount, index, 0, indexCount / 3);
            graphicsD.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            //vertexCount = indexCount = 0;
            graphicsD.SetRenderTarget(null);
            //flush_text();
            print("img.png", swap_tex[swap_index]);

        }
        private void EnsureSpace(int indexSpace, int vertexSpace)
        {
            if (indexCount + indexSpace > index.Length)
                Array.Resize(ref index, Math.Max(indexCount + indexSpace, index.Length * 2));
            if (vertexCount + vertexSpace > vertex.Length)
                Array.Resize(ref vertex, Math.Max(vertexCount + vertexSpace, vertex.Length * 2));
        }
    }
}