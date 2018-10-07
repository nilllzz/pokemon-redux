using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class BiteAnimation : BattleMoveAnimation
    {
        private const float PROGRESS_PER_FRAME = 0.15f;
        private const int TOTAL_STAGES = 3;
        private const int HIT_DELAY = 6;

        private float _progress = 0f;
        private int _stage = 0;
        private int _hitDelay = 0;

        public BiteAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("bite");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);

            if (_hitDelay > 0)
            {
                var hitSize = 24 * Border.SCALE;
                var hitPosition = Vector2.Zero;
                switch (_stage)
                {
                    case 1:
                        hitPosition = new Vector2(center.X, center.Y - hitSize / 2f);
                        break;
                    case 3:
                        hitPosition = new Vector2(center.X - hitSize, center.Y);
                        break;
                }
                batch.Draw(_texture, new Rectangle((int)hitPosition.X, (int)hitPosition.Y, (int)hitSize, (int)hitSize),
                    new Rectangle(0, 10, 24, 24), Color.White);
            }
            var pokemonSize = GetPokemonSpriteSize();

            var upperWidth = 32 * Border.SCALE;
            var upperHeight = 10 * Border.SCALE;
            var upperPosX = center.X - upperWidth / 2f;
            var upperStartPosY = center.Y - pokemonSize / 4f - upperHeight;
            var upperEndPosY = center.Y;
            var upperPosY = upperStartPosY + (upperEndPosY - upperStartPosY) * _progress;

            batch.Draw(_texture, new Rectangle((int)upperPosX, (int)upperPosY, (int)upperWidth, (int)upperHeight),
                new Rectangle(0, 0, 32, 10), Color.White);

            var lowerWidth = 16 * Border.SCALE;
            var lowerHeight = 8 * Border.SCALE;
            var lowerPosX = center.X - lowerWidth / 2f;
            var lowerStartPosY = center.Y + pokemonSize / 2f;
            var lowerEndPosY = upperEndPosY + upperHeight; // right below upper
            var lowerPosY = lowerStartPosY + (lowerEndPosY - lowerStartPosY) * _progress;

            batch.Draw(_texture, new Rectangle((int)lowerPosX, (int)lowerPosY, (int)lowerWidth, (int)lowerHeight),
                new Rectangle(24, 10, 16, 8), Color.White);
        }

        public override void Update()
        {
            if (_hitDelay > 0)
            {
                _hitDelay--;
                if (_hitDelay == 0 && _stage == TOTAL_STAGES)
                {
                    Finish();
                }
            }
            else
            {
                if (_stage % 2 == 0)
                {
                    _progress += PROGRESS_PER_FRAME;
                    if (_progress >= 1f)
                    {
                        _progress = 1f;
                        _hitDelay = HIT_DELAY;
                        _stage++;
                    }
                }
                else
                {
                    _progress -= PROGRESS_PER_FRAME;
                    if (_progress <= 0f)
                    {
                        _progress = 0f;
                        _stage++;
                    }
                }
            }
        }
    }
}
