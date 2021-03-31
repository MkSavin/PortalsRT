
using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace PortalsRT.UI
{
    public class TextRenderer
    {
        Vector2 scaleFactor;
        Vector2 position;

        public TextRenderer(Vector2 _scaleFactor, Vector2 _position)
        {
            scaleFactor = _scaleFactor;
            position = _position;
        }

        public void SetPosition(Vector2 _position)
        {
            position = _position;
        }

        private static float roundRadius = 0.1f;

        public void Draw(string text, float fontsize = 10, float letterspacing = 5)
        {
            float letterWidth;

            foreach (var character in text)
            {
                letterWidth = DrawLetter(character, fontsize, fontsize);
                position.X += (letterWidth + letterspacing) * scaleFactor.X;
            }
        }

        public float DrawLetter(char letter, float width, float height)
        {
            var radius = roundRadius * width;
            float letterWidth = 0;

            GL.Begin(PrimitiveType.Lines);

            switch (letter)
            {
                case 'a':

                    DrawLine(0, 0, 0.5f * width, 1f * height);
                    DrawLine(0.5f * width, 1f * height, 1f * width, 0);
                    DrawLine(0.75f * width, 0.5f * height, 0.25f * width, 0.5f * height);

                    letterWidth = width;

                    break;
                case 'b':

                    DrawTruncatedQuad(0, 0f * height, 0.5f * width, 0.5f * height, 0, 0, radius, radius);
                    DrawTruncatedQuad(0, 0.5F * height, 0.5f * width, 0.5f * height, 0, 0, radius, radius);

                    letterWidth = 0.5f * width;

                    break;
                case 'c':
                    DrawC(0, 0, 0.75f * width, height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case 'd':

                    DrawTruncatedQuad(0, 0, 0.75f * width, height, 0, 0, radius, radius);

                    letterWidth = 0.75f * width;

                    break;
                case 'e':

                    DrawC(0, 0, 0.75f * width, height, radius);
                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'f':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);
                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'g':

                    DrawC(0, 0, 0.75f * width, height, radius);
                    DrawLine(0.5f * width, 0.5f * height, 0.75f * width, 0.5f * height);
                    DrawLine(0.75f * width, 0, 0.75f * width, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'h':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0.75f * width, 0, 0.75f * width, 1f * height);
                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'i':

                    DrawLine(0.75f / 2 * width, 0, 0.75f / 2 * width, 1f * height);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);
                    DrawLine(0, 0, 0.75f * width, 0);

                    letterWidth = 0.75f * width;

                    break;
                case 'j':

                    DrawLine(0.75f / 2 * width, radius, 0.75f / 2 * width, 1f * height);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);
                    DrawLine(0, 0, 0.75f / 2 * width - radius, 0);
                    DrawLine(0.75f / 2 * width - radius, 0, 0.75f / 2 * width, radius);

                    letterWidth = 0.75f * width;

                    break;
                case 'k':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0, 0.5f * height, 0.5f * width, 1f * height);
                    DrawLine(0, 0.5f * height, 0.5f * width, 0);

                    letterWidth = 0.5f * width;

                    break;
                case 'l':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0, 0, 0.5f * width, 0);

                    letterWidth = 0.5f * width;

                    break;
                case 'm':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(1f * width, 0, 1f * width, 1f * height);
                    DrawLine(0, 1f * height, 0.5f * width, 0.5f * height);
                    DrawLine(1f * width, 1f * height, 0.5f * width, 0.5f * height);

                    letterWidth = width;

                    break;
                case 'n':

                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0.75f * width, 0, 0.75f * width, 1f * height);
                    DrawLine(0, 1f * height, 0.75f * width, 0 * height);
                    letterWidth = 0.75f * width;

                    break;
                case 'o':

                    DrawFullyTruncatedQuad(0, 0, 0.75f * width, height, radius);
                    letterWidth = 0.75f * width;

                    break;
                case 'p':

                    DrawTruncatedQuad(0, 0.5f * height, 0.5f * width, 0.5f * height, 0, 0, radius, radius);
                    DrawLine(0, 0, 0, 0.5f * height);

                    letterWidth = 0.5f * width;

                    break;
                case 'q':

                    DrawFullyTruncatedQuad(0, 0, 0.75f * width, height, radius);
                    DrawLine(0.5f * width, 0.25f * height, 0.75f * width, 0);
                    letterWidth = 0.75f * width;

                    break;
                case 'r':

                    DrawTruncatedQuad(0, 0.5f * height, 0.5f * width, 0.5f * height, 0, 0, radius, radius);
                    DrawLine(0, 0, 0, 0.5f * height);
                    DrawLine(0, 0.5f * height, 0.5f * width, 0);

                    letterWidth = 0.5f * width;

                    break;
                case 's':

                    DrawS(0, 0, 0.75f * width, height, radius);
                    letterWidth = 0.75f * width;

                    break;
                case 't':

                    DrawLine(0.75f / 2 * width, 0, 0.75f / 2 * width, 1f * height);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'u':

                    DrawU(0, 0, 0.75f * width, height, radius);
                    letterWidth = 0.75f * width;

                    break;
                case 'v':

                    DrawLine(0, 1f * height, 0.75f / 2 * width, 0);
                    DrawLine(0.75f / 2 * width, 0, 0.75f * width, 1f * height);
                    letterWidth = 0.75f * width;

                    break;
                case 'w':

                    DrawLine(0, 1f * height, 0.75f / 4 * width, 0);
                    DrawLine(0.75f / 4 * width, 0, 0.75f / 2 * width, 1f * height);

                    DrawLine(0.75f / 2 * width, 1f * height, 0.75f * 3 / 4 * width, 0);
                    DrawLine(0.75f * 3 / 4 * width, 0, 0.75f * width, 1f * height);

                    letterWidth = 0.75f * width;

                    break;
                case 'x':

                    DrawLine(0, 1f * height, 0.75f * width, 0);
                    DrawLine(0, 0, 0.75f * width, 1f * height);
                    letterWidth = 0.75f * width;

                    break;
                case 'y':

                    DrawLine(0, 1f * height, 0.75f / 2 * width, 0.5f * height);
                    DrawLine(0, 0, 0.75f * width, 1f * height);
                    letterWidth = 0.75f * width;

                    break;
                case 'z':

                    DrawLine(0.75f * width, 1f * height, 0, 0);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);
                    DrawLine(0, 0, 0.75f * width, 0);

                    letterWidth = 0.75f * width;

                    break;
                case ' ':

                    letterWidth = 0.5f * width;

                    break;
                case '.':

                    DrawQuad(0, 0, 0.15f * width, 0.15f * width);
                    DrawLine(0.15f * width, 0, 0, 0.15f * width);
                    DrawLine(0.15f * width, 0.15f * width, 0, 0);

                    letterWidth = 0.5f * width;

                    break;
                case ',':

                    DrawQuad(0, 0, 0.15f * width, 0.15f * width);
                    DrawLine(0.15f * width, 0, 0, 0.15f * width);
                    DrawLine(0.15f * width, 0.15f * width, 0, 0);
                    DrawLine(0.15f * width, 0, 0.15f / 3 * width, -0.3f * width);

                    letterWidth = 0.5f * width;

                    break;
                case '-':

                    DrawLine(0.75f * width, 0.5f * height, 0, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '_':

                    DrawLine(0.75f * width, 0, 0, 0);

                    letterWidth = 0.75f * width;

                    break;
                case '=':

                    DrawLine(0.75f * width, 0.25f * height, 0, 0.25f * height);
                    DrawLine(0.75f * width, 0.75f * height, 0, 0.75f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '/':

                    DrawLine(0.75f * width, 1f * height, 0, 0);

                    letterWidth = 0.75f * width;

                    break;
                case '*':

                    DrawLine(0.75f / 2 * width, 0, 0.75f / 2 * width, 0.75f * height);
                    DrawLine(0.75f * width, 0, 0, 0.75f * height);
                    DrawLine(0.75f * width, 0.75f * height, 0, 0);

                    letterWidth = 0.75f * width;

                    break;
                case '|':

                    DrawLine(0.75f / 2 * width, 0, 0.75f / 2 * width, 1f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '(':

                    DrawC(0, 0, 0.25f * width, height, radius);

                    letterWidth = 0.25f * width;

                    break;
                case ')':

                    DrawCReverse(0, 0, 0.25f * width, height, radius);

                    letterWidth = 0.25f * width;

                    break;
                case '0':

                    DrawFullyTruncatedQuad(0, 0, 0.75f * width, 1f * height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case '1':

                    DrawLine(0.75f / 2 * width, 0, 0.75f / 2 * width, 1f * height);
                    DrawLine(0, 0.75f * height, 0.75f / 2 * width, 1f * height);
                    DrawLine(0, 0, 0.75f * width, 0);

                    letterWidth = 0.75f * width;

                    break;
                case '2':

                    Draw2(0, 0, 0.75f * width, height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case '3':

                    DrawCReverse(0, 0, 0.75f * width, 0.5f * height, radius);
                    DrawCReverse(0, 0.5f * width, 0.75f * width, 0.5f * height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case '4':

                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);
                    DrawLine(0, 0.5f * height, 0, 1f * height);
                    DrawLine(0.75f * width, 0, 0.75f * width, 1f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '5':

                    Draw5(0, 0, 0.75f * width, height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case '6':

                    DrawLine(0, height, 0.75f * width, height);
                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);
                    DrawLine(0, 0, 0.75f * width, 0);
                    DrawLine(0, 0, 0, 1f * height);
                    DrawLine(0.75f * width, 0, 0.75f * width, 0.5f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '7':

                    DrawLine(0.75f * width, 1f * height, 0, 0);
                    DrawLine(0, 1f * height, 0.75f * width, 1f * height);

                    letterWidth = 0.75f * width;

                    break;
                case '8':

                    DrawFullyTruncatedQuad(0, 0, 0.75f * width, 0.5f * height, radius);
                    DrawFullyTruncatedQuad(0, 0.5f * height, 0.75f * width, 0.5f * height, radius);

                    letterWidth = 0.75f * width;

                    break;
                case '9':

                    DrawLine(0, height, 0.75f * width, height);
                    DrawLine(0, 0.5f * height, 0.75f * width, 0.5f * height);
                    DrawLine(0, 0, 0.75f * width, 0);

                    DrawLine(0.75f * width, 0, 0.75f * width, 1f * height);
                    DrawLine(0, 0.5f * height, 0, height);

                    letterWidth = 0.75f * width;

                    break;
                default:
                    break;
            }
            GL.End();

            return letterWidth;
        }

        public void DrawLine(float x1, float y1, float x2, float y2)
        {
            GL.Vertex2(position.X + x1 * scaleFactor.X, position.Y + y1 * scaleFactor.X); GL.Vertex2(position.X + x2 * scaleFactor.X, position.Y + y2 * scaleFactor.X);
        }

        private void DrawQuad(float x, float y, float width, float height)
        {
            DrawLine(x, y, x, y + height);
            DrawLine(x, y, x + width, y);
            DrawLine(x + width, y + height, x + width, y);
            DrawLine(x + width, y + height, x, y + height);
        }

        private void DrawTruncatedQuad(float x, float y, float width, float height, float radius1, float radius2, float radius3, float radius4)
        {
            DrawLine(x, y + radius1, x, y + height - radius2);
            DrawLine(x + radius2, y, x + width - radius3, y);
            DrawLine(x + width, y + height - radius3, x + width, y + radius4);
            DrawLine(x + width - radius4, y + height, x + radius1, y + height);

            DrawLine(x, y + radius1, x + radius1, y);
            DrawLine(x, y + height - radius2, x + radius2, y + height);
            DrawLine(x + width, y + radius3, x + width - radius3, y);
            DrawLine(x + width, y + height - radius4, x + width - radius4, y + height);
        }

        private void DrawFullyTruncatedQuad(float x, float y, float width, float height, float radius)
        {
            DrawTruncatedQuad(x, y, width, height, radius, radius, radius, radius);
        }

        private void DrawC(float x, float y, float width, float height, float radius)
        {
            DrawLine(x, y + radius, x, y + height - radius);
            DrawLine(x + radius, y, x + width, y);
            DrawLine(x + width, y + height, x + radius, y + height);

            DrawLine(x, y + radius, x + radius, y);
            DrawLine(x, y + height - radius, x + radius, y + height);
        }

        private void DrawCReverse(float x, float y, float width, float height, float radius)
        {
            DrawLine(x, y, x + width - radius, y);
            DrawLine(x + width, y + height - radius, x + width, y + radius);
            DrawLine(x + width - radius, y + height, x, y + height);

            DrawLine(x + width, y + radius, x + width - radius, y);
            DrawLine(x + width, y + height - radius, x + width - radius, y + height);
        }

        private void DrawS(float x, float y, float width, float height, float radius)
        {
            var halfHeight = height / 2;
            DrawLine(x, y + halfHeight + radius, x, y + height - radius);
            DrawLine(x + radius, y + halfHeight, x + width - radius, y + halfHeight);
            DrawLine(x + width, y + height, x + radius, y + height);

            DrawLine(x, y + halfHeight + radius, x + radius, y + halfHeight);
            DrawLine(x, y + height - radius, x + radius, y + height);

            DrawLine(x, y, x + width - radius, y);
            DrawLine(x + width, y + halfHeight - radius, x + width, y + radius);

            DrawLine(x + width, y + radius, x + width - radius, y);
            DrawLine(x + width, y + halfHeight - radius, x + width - radius, y + halfHeight);
        }

        private void Draw5(float x, float y, float width, float height, float radius)
        {
            var halfHeight = height / 2;
            DrawLine(x, y + halfHeight, x, y + height);
            DrawLine(x, y + halfHeight, x + width, y + halfHeight);
            DrawLine(x + width, y + height, x, y + height);

            DrawLine(x, y, x + width - radius, y);
            DrawLine(x + width, y + halfHeight - radius, x + width, y + radius);

            DrawLine(x + width, y + radius, x + width - radius, y);
            DrawLine(x + width, y + halfHeight - radius, x + width - radius, y + halfHeight);
        }


        private void Draw2(float x, float y, float width, float height, float radius)
        {
            var halfHeight = height / 2;

            DrawLine(x + width, y + height - radius, x + width, y + halfHeight);
            DrawLine(x + width - radius, y + height, x, y + height);

            DrawLine(x + width, y + height - radius, x + width - radius, y + height);

            DrawLine(width, halfHeight, x, y);
            DrawLine(x, y, width, y);
        }

        private void DrawU(float x, float y, float width, float height, float radius)
        {
            DrawLine(x + width - radius, y, x + radius, y);
            DrawLine(x, y + radius, x, y + height);
            DrawLine(x + width, y + height, x + width, y + radius);

            DrawLine(x, y + radius, x + radius, y);
            DrawLine(x + width, y + radius, x + width - radius, y);
        }
    }

}
