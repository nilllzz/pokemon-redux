using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class TakeDownAnimation : BattleAnimation
    {
        private const int MOVE_OFFSET = 6;
        private const int INVERT_FRAMES = 6;
        private const int INVERT_START_FRAME = 4;
        private const int HIT_FRAMES = 8;
        private const int HIT_START_FRAME = 3;

        private readonly BattlePokemon _target;

        private Texture2D _texture;

        private int _offsetX = 0;
        private bool _moveBack = false;
        private int _invertFrames = 0;
        private int _hit1State = 0, _hit2State = 0;

        public TakeDownAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/Animations/takedown.png");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var frameSize = 24 * Border.SCALE;
            var textureRect = _invertFrames > 0 ?
                new Rectangle(24, 0, 24, 24) :
                new Rectangle(0, 0, 24, 24);
            if (_hit1State > 0)
            {
                var posX = (int)(center.X - frameSize / 2f);
                var posY = (int)center.Y;

                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    textureRect, Color.White);
            }
            else if (_hit2State > 0)
            {
                var posX = center.X;
                if (_target.Side == PokemonSide.Player)
                {
                    posX -= frameSize;
                }

                var posY = (int)(center.Y - frameSize / 2f);

                batch.Draw(_texture, new Rectangle((int)posX, posY, (int)frameSize, (int)frameSize),
                    textureRect, Color.White);
            }
        }

        public override void Update()
        {
            if (_hit1State > 0)
            {
                _hit1State--;
                if (_hit1State == 0)
                {
                    _hit2State = HIT_FRAMES;
                }
            }
            if (_hit2State > 0)
            {
                _hit2State--;
                if (_hit2State == 0)
                {
                    IsFinished = true;
                    // show status again
                    Battle.ActiveBattle.UI.SetPokemonStatusVisible(BattlePokemon.ReverseSide(_target.Side), true);
                }
            }

            if (_moveBack)
            {
                if (_offsetX > 0)
                {
                    _offsetX--;
                    if (_offsetX == 0)
                    {
                        _invertFrames = INVERT_FRAMES;
                    }
                }
            }
            else
            {
                _offsetX++;
                if (_offsetX == MOVE_OFFSET)
                {
                    _moveBack = true;
                }
                else if (_offsetX == INVERT_START_FRAME)
                {
                    _invertFrames = INVERT_FRAMES;
                }
                else if (_offsetX == HIT_START_FRAME)
                {
                    _hit1State = HIT_FRAMES;
                }
            }

            if (_invertFrames == 0)
            {
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
            }
            else
            {
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(true);
                _invertFrames--;
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
