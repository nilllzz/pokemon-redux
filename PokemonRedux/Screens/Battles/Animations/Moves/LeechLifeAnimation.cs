using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class LeechLifeAnimation : BattleAnimation
    {
        private const float PROGRESS_PER_FRAME = 0.0075f;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private float _progress = 0f;

        public LeechLifeAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/leechlife.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            var source = GetCenter(_target.Side);
            var destination = GetCenter(BattlePokemon.ReverseSide(_target.Side));

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
                IsFinished = true;
                // show status again
                Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
            }
        }
    }
}
