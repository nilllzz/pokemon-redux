using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using System.Collections.Generic;
using System.Linq;
using static Core;

namespace PokemonRedux.Game.Pokemons
{
    static class PokemonTextureManager
    {
        public const int TEXTURE_SIZE = 56;
        private const string SIDE_FRONT = "front";
        private const string SIDE_BACK = "back";

        private static Dictionary<string, Texture2D> _spriteCache = new Dictionary<string, Texture2D>();

        private static Texture2D UnmaskTexture(Texture2D template, int x, Color[] palette)
        {
            var texture = new Texture2D(Controller.GraphicsDevice, TEXTURE_SIZE, TEXTURE_SIZE);
            var pixels = template.GetData(new Rectangle(x * TEXTURE_SIZE, 0, TEXTURE_SIZE, TEXTURE_SIZE));

            for (var i = 0; i < pixels.Length; i++)
            {
                var pixel = pixels[i];
                if (pixel.A == 255)
                {
                    if (pixel.R == 255)
                    {
                        pixels[i] = palette[1];
                    }
                    else if (pixel.G == 255)
                    {
                        pixels[i] = palette[2];
                    }
                    else if (pixel.B == 255)
                    {
                        pixels[i] = palette[3];
                    }
                    else
                    {
                        pixels[i] = palette[0];
                    }
                }
            }

            texture.SetData(pixels);
            return texture;
        }

        private static string GetId(int id, int inputUnownLetter)
        {
            if (id == UnownHelper.UNOWN_ID)
            {
                var unownLetter = inputUnownLetter;
                if (unownLetter == -1) // display an unown letter for an unown not bound to pokemon save data.
                {
                    if (Controller.ActivePlayer.UnownsCaught.Length > 0)
                    {
                        unownLetter = Controller.ActivePlayer.UnownsCaught[0];
                    }
                    else
                    {
                        unownLetter = 0;
                    }
                }
                return id + "_" + unownLetter;
            }
            else
            {
                return id.ToString();
            }
        }

        private static Texture2D GetTexture(string id, bool front, Color[] palette)
        {
            var key = id + $":{(front ? SIDE_FRONT : SIDE_BACK)}:" + string.Join(",", palette.Select(c => c.ToString()));
            var sideId = front ? 0 : 1;
            if (!_spriteCache.TryGetValue(key, out var texture))
            {
                var template = Controller.Content.LoadDirect<Texture2D>($"Textures/Pokemon/Main/{id}.png");
                texture = UnmaskTexture(template, sideId, palette);
                _spriteCache.Add(key, texture);
            }
            return texture;
        }

        public static Texture2D GetFront(int id, Color[] palette, int unownLetter = -1)
        {
            return GetTexture(GetId(id, unownLetter), true, palette);
        }

        public static Texture2D GetBack(int id, Color[] palette, int unownLetter = -1)
        {
            return GetTexture(GetId(id, unownLetter), false, palette);
        }

        public static Texture2D GetMenu(int id)
        {
            return Controller.Content.LoadDirect<Texture2D>($"Textures/Pokemon/Menu/{id}.png");
        }

        public static Color[] GetPalette(int[][] colorData)
        {
            return colorData.Select(c => new Color(c[0], c[1], c[2])).ToArray();
        }
    }
}
