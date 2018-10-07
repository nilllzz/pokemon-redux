using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class PeckAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 3;
        private const int STAGE_DELAY = 8;
        private const int STAGE_INVISIBLE_DELAY = 4;

        private int _stage = 0;
        private int _stageDelay = STAGE_DELAY;

        public PeckAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("peck");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_stage % 2 == 0)
            {
                var center = GetCenter(_target.Side);
                var frameSize = _texture.Width * Border.SCALE;
                var offset = new Vector2(-frameSize / 2f);

                if (_stage == TOTAL_STAGES - 1)
                {
                    if (_user.Side == PokemonSide.Player)
                    {
                        offset = Vector2.Zero;
                    }
                    else
                    {
                        offset = new Vector2(-frameSize, 0);
                    }
                }

                var posX = (int)(center.X + offset.X);
                var posY = (int)(center.Y + offset.Y);
                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize), Color.White);
            }
        }

        public override void Update()
        {
            _stageDelay--;
            if (_stageDelay == 0)
            {
                _stage++;
                if (_stage == TOTAL_STAGES)
                {
                    Finish();
                }
                else if (_stage % 2 == 0)
                {
                    _stageDelay = STAGE_DELAY;
                }
                else
                {
                    _stageDelay = STAGE_INVISIBLE_DELAY;
                }
            }
        }
    }
}
