using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class LeechLifeAnimation : BattleMoveAnimation
    {
        private const float PROGRESS_PER_FRAME = 0.0075f;

        private float _progress = 0f;

        public LeechLifeAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("leechlife");
        }

        public override void Draw(SpriteBatch batch)
        {
            var source = GetCenter(_target.Side);
            var destination = GetCenter(_user.Side);

            var posX = source.X + (destination.X - source.X) * _progress;
            var posY = source.Y + (destination.Y - source.Y) * _progress;

            var frameSize = Border.SCALE * 8;

            for (var i = 0; i < 8; i++)
            {
                var angle = MathHelper.PiOver4 * i + _progress * MathHelper.TwoPi * 1.5f;
                var radius = Border.SCALE * Border.UNIT * Border.SCREEN_WIDTH * 0.5f;
                if (_progress <= 0.5f)
                {
                    radius *= _progress * 2f;
                }
                else
                {
                    radius *= 1f - ((_progress - 0.5f) * 2f);
                }

                var x = (int)(posX + radius * Math.Cos(angle));
                var y = (int)(posY + radius * Math.Sin(angle));

                batch.Draw(_texture, new Rectangle(x, y, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            _progress += PROGRESS_PER_FRAME;
            if (_progress >= 1f)
            {
                _progress = 1f;
                Finish();
            }
        }
    }
}
