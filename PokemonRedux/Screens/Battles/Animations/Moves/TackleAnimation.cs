using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class TackleAnimation : BattleAnimation
    {
        private const int TEXTURE_VISIBLE_FRAMES = 5;
        private const int MOVE_OFFSET = 6;

        private readonly BattlePokemon _target;
        private Texture2D _texture;

        private int _offsetX = 0;
        private bool _moveBack = false;
        private bool _textureVisible = false;
        private int _textureVisibleFrames = TEXTURE_VISIBLE_FRAMES;

        public TackleAnimation(BattlePokemon target)
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
