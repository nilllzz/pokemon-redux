using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class BarrageAnimation : BattleMoveAnimation
    {
        private const float PROGRESS_PER_FRAME = 0.03f;
        private const int EXPLOSION_DELAY = 6;
        private const int EXPLOSION_FRAMES = 3;

        private float _ball;
        private int _explosionStage = 0;
        private int _explosionDelay = EXPLOSION_DELAY;

        public BarrageAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("barrage");
        }

        public override void Draw(SpriteBatch batch)
        {
            var targetCenter = GetCenter(_target.Side);
            if (_ball <= 1f)
            {
                // draw ball
                var userCenter = GetCenter(_user.Side);
                var pokemonSize = GetPokemonSpriteSize();
                var frameSize = 12 * Border.SCALE;

                Vector2 startPos;
                Vector2 endPos;
                float progressY;

                if (_target.Side == PokemonSide.Enemy)
                {
                    startPos = new Vector2(userCenter.X + pokemonSize / 2f - frameSize, userCenter.Y - frameSize / 2f);
                    endPos = new Vector2(targetCenter.X - frameSize / 2f, targetCenter.Y);
                    progressY = (float)Math.Sin(_ball * MathHelper.PiOver2 * 1.2f);
                }
                else
                {
                    startPos = new Vector2(userCenter.X - pokemonSize / 2f, userCenter.Y - frameSize / 2f);
                    endPos = new Vector2(targetCenter.X - frameSize / 2f, targetCenter.Y);
                    progressY = 1f - (float)Math.Sin((1f - _ball) * MathHelper.PiOver2 * 1.2f);
                }

                var posX = startPos.X + (endPos.X - startPos.X) * _ball;
                var posY = startPos.Y + (endPos.Y - startPos.Y) * progressY;

                batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameSize, (int)frameSize),
                    new Rectangle(96, 0, 12, 12), Color.White);
            }
            else
            {
                // draw explosion
                var frameSize = 32 * Border.SCALE;

                var posX = targetCenter.X - frameSize / 2f;
                var posY = targetCenter.Y - frameSize / 2f;

                batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameSize, (int)frameSize),
                    new Rectangle(_explosionStage * 32, 0, 32, 32), Color.White);
            }
        }

        public override void Update()
        {
            if (_ball < 1f)
            {
                _ball += PROGRESS_PER_FRAME;
            }
            else
            {
                _explosionDelay--;
                if (_explosionDelay == 0)
                {
                    _explosionDelay = EXPLOSION_DELAY;
                    _explosionStage++;
                    if (_explosionStage == EXPLOSION_FRAMES)
                    {
                        Finish();
                    }
                }
            }
        }
    }
}
