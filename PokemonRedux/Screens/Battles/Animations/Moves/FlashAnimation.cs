using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Game.Battles;
using System.Linq;

namespace PokemonRedux.Screens.Battles.Animations.Moves
{
    class FlashAnimation : BattleMoveAnimation
    {
        private static readonly int[] INVERTED_FRAMES = new[] { 10, 11, 12, 13, 14, 15, 16, 17, 30, 31, 32, 33, 34, 35, 36, 37 };
        private const int TOTAL_FRAMES = 48;
        private const int FRAMES_PER_ROW = 7;

        private int _currentFrame = 0;

        public FlashAnimation(BattlePokemon user, BattlePokemon target)
            : base(user, target)
        { }

        public override void LoadContent()
        {
            LoadTexture("flash");
        }

        public override void Draw(SpriteBatch batch)
        {
            var center = GetCenter(_target.Side);
            var frameSize = 48 * Border.SCALE;
            var posX = (int)(center.X - frameSize / 2f);
            var posY = (int)(center.Y - frameSize / 2f);

            var texX = _currentFrame % FRAMES_PER_ROW;
            var texY = _currentFrame / FRAMES_PER_ROW;

            var effect = _target.Side == PokemonSide.Enemy ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            batch.Draw(_texture, new Rectangle(posX, posY, (int)frameSize, (int)frameSize),
                new Rectangle(texX * 48, texY * 48, 48, 48), Color.White,
                0f, Vector2.Zero, effect, 0f);
        }

        public override void Update()
        {
            _currentFrame++;
            if (_currentFrame > TOTAL_FRAMES)
            {
                Finish();
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
            }
            else
            {
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(INVERTED_FRAMES.Contains(_currentFrame));
            }
        }
    }
}
