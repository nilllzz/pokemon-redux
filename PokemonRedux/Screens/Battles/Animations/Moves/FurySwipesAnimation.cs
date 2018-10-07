using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class FurySwipesAnimation : BattleMoveAnimation
    {
        private const int ANIMATION_STAGES = 4;
        private const int ANIMATION_DELAY = 3;
        private const int ANIMATION_FLICKER_AMOUNT = 8;
        private const int ANIMATION_FRAME_SIZE = 48;

        private readonly int _hitAmount;

        private int _animationDelay = ANIMATION_DELAY;
        private int _animationStage = 0;
        private int _flickerAmount = 0;

        public FurySwipesAnimation(BattlePokemon user, BattlePokemon target, int hitAmount)
            : base(user, target)
        {
            _hitAmount = hitAmount;
        }

        public override void LoadContent()
        {
            LoadTexture("furyswipes");
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_flickerAmount % 2 == 0)
            {
                var center = GetCenter(_target.Side);

                var frameSize = ANIMATION_FRAME_SIZE * Border.SCALE;
                var x = (int)(center.X - frameSize / 2f);
                var y = (int)(center.Y - frameSize / 2f);

                var effect = ((_user.Side == PokemonSide.Player ? 0 : 1) + _hitAmount) % 2 == 0 ?
                    SpriteEffects.None : SpriteEffects.FlipHorizontally;

                batch.Draw(_texture, new Rectangle(x, y, (int)frameSize, (int)frameSize),
                    new Rectangle(_animationStage * ANIMATION_FRAME_SIZE, 0, ANIMATION_FRAME_SIZE, ANIMATION_FRAME_SIZE),
                    Color.White, 0f, Vector2.Zero, effect, 0f);
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
