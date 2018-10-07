using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class AcidAnimation : BattleMoveAnimation
    {
        private const int BALLS = 8;
        private const float PROGRESS_PER_FRAME = 0.035f;
        private const int BALL_DELAY = 6;

        private List<float> _balls = new List<float>();
        private int _ballDelay = BALL_DELAY;

        public AcidAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("acid");
        }

        public override void Draw(SpriteBatch batch)
        {
            foreach (var ball in _balls.Where(b => b <= 1f))
            {
                var targetCenter = GetCenter(_target.Side);
                var userCenter = GetCenter(_user.Side);
                var pokemonSize = GetPokemonSpriteSize();
                var frameSize = _texture.Width * Border.SCALE;

                Vector2 startPos;
                Vector2 endPos;
                float progressY;

                if (_target.Side == PokemonSide.Enemy)
                {
                    startPos = new Vector2(userCenter.X + pokemonSize / 2f - frameSize, userCenter.Y - frameSize / 2f);
                    endPos = new Vector2(targetCenter.X - frameSize / 2f, targetCenter.Y);
                    progressY = (float)Math.Sin(ball * MathHelper.PiOver2 * 1.2f);
                }
                else
                {
                    startPos = new Vector2(userCenter.X - pokemonSize / 2f, userCenter.Y - frameSize / 2f);
                    endPos = new Vector2(targetCenter.X - frameSize / 2f, targetCenter.Y);
                    progressY = 1f - (float)Math.Sin((1f - ball) * MathHelper.PiOver2 * 1.2f);
                }

                var posX = startPos.X + (endPos.X - startPos.X) * ball;
                var posY = startPos.Y + (endPos.Y - startPos.Y) * progressY;

                batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            if (_balls.Count < BALLS)
            {
                _ballDelay--;
                if (_ballDelay == 0)
                {
                    _ballDelay = BALL_DELAY;
                    _balls.Add(0f);
                }
            }
            else if (_balls.All(b => b > 1f))
            {
                Finish();
            }

            for (var i = 0; i < _balls.Count; i++)
            {
                _balls[i] += PROGRESS_PER_FRAME;
            }
        }
    }
}
