using GameDevCommon.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class LickAnimation : BattleMoveAnimation
    {
        private const int TOTAL_STAGES = 3;
        private const int STAGE_DELAY = 4;
        private const int VISIBLE_FLIPS = 7;
        private const int VISIBLE_FLIP_INITIAL_DELAY = 8;
        private const int VISIBLE_FLIP_DELAY = 3;

        private int _stage = 0;
        private int _stageDelay = STAGE_DELAY;
        private int _visibleFlip = 0;
        private int _visibleFlipDelay = VISIBLE_FLIP_INITIAL_DELAY;

        public LickAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("lick");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_visibleFlip % 2 == 0)
            {
                var center = GetCenter(_target.Side);
                var frameWidth = _texture.Width / 3f * Border.SCALE;
                var frameHeight = _texture.Height * Border.SCALE;

                var posX = (int)(center.X - frameWidth / 2f);
                var posY = (int)(center.Y - frameHeight / 2f);

                var effect = _user.Side == PokemonSide.Player ?
                    SpriteEffects.None : SpriteEffects.FlipHorizontally;

                batch.Draw(_texture, new Rectangle(posX, posY, (int)frameWidth, (int)frameHeight),
                    new Rectangle(16 * _stage, 0, 16, 24), Color.White,
                    0f, Vector2.Zero, effect, 0f);
            }
        }

        public override void Update()
        {
            if (_stage < TOTAL_STAGES - 1)
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
                _visibleFlipDelay--;
                if (_visibleFlipDelay == 0)
                {
                    _visibleFlip++;
                    if (_visibleFlip == VISIBLE_FLIPS)
                    {
                        Finish();
                    }
                    else
                    {
                        _visibleFlipDelay = VISIBLE_FLIP_DELAY;
                    }
                }
            }
        }
    }
}
