using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    internal class SurfAnimation : BattleMoveAnimation
    {
        private const float EFFECT_OFFSET_SPEED = 1f;
        private const int WAVE_HEIGHT = 8;
        private const int WAVE_WIDTH = 80;
        private const float WAVE_ACCELERATION = 0.15f;
        private const float WAVE_MAX_SPEED = 1.5f;
        private const float WAVE_LOWER_Y = 12 * Border.UNIT;
        private const float MAX_WAVE_X_OFFSET = 8f;
        private const float WAVE_X_OFFSET_SPEED = 0.5f;
        private static readonly float[] PASS_FALLING = new[] // positions at which the direction reverses
        {
            48f,
            8f
        };

        private Effect _shader;

        private float _startY = 0;
        private float _effectOffset = 0f;
        private bool _falling;
        private float _fallTo;
        private float _speed;
        private int _fallToIndex = 0;
        private float _waveXOffset = 0f;

        public SurfAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("surf");
            _shader = Controller.Content.Load<Effect>("Shaders/Battle/underwater");

            _startY = WAVE_LOWER_Y;
            _falling = true;
            _fallTo = _startY + WAVE_HEIGHT;
        }

        public override void Show()
        {
            base.Show();

            Battle.ActiveBattle.AnimationController.SetScreenEffect(_shader);

            _shader.Parameters["offset"].SetValue(_effectOffset);
            _shader.Parameters["startY"].SetValue(_startY * Border.SCALE + BattleScreen.StartY);
            _shader.Parameters["endY"].SetValue(_startY * Border.SCALE + BattleScreen.StartY);
        }

        public override void Draw(SpriteBatch batch)
        {
            var (startX, startY, _) = GetScreenValues();
            var y = startY + _startY * Border.SCALE;
            var x = (int)(startX + _waveXOffset);
            var waveWidth = (int)(WAVE_WIDTH * Border.SCALE);
            var waveHeight = (int)(WAVE_HEIGHT * Border.SCALE);
            // center piece
            batch.Draw(_texture, new Rectangle(x, (int)y, waveWidth, waveHeight), Color.White);
            // right piece
            batch.Draw(_texture, new Rectangle(x + waveWidth, (int)y - waveHeight, waveWidth, waveHeight), Color.White);
            // left piece
            batch.Draw(_texture, new Rectangle(x - waveHeight, (int)y + waveHeight, waveHeight, waveHeight),
                new Rectangle(WAVE_WIDTH - WAVE_HEIGHT, 0, WAVE_HEIGHT, WAVE_HEIGHT), Color.White);
        }

        public override void Update()
        {
            _effectOffset += EFFECT_OFFSET_SPEED;

            if (_falling)
            {
                if (_speed < WAVE_MAX_SPEED)
                {
                    _speed += WAVE_ACCELERATION;
                    if (_speed >= WAVE_MAX_SPEED)
                    {
                        _speed = WAVE_MAX_SPEED;
                    }
                }
                if (_startY >= _fallTo)
                {
                    _falling = false;
                }
            }
            else
            {
                if (_speed > -WAVE_MAX_SPEED)
                {
                    _speed -= WAVE_ACCELERATION;
                    if (_speed <= -WAVE_MAX_SPEED)
                    {
                        _speed = -WAVE_MAX_SPEED;
                    }
                }
            }

            _startY += _speed;
            if (_fallToIndex < PASS_FALLING.Length)
            {
                if (_startY <= PASS_FALLING[_fallToIndex])
                {
                    _falling = true;
                    _fallTo = _startY + WAVE_HEIGHT;
                    _fallToIndex++;
                }
            }
            else
            {
                if (_startY <= -WAVE_HEIGHT * 4)
                {
                    _falling = true;
                    _fallTo = WAVE_LOWER_Y + WAVE_HEIGHT;
                }
                else if (_startY >= WAVE_LOWER_Y)
                {
                    Finish();
                    Battle.ActiveBattle.AnimationController.SetScreenEffect();
                }
            }

            _waveXOffset += WAVE_X_OFFSET_SPEED;
            if (_waveXOffset >= MAX_WAVE_X_OFFSET)
            {
                _waveXOffset = 0f;
            }

            _shader.Parameters["offset"].SetValue(_effectOffset);
            _shader.Parameters["startY"].SetValue(_startY * Border.SCALE + BattleScreen.StartY);
        }
    }
}