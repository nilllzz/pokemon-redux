using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class CutAnimation : BattleMoveAnimation
    {
        private const int STAGE_DELAY = 3;
        private const int TOTAL_STAGES = 5;
        private const int BLINK_DELAY = 3;
        private const int TOTAL_BLINKS = 6;

        private int _stage = 0;
        private int _stageDelay = STAGE_DELAY;
        private int _blinks = 0;
        private int _blinkDelay = BLINK_DELAY;

        public CutAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("cut");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_blinks % 2 == 0)
            {
                var center = GetCenter(_target.Side);
                var frameSize = 46 * Border.SCALE;
                var posX = (int)(center.X - frameSize / 2f);
                var posY = (int)(center.Y - frameSize / 2f);
                var effect = _target.Side == PokemonSide.Enemy ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                    new Rectangle(_stage * 46, 0, 46, 46), Color.White,
                    0f, Vector2.Zero, effect, 0f);
            }
        }

        public override void Update()
        {
            if (_stage < TOTAL_STAGES)
            {
                _stageDelay--;
                if (_stageDelay == 0)
                {
                    _stageDelay = STAGE_DELAY;
                    _stage++;
                }
            }
            else
            {
                _blinkDelay--;
                if (_blinkDelay == 0)
                {
                    _blinkDelay = BLINK_DELAY;
                    _blinks++;
                    if (_blinks == TOTAL_BLINKS)
                    {
                        Finish();
                    }
                }
            }
        }
    }
}
