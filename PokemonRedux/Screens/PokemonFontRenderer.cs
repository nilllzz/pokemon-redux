using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PokemonRedux.Content;
using PokemonRedux.Game.Data;
using System;
using System.Collections.Generic;
using static Core;

namespace PokemonRedux.Screens
{
    class PokemonFontRenderer
    {
        public const char ESCAPE_CHAR = '^';

        private Dictionary<string, Point> _charPositions = new Dictionary<string, Point>();
        private Texture2D _fontTexture;
        private FontDescription _fontDescription;

        public int LineGap { get; set; } = 1;

        public void LoadContent()
        {
            _fontTexture = Controller.Content.LoadDirect<Texture2D>("Fonts/main.png");

            var fontDescriptionText = Controller.Content.LoadDirect<string>("Fonts/main.json");
            _fontDescription = JsonConvert.DeserializeObject<FontDescription>(fontDescriptionText);

            foreach (var charDesc in _fontDescription.chars)
            {
                var x = charDesc.x * _fontDescription.charWidth;
                var y = charDesc.y * _fontDescription.charHeight;

                if (charDesc.isStr)
                {
                    _charPositions.Add(charDesc.chars, new Point(x, y));
                }
                else
                {
                    foreach (var c in charDesc.chars)
                    {
                        _charPositions.Add(c.ToString(), new Point(x, y));
                        x += _fontDescription.charWidth;
                        if (x >= _fontTexture.Width)
                        {
                            x = 0;
                            y += _fontDescription.charHeight;
                        }
                    }
                }
            }
        }

        public void DrawText(SpriteBatch batch, string text, Vector2 position, Color color, float size = 1f)
        {
            text = text.Replace(Environment.NewLine, "\n");

            var charWidth = _fontDescription.charWidth * size;
            var charHeight = _fontDescription.charHeight * size;
            var kerning = size;

            var charSize = new Point(_fontDescription.charWidth, _fontDescription.charHeight);

            var x = position.X;
            var y = (int)position.Y;

            void drawChar(string charStr)
            {
                var texPos = _charPositions[charStr];
                batch.Draw(_fontTexture, new Rectangle((int)x, y, (int)charWidth, (int)charHeight),
                    new Rectangle(texPos, charSize), color);
                x += charWidth;
            }

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];

                if (c == '\n')
                {
                    // new line
                    x = position.X;
                    y = (int)(y + charHeight * (1 + LineGap));
                }
                else if (c == ESCAPE_CHAR)
                {
                    if (text.Length > i + 2)
                    {
                        var charStr = text[i + 1].ToString() + text[i + 2];
                        if (_charPositions.ContainsKey(charStr))
                        {
                            drawChar(charStr);
                            // advance char pos to after the id
                            i += 2;
                        }
                    }
                }
                else
                {
                    drawChar(c.ToString());
                }
            }
        }

        public Vector2 MeasureText(string text, float size = 1f)
        {
            var charAmount = PrintableCharAmount(text);
            return new Vector2(_fontDescription.charWidth * size * charAmount, _fontDescription.charHeight * size);
        }

        public static int PrintableCharAmount(string text)
        {
            var length = 0;

            foreach (var c in text)
            {
                if (c == '^')
                {
                    length--;
                }
                else
                {
                    length++;
                }
            }

            return length;
        }
    }
}
