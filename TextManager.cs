using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace multi_pass_stress_test
{
    class FontManager
    {
        public class Atlas
        {
            public string type;
            public int distanceRange;
            public float size;
            public int width;
            public int height;
            public string yOrigin;
        }
        public class Metrics
        {
            public float emSize;
            public float lineHeight;
            public float ascender;
            public float descender;
            public float underlineY;
            public float underlineThickness;
        }
        public class Bounds
        {
            public float left;
            public float bottom;
            public float right;
            public float top;
        }
        public class Glyphs
        {
            public int unicode;
            public float advance;
            public Bounds planeBounds;
            public Bounds atlasBounds;
        }
        public class Font
        {
            public Atlas atlas;
            public Metrics metrics;
            public List<Glyphs> glyphs;
            public List<string> kerning;
        }

        public static Font font;
        public static Dictionary<int, Glyphs> glyphs = new Dictionary<int, Glyphs>();
        public static Texture2D tex;
        public static void LoadFont(Texture2D t, string d)
        {
            char c = (char)(int)'y'; // ! = 33, y = 121
            tex = t;
            font = JsonConvert.DeserializeObject<Font>(File.ReadAllText(d));
            foreach (var p in font.glyphs) glyphs.Add(p.unicode, p);
        }
    }
    partial class GraphicsManager
    {
        public enum Pivo
        {
            TopLeft,
            TopCenter,
            TopRight,

            BottomLeft,
            BottomCenter,
            BottomRight,

            MiddleLeft,
            MiddleCenter,
            MiddleRight,
        }

        VertexPositionColorTexture[] text_vertex = new VertexPositionColorTexture[1];
        short[] text_index = new short[1];
        int text_vertexCount = 0, text_indexCount = 0;

        public Effect text_effect;

        public float DrawString(string text, Vector2 pos, float animation, float _size = 1, bool rtl = false, int lineLen = 1000, Pivo pivo = Pivo.BottomLeft, int stringAnimationId = -1)
        {
            Vector2 offset = Vector2.Zero;
            bool firstGlyph = true;
            float size = FontManager.font.atlas.size;
            int counter = 0;
            int xpos = 0;
            int ypos = 0;
            float stringlen = 0;
            switch (pivo)
            {
                case Pivo.TopLeft:
                    xpos = 0;
                    ypos = 0;
                    break;
                case Pivo.TopCenter:
                    xpos = 1;
                    ypos = 0;
                    break;
                case Pivo.TopRight:
                    xpos = 2;
                    ypos = 0;
                    break;
                case Pivo.MiddleLeft:
                    xpos = 0;
                    ypos = 1;
                    break;
                case Pivo.MiddleCenter:
                    xpos = 1;
                    ypos = 1;
                    break;
                case Pivo.MiddleRight:
                    xpos = 2;
                    ypos = 1;
                    break;
                case Pivo.BottomLeft:
                    xpos = 0;
                    ypos = 2;
                    break;
                case Pivo.BottomCenter:
                    xpos = 1;
                    ypos = 2;
                    break;
                case Pivo.BottomRight:
                    xpos = 2;
                    ypos = 2;
                    break;
            }
            float temp = 0;
            foreach (char cc in text)
            {
                if (cc != '\n')
                    stringlen += FontManager.glyphs[cc].advance;
            }
            switch (xpos)
            {
                case 0: break;
                case 1:

                    offset.X -= stringlen / 2 * _size * size;
                    break;
                case 2:
                    offset.X -= stringlen * _size * size;
                    break;
            }
            float letterSize = FontManager.font.metrics.ascender - FontManager.font.metrics.descender;
            switch (ypos)
            {
                case 0:
                    offset.Y -=
                        (FontManager.glyphs[text[0]].planeBounds.bottom -
                        FontManager.glyphs[text[0]].planeBounds.top) *
                        _size * size;
                    break;
                case 1:
                    offset.Y -=
                        (FontManager.glyphs[text[0]].planeBounds.bottom -
                        FontManager.glyphs[text[0]].planeBounds.top) *
                        _size * size * .5f;
                    break;
                case 2: break;
            }
            for (int i = 0; i < text.Length; i++)
            {

                char c = text[i];
                if (counter >= lineLen)
                {
                    counter -= lineLen;
                    c = '\n';
                    --i;
                }
                ++counter;
                if (c == '\r') continue;
                if (c == '\n')
                {
                    offset.X = 0;
                    //sprfont.LineSpacing
                    //sprfont.LineSpacing * _size;
                    //offset.Y += (size + 1) * _size; // hardcoded offset de + 1                    
                    offset.Y += FontManager.font.metrics.lineHeight * size * _size;
                    firstGlyph = true;
                    continue;
                }
                if (!FontManager.glyphs.ContainsKey(c)) c = '.';
                var glyph = FontManager.glyphs[c];//sprfont.GetGlyphs()[c];
                if (c == ' ')
                {
                    offset.X += glyph.advance * _size * size;
                    continue;
                }
                if (firstGlyph)
                {
                    //offset.X = Math.Max(glyph.LeftSideBearing, 0) * _size;
                    firstGlyph = false;
                }
                else
                {
                    //offset.X += (sprfont.Spacing /*+ glyph.LeftSideBearing*/) * _size;
                }
                Vector2 currentOff = offset;
                //currentOff.X += glyph.planeBounds.left;//(0/*glyph.Cropping.X*/) * _size;
                //currentOff.Y += glyph.planeBounds.top;//(0/*glyph.Cropping.Y*/) * _size;

                float Size = size * _size;

                Matrix transform = Matrix.Identity;

                transform.M11 = Size;
                transform.M22 = Size;
                transform.M41 = pos.X * 0;
                transform.M42 = pos.Y * 0;

                //offset.X += //(glyph.Width + glyph.RightSideBearing) * _size;

                //ref var o = ref sdfObjects[(int)EffectType.Text];
                Text_EnsureSpace(6, 4);

                int colorID = 0; float packedData = 0; int z = 0; byte flipX = 0; byte flipY = 0;

                float fw = Size, fh = Size;

                //float _x = glyph.BoundsInTexture.X, _y = glyph.BoundsInTexture.Y,
                //    _w = glyph.BoundsInTexture.Width, _h = glyph.BoundsInTexture.Height;//sprfont.Texture.Width, _h = sprfont.Texture.Height;
                float _x = glyph.atlasBounds.left,
                    _h = FontManager.font.atlas.height - glyph.atlasBounds.bottom,
                    _w = glyph.atlasBounds.right,
                    _y = FontManager.font.atlas.height - glyph.atlasBounds.top;


                //_x /= sprfont.Texture.Width;
                //_y /= sprfont.Texture.Height;
                //_w /= sprfont.Texture.Width;
                //_h /= sprfont.Texture.Height;
                _x /= FontManager.font.atlas.width;
                _y /= FontManager.font.atlas.height;
                _w /= FontManager.font.atlas.width;
                _h /= FontManager.font.atlas.height;
                //////DEBUG
                //float off = .02f;
                //_x -= off;
                //_y -= off;
                //_w += off * 2;
                //_h += off * 2;
                float f = 1111f;

                //int indx = colorIndex % colorsManager.backG.Length;
                //Vector3 data = colorsManager.playr.ToVector3();// new Vector3(colorID, size.Y, packedData);
                ////Vector4 cor = Color.White.ToVector4();
                ////data.X = glyph.atlasBounds.left - .5f + (FontManager.font.atlas.height - glyph.atlasBounds.top - .5f) * 1000;
                ////data.Y = glyph.atlasBounds.right - .5f + (FontManager.font.atlas.height - glyph.atlasBounds.bottom - .5f) * 1000;
                //cor.Z = animation - i * .2f;
                // a - abs(x - 4) * speed
                ////data.Z = animation - Math.Abs((i - text.Length / 2) * .2f);

                text_index[text_indexCount++] = (short)(text_vertexCount + 0);
                text_index[text_indexCount++] = (short)(text_vertexCount + 1);
                text_index[text_indexCount++] = (short)(text_vertexCount + 2);
                text_index[text_indexCount++] = (short)(text_vertexCount + 1);
                text_index[text_indexCount++] = (short)(text_vertexCount + 3);
                text_index[text_indexCount++] = (short)(text_vertexCount + 2);

                text_vertex[text_vertexCount++] = new VertexPositionColorTexture(new Vector3(0, 0, z), Color.White, new Vector2(_x, _y));
                text_vertex[text_vertexCount++] = new VertexPositionColorTexture(new Vector3(1, 0, z), Color.White, new Vector2(_w, _y));
                text_vertex[text_vertexCount++] = new VertexPositionColorTexture(new Vector3(0, 1, z), Color.White, new Vector2(_x, _h));
                text_vertex[text_vertexCount++] = new VertexPositionColorTexture(new Vector3(1, 1, z), Color.White, new Vector2(_w, _h));
                float w = glyph.planeBounds.right - glyph.planeBounds.left, h = glyph.planeBounds.top - glyph.planeBounds.bottom;
                currentOff.X += glyph.planeBounds.left * Size;
                currentOff.Y -= glyph.planeBounds.bottom * Size;
                offset.X += glyph.advance * Size;//w *Size;
                //if (stringAnimationId == -1)
                //{
                //    float v = (float)(Math.PI * data.Z * 2 - Math.PI);
                //    currentOff.Y += (float)Math.Sin(v) / v * 3 * _size;
                //}
                //else
                //{
                //    currentOff.Y += stringanimation[stringAnimationId].waveArray[i] * _size;
                //}

                Matrix world = Matrix.CreateTranslation(new Vector3(-.0f, -1f, 0))
                    * Matrix.CreateScale(new Vector3(w * Size, h * Size, 1))
                    // * Matrix.CreateScale(new Vector3(FontManager.font.atlas.width * _size,FontManager.font.atlas.height * _size, 1))
                    * Matrix.CreateTranslation(new Vector3(pos + currentOff, 0));

                for (int j = text_vertexCount - 4; j < text_vertexCount; j++)
                    Vector3.Transform(ref text_vertex[j].Position, ref world, out text_vertex[j].Position);
            }
            return stringlen * _size * size;
        }
        public void flush_text()
        {
            //RasterizerState rast = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid };
            graphicsD.RasterizerState = rast;
            graphicsD.BlendState = BlendState.NonPremultiplied;

            graphicsD.SetRenderTarget(null);

            if (text_vertexCount == 0) return;

            text_effect.CurrentTechnique.Passes[0].Apply();

            graphicsD.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, text_vertex, 0, text_vertexCount, text_index, 0, text_indexCount / 3);
            text_vertexCount = text_indexCount = 0;

        }

        private void Text_EnsureSpace(int indexSpace, int vertexSpace)
        {
            if (text_indexCount + indexSpace >= text_index.Length)
                Array.Resize(ref text_index, Math.Max(text_indexCount + indexSpace, text_index.Length * 2));
            if (text_vertexCount + vertexSpace >= text_vertex.Length)
                Array.Resize(ref text_vertex, Math.Max(text_vertexCount + vertexSpace, text_vertex.Length * 2));
        }
    }
}
