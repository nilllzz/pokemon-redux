using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class BatonPassAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 3;
        private const float PROGRESS_PER_FRAME = 0.02f;

        private float _progress = 0f;
        private int _stage = 0;

        public BatonPassAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("batonpass");
        }

        public override void Draw(SpriteBatch batch)
        {
            var frameSize = _texture.Width * Border.SCALE;
            var center = GetCenter(_user.Side);
            var pokemonSize = GetPokemonSpriteSize();

            var posX = (int)(center.X - frameSize / 2f);
            var baseY = center.Y + pokemonSize / 2f - frameSize;
            var varianceY = pokemonSize - frameSize;

            var multiplier = 1f;
            switch (_stage)
            {
                case 1:
                    multiplier = 0.5f;
                    break;
                case 2:
                    multiplier = 0.25f;
                    break;
            }
            var posY = (int)(baseY - Math.Sin(_progress * MathHelper.Pi) * multiplier * varianceY);

            batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize), Color.White);
        }

        public override void Update()
        {
            _progress += PROGRESS_PER_FRAME;
            if (_progress >= 1f)
            {
                _stage++;
                if (_stage == TOTAL_STAGES)
                {
                    IsFinished = true;
                    // do not show the status again, will be done when switched in
                }
                else
                {
                    _progress -= 1f;
                }
            }
        }
    }
}
