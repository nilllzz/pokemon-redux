using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class RapidSpinAnimation : BattleMoveAnimation
    {
        private const int TEXTURE_VISIBLE_FRAMES = 5;
        private const int MOVE_OFFSET = 6;

        private readonly int[] _bladeStates = new[] { -1, -4, -7, -10, -13 };
        private int _waitFrames = 20;
        private int _offsetX = 0;
        private bool _moveBack = false;
        private bool _textureVisible = false;
        private int _textureVisibleFrames = TEXTURE_VISIBLE_FRAMES;

        public RapidSpinAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("rapidspin");
        }

        public override void Draw(SpriteBatch batch)
        {
            (_, _, var unit) = GetScreenValues();
            var userCenter = GetCenter(_user.Side);
            var bladeWidth = (int)(32 * Border.SCALE);
            var bladeHeight = (int)(16 * Border.SCALE);

            foreach (var bladeState in _bladeStates)
            {
                if (bladeState >= 0 && bladeState <= 13)
                {
                    var bladeSide = Math.Floor(bladeState / 2f) % 2;

                    var posY = (int)(userCenter.Y + unit * 1.5f);
                    posY -= (int)(bladeState * 4 * Border.SCALE);
                    int posX;
                    SpriteEffects effect;

                    if (bladeSide == 0)
                    {
                        posX = (int)(userCenter.X - unit);
                        effect = SpriteEffects.FlipHorizontally;
                    }
                    else
                    {
                        posX = (int)(userCenter.X - unit * 4 + Border.SCALE);
                        effect = SpriteEffects.None;
                    }

                    batch.Draw(_texture, new Rectangle(posX, posY, bladeWidth, bladeHeight),
                        new Rectangle(0, 0, 32, 16), Color.White, 0f, Vector2.Zero, effect, 0f);
                }
            }

            if (_textureVisible)
            {
                var center = GetCenter(_target.Side);
                var frameSize = 24 * Border.SCALE;
                var x = (int)(center.X - frameSize / 2f);
                var y = (int)(center.Y - frameSize / 2f);

                batch.Draw(_texture, new Rectangle(x, y, (int)frameSize, (int)frameSize),
                    new Rectangle(0, 16, 24, 24), Color.White);
            }
        }

        public override void Update()
        {
            if (_bladeStates.Any(b => b <= 13))
            {
                for (var i = 0; i < _bladeStates.Length; i++)
                {
                    _bladeStates[i]++;
                }
            }
            else
            {
                if (_waitFrames > 0)
                {
                    _waitFrames--;
                }
                else
                {
                    if (_moveBack)
                    {
                        if (_offsetX > 0)
                        {
                            _offsetX--;
                        }
                    }
                    else
                    {
                        _offsetX++;
                        if (_offsetX == MOVE_OFFSET)
                        {
                            _moveBack = true;
                            _textureVisible = true;
                        }
                    }

                    if (_textureVisible)
                    {
                        _textureVisibleFrames--;
                        if (_textureVisibleFrames == 0)
                        {
                            _textureVisible = false;
                        }
                    }

                    if (!_textureVisible && _moveBack && _offsetX == 0)
                    {
                        Finish();
                    }

                    var offsetX = _offsetX;
                    if (_target.Side == PokemonSide.Player)
                    {
                        offsetX *= -1;
                        offsetX /= 2;
                    }
                    else
                    {
                        offsetX *= 2;
                    }
                    Battle.ActiveBattle.AnimationController.SetPokemonOffset(BattlePokemon.ReverseSide(_target.Side),
                        new Vector2(offsetX * Border.SCALE, 0));
                }
            }
        }
    }
}
