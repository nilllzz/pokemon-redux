using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Pokemons;
using System;
using static Core;

namespace PokemonRedux.Screens.Pokemons
{
    class HPBar
    {
        private const int BAR_WIDTH = 48;
        private const int BAR_HEIGHT = 2;
        private const int BAR_OFFSET_X = 16;
        private const int BAR_OFFSET_Y = 3;

        private Texture2D _texture;

        public void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/UI/Pokemon/hpbar.png");
        }

        public void Draw(SpriteBatch batch, Vector2 position, int hp, int maxHp, float scale)
        {
            var startX = (int)position.X;
            var startY = (int)position.Y;

            batch.Draw(_texture, new Rectangle(startX, startY,
                (int)(_texture.Width * scale),
                (int)(_texture.Height * scale)), Color.White);

            var remaining = hp / (double)maxHp;

            var barWidth = (int)(Math.Ceiling(BAR_WIDTH * scale * remaining));
            var barHeight = (int)(BAR_HEIGHT * scale);

            var barColor = PokemonStatHelper.GetHPBarColor(PokemonStatHelper.GetPokemonHealth(hp, maxHp));
            batch.DrawRectangle(new Rectangle(
                (int)(BAR_OFFSET_X * scale) + startX,
                (int)(BAR_OFFSET_Y * scale) + startY,
                barWidth, barHeight), barColor);
        }
    }
}
