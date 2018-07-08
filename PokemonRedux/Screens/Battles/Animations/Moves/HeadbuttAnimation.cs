using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class HeadbuttAnimation : BattleAnimation
    {
        private const int TEXTURE_VISIBLE_FRAMES = 5;
        private const int MOVE_OFFSET = 6;
        private const int TOTAL_SHAKES = 12;
        private const int SHAKING_DELAY = 3;
        private const int SCREEN_OFFSET = 2;

        private readonly BattlePokemon _target;
        private Texture2D _texture;

        private bool _shaking = true;
        private int _totalShakes;
        private int _shakingDelay = SHAKING_DELAY;
        private int _offsetX = 0;
        private bool _moveBack = false;
        private bool _textureVisible = false;
        private int _textureVisibleFrames = TEXTURE_VISIBLE_FRAMES;

        public HeadbuttAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/tackle.png");
        }

        public override void Show()
        {
            // hide status of user
            Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), false);
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_textureVisible)
            {
                var center = GetCenter(_target.Side);
                var frameSize = _texture.Width * Border.SCALE;
                var x = (int)(center.X - frameSize / 2f);
                var y = (int)(center.Y - frameSize / 2f);

                batch.Draw(_texture, new Rectangle(x, y, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            if (_shaking)
            {
                _shakingDelay--;
                if (_shakingDelay == 0)
                {
                    _shakingDelay = SHAKING_DELAY;
                    _totalShakes++;
                    if (_totalShakes == TOTAL_SHAKES)
                    {
                        _shaking = false;
                        Battle.ActiveBattle.AnimationController.SetScreenOffset(Vector2.Zero);
                    }
                    else
                    {
                        var xOffset = (float)(_totalShakes % 2);
                        if (xOffset == 0)
                        {
                            xOffset = -1;
                        }
                        xOffset *= Border.SCALE * SCREEN_OFFSET;

                        Battle.ActiveBattle.AnimationController.SetScreenOffset(new Vector2(xOffset, 0));
                    }
                }
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
                    IsFinished = true;
                    // show status again
                    Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
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
