using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class StringShotAnimation : BattleMoveAnimation
    {
        private const int COLOR_SWAP_DELAY = 6;
        private const int STATIC_FRAME_AMOUNT = 60;

        private readonly int[] _lineStatus = new[] { 0, -16, -22 };
        private readonly int[] _wrapStatus = new[] { -10, -30, -20 };
        private int _colorSwap = 0;
        private int _colorSwapDelay = COLOR_SWAP_DELAY;
        private int _staticFrames = STATIC_FRAME_AMOUNT;
        private bool _countStaticFrames = false;

        public StringShotAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("stringshot");
        }

        public override void Draw(SpriteBatch batch)
        {
            for (var i = 0; i < _lineStatus.Length; i++)
            {
                var status = _lineStatus[i];
                if (status >= 0 && status <= 15)
                {
                    DrawLine(batch, i, status);
                }
            }
            for (var i = 0; i < _wrapStatus.Length; i++)
            {
                var status = _wrapStatus[i];
                if (status >= 0)
                {
                    DrawWrap(batch, i, status);
                }
            }
        }

        private void DrawLine(SpriteBatch batch, int lineIndex, int lineStatus)
        {
            var center = GetCenter(_user.Side);
            var linePartWidth = Border.SCALE * 16;
            var linePartHeight = Border.SCALE * 8;

            if (_target.Side == PokemonSide.Enemy)
            {
                var offsetX = Border.SCALE * 12;
                var offsetY = Border.SCALE * (-8 + 4 * lineIndex);

                var posX = offsetX + center.X;
                var posY = offsetY + center.Y - linePartHeight;

                var partAmount = Math.Min((int)Math.Ceiling(lineStatus / 3d), 3);

                for (var i = 0; i < partAmount; i++)
                {
                    batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)linePartWidth, (int)linePartHeight),
                        new Rectangle(52 + _colorSwap * 16, 0, 16, 8), Color.White);

                    posX += linePartWidth;
                    posY -= linePartHeight;
                }
            }
            else
            {
                var offsetX = Border.SCALE * -12;
                var offsetY = Border.SCALE * (8 + 4 * lineIndex);

                var posX = offsetX + center.X - linePartWidth;
                var posY = offsetY + center.Y;

                var partAmount = Math.Min((int)Math.Ceiling(lineStatus / 3d), 3);

                for (var i = 0; i < partAmount; i++)
                {
                    batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)linePartWidth, (int)linePartHeight),
                        new Rectangle(52 + _colorSwap * 16, 0, 16, 8), Color.White);

                    posX -= linePartWidth;
                    posY += linePartHeight;
                }
            }
        }

        private void DrawWrap(SpriteBatch batch, int wrapIndex, int wrapStatus)
        {
            var center = GetCenter(_target.Side);
            var wrapPartWidth = Border.SCALE * 52;
            var wrapPartHeight = Border.SCALE * 12;

            var offsetX = -wrapPartWidth / 2f;
            var offsetY = -16 * Border.SCALE + wrapIndex * (wrapPartHeight - 4 * Border.SCALE);

            var posX = offsetX + center.X;
            var posY = offsetY + center.Y;

            var progress = Math.Min(Math.Ceiling(wrapStatus / 3d), 3);
            var texWidth = (int)Math.Floor(52 / 3d * progress);

            batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)(wrapPartWidth * (progress / 3d)), (int)wrapPartHeight),
                new Rectangle(0, wrapIndex * 12 + 36 * _colorSwap, texWidth, 12), Color.White);
        }

        public override void Update()
        {
            _colorSwapDelay--;
            if (_colorSwapDelay == 0)
            {
                _colorSwapDelay = COLOR_SWAP_DELAY;
                _colorSwap++;
                if (_colorSwap == 2)
                {
                    _colorSwap = 0;
                }
            }

            for (var i = 0; i < _lineStatus.Length; i++)
            {
                _lineStatus[i]++;
            }
            for (var i = 0; i < _wrapStatus.Length; i++)
            {
                _wrapStatus[i]++;
            }

            if (!_countStaticFrames && _wrapStatus.All(w => w > 10) && _lineStatus.All(l => l > 15))
            {
                _countStaticFrames = true;
            }
            if (_countStaticFrames)
            {
                _staticFrames--;
                if (_staticFrames == 0)
                {
                    Finish();
                }
            }
        }
    }
}
