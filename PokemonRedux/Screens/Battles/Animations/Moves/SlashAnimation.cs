using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class SlashAnimation : BattleMoveAnimation
    {
        private const int ANIMATION_STAGES = 6;
        private const int ANIMATION_DELAY = 3;
        private const int ANIMATION_FLICKER_AMOUNT = 8;
        private const int ANIMATION_FRAME_SIZE = 56;

        private int _animationDelay = ANIMATION_DELAY;
        private int _animationStage = 0;
        private int _flickerAmount = 0;

        public SlashAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("slash");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);

            var frameSize = ANIMATION_FRAME_SIZE * Border.SCALE;
            var x = (int)(center.X - frameSize / 2f);
            var y = (int)(center.Y - frameSize / 2f);

            if (_flickerAmount % 2 == 0)
            {
                batch.Draw(_texture, new Rectangle(x, y, (int)frameSize, (int)frameSize),
                    new Rectangle(_animationStage * ANIMATION_FRAME_SIZE, 0, ANIMATION_FRAME_SIZE, ANIMATION_FRAME_SIZE),
                    Color.White);
            }
        }

        public override void Update()
        {
            _animationDelay--;
            if (_animationDelay == 0)
            {
                _animationDelay = ANIMATION_DELAY;
                if (_animationStage == ANIMATION_STAGES - 1)
                {
                    _flickerAmount++;
                    if (_flickerAmount == ANIMATION_FLICKER_AMOUNT)
                    {
                        Finish();
                    }
                }
                else
                {
                    _animationStage++;
                }
            }
        }
    }
}
