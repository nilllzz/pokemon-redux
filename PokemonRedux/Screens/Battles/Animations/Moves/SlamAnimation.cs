using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class SlamAnimation : BattleMoveAnimation
    {
        private const int PRE_DELAY = 8;
        private const int HIT_DELAY = 8;
        private const int POST_DELAY = 16;

        private int _delay = PRE_DELAY;
        private int _stage = 0;

        public SlamAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("slam");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stage == 1)
            {
                var center = GetCenter(_target.Side);
                var frameSize = _texture.Width * Border.SCALE;
                var posY = center.Y - frameSize;
                var posX = center.X;
                if (_target.Side == PokemonSide.Enemy)
                {
                    posX -= frameSize;
                }

                batch.Draw(_texture, new Rectangle((int)posX, (int)posY, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            _delay--;
            if (_delay == 0)
            {
                _stage++;
                switch (_stage)
                {
                    case 1:
                        // show the hit, invert screen
                        Battle.ActiveBattle.AnimationController.SetScreenColorInvert(true);
                        _delay = HIT_DELAY;
                        break;

                    case 2:
                        // do nothing, but don't show the hit anymore
                        // screen is still inverted
                        _delay = POST_DELAY;
                        break;

                    case 3:
                        Finish();
                        break;
                }
            }
        }

        protected override void Finish()
        {
            base.Finish();
            Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
        }
    }
}
