using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PokemonRedux.Content;
using PokemonRedux.Game.Battles;
using System.Linq;
using static Core;

namespace PokemonRedux.Screens.Battles.Animations
{
    class ShinyAnimation : BattleAnimation
    {
        private static readonly int[] INVERTED_FRAMES = new[] { 0, 1, 2, 3, 4, 5 };
        private const int TOTAL_FRAMES = 48;
        private const int FRAMES_PER_ROW = 7;

        private readonly BattlePokemon _target;
        private Texture2D _texture;
        private int _currentFrame = 0;

        public ShinyAnimation(BattlePokemon target)
        {
            _target = target;
        }

        public override void LoadContent()
        {
            _texture = Controller.Content.LoadDirect<Texture2D>("Textures/Battle/shinySparkle.png");
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
                IsFinished = true;
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(false);
            }
            else
            {
                Battle.ActiveBattle.AnimationController.SetScreenColorInvert(INVERTED_FRAMES.Contains(_currentFrame));
            }
        }
    }
}
