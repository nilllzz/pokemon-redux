using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using static Core;

namespace PokemonRedux.Screens
{
    static class Border
    {
        // typical screen dimensions in unit sizes
        public const int SCREEN_WIDTH = 20;
        public const int SCREEN_HEIGHT = 18;

        public const int BORDER_TYPES = 8;
        public const int UNIT = 8; // pixels
        public const float SCALE = 4f;

        public static (int unit, int startX, int width, int height) GetDefaultScreenValues()
        {
            var unit = (int)(SCALE * UNIT);
            var width = SCREEN_WIDTH * unit;
            var height = SCREEN_HEIGHT * unit;
            var startX = (int)(Controller.ClientRectangle.Width / 2f - width / 2f);

            return (unit, startX, width, height);
        }

        // slightly gray color used for white planes
        public static Color DefaultWhite
            => new Color(248, 248, 248);

        private static Texture2D GetTexture()
        {
            return Controller.Content.LoadDirect<Texture2D>($"Textures/UI/Borders/{Controller.GameOptions.BorderFrameType}.png");
        }

        public static void Draw(SpriteBatch batch, int startX, int startY, int width, int height, float scale)
        {
            Draw(batch, startX, startY, width, height, scale, DefaultWhite);
        }

        public static void Draw(SpriteBatch batch, int startX, int startY, int width, int height, float scale, Color color)
        {
            var texture = GetTexture();
            var unit = (int)(UNIT * scale);

            // draw corners
            // upper left
            batch.Draw(texture, new Rectangle(startX, startY, unit, unit),
                new Rectangle(0, 0, UNIT, UNIT), color);
            // upper right
            batch.Draw(texture, new Rectangle(startX + (width - 1) * unit, startY, unit, unit),
                new Rectangle(UNIT * 2, 0, UNIT, UNIT), color);
            // lower left
            batch.Draw(texture, new Rectangle(startX, startY + (height - 1) * unit, unit, unit),
                new Rectangle(0, UNIT * 2, UNIT, UNIT), color);
            // lower right
            batch.Draw(texture, new Rectangle(startX + (width - 1) * unit, startY + (height - 1) * unit, unit, unit),
                new Rectangle(UNIT * 2, UNIT * 2, UNIT, UNIT), color);

            // draw horizontal lines
            for (int x = 1; x < width - 1; x++)
            {
                // upper
                batch.Draw(texture, new Rectangle(startX + x * unit, startY, unit, unit),
                    new Rectangle(UNIT, 0, UNIT, UNIT), color);
                // lower
                batch.Draw(texture, new Rectangle(startX + x * unit, startY + (height - 1) * unit, unit, unit),
                    new Rectangle(UNIT, UNIT * 2, UNIT, UNIT), color);
            }
            // draw vertical lines
            for (int y = 1; y < height - 1; y++)
            {
                // left
                batch.Draw(texture, new Rectangle(startX, startY + y * unit, unit, unit),
                    new Rectangle(0, UNIT, UNIT, UNIT), color);
                // right
                batch.Draw(texture, new Rectangle(startX + (width - 1) * unit, startY + y * unit, unit, unit),
                    new Rectangle(UNIT * 2, UNIT, UNIT, UNIT), color);
            }

            DrawCenter(batch, startX + unit, startY + unit, width - 2, height - 2, scale, color);
        }

        public static void DrawCenter(SpriteBatch batch, int startX, int startY, int width, int height, float scale)
        {
            DrawCenter(batch, startX, startY, width, height, scale, DefaultWhite);
        }

        public static void DrawCenter(SpriteBatch batch, int startX, int startY, int width, int height, float scale, Color color)
        {
            var texture = GetTexture();
            var unit = (int)(UNIT * scale);

            // fill center
            batch.Draw(texture, new Rectangle(startX, startY, width * unit, height * unit),
                new Rectangle(UNIT, UNIT, UNIT, UNIT), color);
        }
    }
}
