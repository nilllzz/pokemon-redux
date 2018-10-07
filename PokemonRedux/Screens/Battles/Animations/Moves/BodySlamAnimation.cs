using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class BodySlamAnimation : BattleMoveAnimation
    {
        private const float VERTICAL_PROGRESS_PER_FRAME = 0.03f;
        private const float HORIZONTAL_PROGRESS_PER_FRAME = 0.2f;
        private const int HIT_DELAY = 6;

        private float _progress = 0f;
        private bool _vertical = true, _goingRight, _goingLeft;
        private int _hit = -1;
        private int _hitDelay = 0;

        public BodySlamAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("bodyslam");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_hit >= 0)
            {
                var center = GetCenter(_target.Side);
                var pokemonSize = GetPokemonSpriteSize();
                var frameSize = _texture.Width * Border.SCALE;

                var offset = Vector2.Zero;
                switch (_hit)
                {
                    case 0:
                        offset = new Vector2(-frameSize / 2f);
                        break;
                    case 1:
                        if (_target.Side == PokemonSide.Enemy)
                        {
                            offset = new Vector2(0, -frameSize / 2f);
                        }
                        else
                        {
                            offset = new Vector2(-frameSize, -frameSize / 2f);
                        }
                        break;
                }

                var posX = (int)(center.X + offset.X);
                var posY = (int)(center.Y + offset.Y);
                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            if (_hitDelay > 0)
            {
                _hitDelay--;
                if (_hitDelay == 0)
                {
                    if (_hit == 1)
                    {
                        Finish();
                    }
                    else
                    {
                        _hit++;
                        _hitDelay = HIT_DELAY;
                    }
                }
            }

            if (_vertical)
            {
                _progress += VERTICAL_PROGRESS_PER_FRAME;
                if (_progress >= 1f)
                {
                    _progress = 0f;
                    _vertical = false;
                    _goingRight = true;
                }
            }
            else if (_goingRight)
            {
                _progress += HORIZONTAL_PROGRESS_PER_FRAME;
                if (_progress >= 1f)
                {
                    _progress = 1f;
                    _goingRight = false;
                    _goingLeft = true;

                    _hit = 0;
                    _hitDelay = HIT_DELAY;
                }
            }
            else if (_goingLeft)
            {
                _progress -= HORIZONTAL_PROGRESS_PER_FRAME;
                if (_progress <= 0f)
                {
                    _progress = 0f;
                    _goingLeft = false;
                }
            }

            var offset = Vector2.Zero;

            if (_vertical)
            {
                offset = new Vector2(0, (float)Math.Sin(MathHelper.Pi * _progress) * GetPokemonSpriteSize() * 0.15f);
            }
            else if (_goingLeft || _goingRight)
            {
                var xOffset = _progress * GetPokemonSpriteSize() * 0.25f;
                if (_user.Side == PokemonSide.Enemy)
                {
                    xOffset *= -1f;
                }
                offset = new Vector2(xOffset, 0);
            }

            Battle.ActiveBattle.AnimationController.SetPokemonOffset(_user.Side, offset);
        }
    }
}
